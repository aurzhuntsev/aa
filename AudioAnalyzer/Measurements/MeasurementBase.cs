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
    public interface IMeasurement
    {
        string Title { get; }

        int CurrentActivityIndex { get; }
        string CurrentActivityDescription { get; }
        int ActivitiesCount { get; }
        TimeSpan? Remaining { get; }
        TimeSpan Elapsed { get; }
        bool Running { get; }

        event EventHandler<object> OnDataUpdate;
        event EventHandler OnComplete;
        event EventHandler<Exception> OnError;

        Task Run();
        void Stop();
    }

    public abstract class MeasurementBase<TResult> : IMeasurement
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

        public TimeSpan? Remaining
        {
            get => CurrentActivity?.Remaining;
        }

        private DateTime _currentActivityStartedAt;
        public TimeSpan Elapsed
        {
            get => DateTime.Now.Subtract(_currentActivityStartedAt).Duration();
        }

        public string Title { get; protected set; }

        private int _dataDiscardCounter = 0;
        private DateTime _lastStopConditionsChecked;
        private readonly object _stopConditionCheckSync = new object();

        private EventWaitHandle _activityWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        public event EventHandler<object> OnDataUpdate;
        public event EventHandler OnComplete;
        public event EventHandler<Exception> OnError;

        public MeasurementBase()
        {
            _adapter = AudioDataAdapterProvider.Create();
            _adapter.OnWrite = OnAdapterWrite;
            _adapter.OnRead = OnAdapterRead;

            Generators = new IGenerator[AppSettings.Current.Device.OutputDevice.ChannelsCount];
            DataSinks = new IDataSink<TResult>[AppSettings.Current.Device.InputDevice.ChannelsCount];
        }

        public async Task Run()
        {
            Initialize();

            _running = true;
            _adapter.Start();
            await Task.Run(() =>
            {
                foreach (var activity in _activities)
                {
                    if (!_running)
                    {
                        break;
                    }

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

                OnComplete?.Invoke(this, null);
            });
        }

        private void OnItemProcessed(object sender, TResult e)
        {
            OnDataUpdate?.Invoke(this, e);
        }

        public void Stop()
        {
            _running = false;
            _adapter.Stop();
            _activityWaitHandle.Set();

            OnComplete?.Invoke(this, null);
        }

        public void RegisterActivity(Activity<TResult> activity)
        {
            activity.OnComplete += (sender, e) =>
            {
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
