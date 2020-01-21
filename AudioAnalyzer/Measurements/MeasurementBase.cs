using AudioMark.Core.AudioData;
using AudioMark.Core.Common;
using AudioMark.Core.Generators;
using AudioMark.Core.Settings;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AudioMark.Core.Measurements
{
    /* TODO: Refactor a bit */
    public abstract class MeasurementBase<TResult> : IMeasurement<TResult>
    {
        private readonly int DataSamplesToDiscard = AppSettings.Current.Device.SampleRate;

        protected IAudioDataAdapter _adapter;

        public IGenerator[] Generators { get; protected set; }

        public IDataSink<TResult>[] DataSinks { get; }

        private List<Activity<TResult>> _activities = new List<Activity<TResult>>();
        public IEnumerable<Activity<TResult>> Activities
        {
            get => _activities;
        }

        public Activity<TResult> CurrentActivity { get; protected set; }

        private bool _allActivitiesCompleted = false;
        protected volatile bool _running;
        public bool Running
        {
            get => _running;
        }        

        public string CurrentActivityDescription
        {
            get => CurrentActivity?.Description;
        }

        public int CurrentActivityIndex
        {
            get
            {
                if (CurrentActivity != null)
                {
                    return _activities.IndexOf(CurrentActivity);
                }
                return 0;
            }
        }

        public int ActivitiesCount
        {
            get => _activities.Count;
        }

        public int CompletedActivitiesCount
        {
            get; private set;
        }

        public TimeSpan? Remaining
        {
            get => CurrentActivity?.Remaining;
        }

        private DateTime _currentActivityStartedAt;
        public TimeSpan Elapsed
        {
            get => DateTime.Now.Subtract(_currentActivityStartedAt).Duration();
        }

        public string Name { get; set; }

        public IAnalysisResult AnalysisResult { get; protected set; }

        public abstract TResult Data { get; }
        public IMeasurementSettings Settings { get; private set; }

        private int _dataDiscardCounter = 0;
        private DateTime _lastStopConditionsChecked;
        private readonly object _stopConditionCheckSync = new object();

        private EventWaitHandle _activityWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        public event EventHandler<object> OnDataUpdate;
        public event EventHandler<bool> OnComplete;
        public event EventHandler<Exception> OnError;
        public event EventHandler<IAnalysisResult> OnAnalysisComplete;

        public MeasurementBase(IMeasurementSettings settings)
        {
            _adapter = AudioDataAdapterProvider.Get();

            Generators = new IGenerator[AppSettings.Current.Device.OutputDevice.ChannelsCount];
            DataSinks = new IDataSink<TResult>[AppSettings.Current.Device.InputDevice.ChannelsCount];

            Settings = settings;
        }

        public async Task Run()
        {
            _activities.Clear();
            AnalysisResult = null;

            Initialize();            

            _running = true;

            _adapter.SetWriteHandler(OnAdapterWrite);
            _adapter.SetReadHandler(OnAdapterRead);
            _adapter.Start();
            
            await Task.Run(() =>
            {
                var index = 0;

                _allActivitiesCompleted = false;
                for (index = 0; index < _activities.Count; index++)
                {
                    if (!_running)
                    {
                        break;
                    }

                    var activity = _activities[index];
                    _dataDiscardCounter = 0;

                    for (var i = 0; i < Generators.Length; i++)
                    {
                        if (activity.Generators.ContainsKey(i))
                        {
                            Generators[i] = activity.Generators[i];
                        }
                        else
                        {
                            Generators[i] = null;
                        }
                    }

                    for (var i = 0; i < DataSinks.Length; i++)
                    {
                        if (activity.DataSinks.ContainsKey(i))
                        {
                            DataSinks[i] = activity.DataSinks[i];
                            DataSinks[i].OnItemProcessed += OnItemProcessed;
                        }
                        else
                        {
                            DataSinks[i] = null;
                        }
                    }

                    _currentActivityStartedAt = DateTime.Now;
                    CurrentActivity = activity;
                    CurrentActivity.SetStopConditions();

                    _activityWaitHandle.WaitOne();
                    _activityWaitHandle.Reset();

                }

                if (CompletedActivitiesCount == _activities.Count)
                {                    
                    AnalysisResult = Analyze();
                    OnAnalysisComplete?.Invoke(this, AnalysisResult);
                }

                StopInternal(false);
            });
        }

        private void OnItemProcessed(object sender, TResult e)
        {
            OnDataUpdate?.Invoke(this, e);
        }

        public void Stop()
        {
            StopInternal(true);
        }

        private void StopInternal(bool interrupted)
        {
            if (_running)
            {
                _running = false;
                _adapter.Stop();
                _activityWaitHandle.Set();

                OnComplete?.Invoke(this, !interrupted);
            }
        }

        public void RegisterActivity(Activity<TResult> activity)
        {
            activity.OnComplete += (sender, e) =>
            {
                CompletedActivitiesCount++;
                _activityWaitHandle.Set();
            };

            activity.OnError += (sender, e) =>
            {
                OnError?.Invoke(this, new Exception("Activity has failed.", e));
                _running = false;

                _activityWaitHandle.Set();
            };

            _activities.Add(activity);
        }

        protected abstract void Initialize();
        protected abstract IAnalysisResult Analyze();

        protected void InvokeDataUpdate(TResult data)
        {
            OnDataUpdate.Invoke(this, data);
        }

        private int OnAdapterWrite(IAudioDataAdapter sender, double[] buffer, bool discard)
        {
            if (!_running)
            {
                return 0;
            }

            for (var channel = 0; channel < Generators.Length; channel++)
            {
                if (Generators[channel] != null)
                {
                    buffer[channel] = Generators[channel].Next();
                }
            }

            /* TODO: fix return semantic */
            return buffer.Length;
        }

        private void OnAdapterRead(IAudioDataAdapter sender, double[] buffer, int length, bool discard)
        {
            if (!_running)
            {
                return;
            }

            if (CurrentActivity != null &&
                DateTime.Now.Subtract(_lastStopConditionsChecked).Duration().TotalMilliseconds >= AppSettings.Current.StopConditions.CheckIntervalMilliseconds)
            {
                _lastStopConditionsChecked = DateTime.Now;

                Task.Run(() =>
                {
                    lock (_stopConditionCheckSync)
                    {
                        CurrentActivity.CheckStopConditions();
                    }
                });
            }

            if (_dataDiscardCounter < DataSamplesToDiscard)
            {
                _dataDiscardCounter++;
            }
            else
            {
                for (var channel = 0; channel < DataSinks.Length; channel++)
                {
                    if (DataSinks[channel] != null)
                    {
                        DataSinks[channel].Add(buffer[channel]);
                    }
                }
            }
        }
    }
}
