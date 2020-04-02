﻿using AudioMark.Core.AudioData;
using AudioMark.Core.Common;
using AudioMark.Core.Generators;
using AudioMark.Core.Measurements.Analysis;
using AudioMark.Core.Measurements.Settings.Common;
using AudioMark.Core.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AudioMark.Core.Measurements.Common
{
    /* TODO: Refactor a bit */
    public abstract class MeasurementBase<TResult> : IMeasurement<TResult>
    {
        private int DataSamplesToDiscard => AppSettings.Current.Device.SampleRate * 2;

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

        public abstract TResult Result { get; }
        public IMeasurementSettings Settings { get; private set; }

        private int _dataDiscardCounter = 0;
        private DateTime _lastStopConditionsChecked;
        private readonly object _stopConditionCheckSync = new object();

        private EventWaitHandle _activityWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

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

        internal MeasurementBase(IMeasurementSettings settings, IAnalysisResult result)
        {
            Settings = settings;
            AnalysisResult = result;
        }

        public async Task Run()
        {
            _activities.Clear();
            AnalysisResult = null;

            Initialize();

            _running = true;

            _adapter.SetWriteHandler(OnAdapterWrite);
            _adapter.SetReadHandler(OnAdapterRead);

            await Task.Run(() =>
            {
                var index = 0;

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

                    if (!_adapter.Running)
                    {
                        _adapter.Start();
                    }
                    else
                    {
                        _adapter.ResetBuffers();
                    }

                    _activityWaitHandle.WaitOne();                    
                }

                if (CompletedActivitiesCount == _activities.Count)
                {
                    UpdateAnalysisResult();
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

        protected void InvokeDataUpdate(TResult data)
        {
            OnDataUpdate.Invoke(this, data);
        }

        private void OnAdapterWrite(object sender, AudioDataEventArgs args)
        {
            if (!_running)
            {
                return;
            }

            for (var frame = 0; frame < args.Frames; frame++)
            {
                for (var channel = 0; channel < Generators.Length; channel++)
                {
                    if (Generators[channel] != null)
                    {
                        args.Buffer[frame * args.Channels + channel] = Generators[channel].Next();
                    }
                }
            }
        }

        private void OnAdapterRead(object sender, AudioDataEventArgs args)
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

            //if (_dataDiscardCounter < DataSamplesToDiscard)
            //{
            //    _dataDiscardCounter += args.Length;
            //}
            //else
            //{
                for (var frame = 0; frame < args.Frames; frame++)
                {
                    for (var channel = 0; channel < DataSinks.Length; channel++)
                    {
                        if (DataSinks[channel] != null)
                        {
                            DataSinks[channel].Add(args.Buffer[frame * args.Channels + channel]);
                        }
                    }
                }
            //}
        }

        public abstract void UpdateAnalysisResult();
    }
}
