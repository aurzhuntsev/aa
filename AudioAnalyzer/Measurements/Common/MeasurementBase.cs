using AudioMark.Core.AudioData;
using AudioMark.Core.Common;
using AudioMark.Core.Generators;
using AudioMark.Core.Measurements.Analysis;
using AudioMark.Core.Measurements.Settings.Common;
using AudioMark.Core.Measurements.StopConditions;
using AudioMark.Core.Settings;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AudioMark.Core.Measurements.Common
{
    public abstract class MeasurementBase : IMeasurement
    {
        protected IAudioDataAdapter _adapter;

        private Dictionary<int, IGenerator> _generators = new Dictionary<int, IGenerator>();
        public ImmutableDictionary<int, IGenerator> Generators => _generators.ToImmutableDictionary();

        private Dictionary<int, SpectrumProcessor> _sinks = new Dictionary<int, SpectrumProcessor>();
        public ImmutableDictionary<int, SpectrumProcessor> Sinks => _sinks.ToImmutableDictionary();

        private List<IStopCondition> _stopConditions = new List<IStopCondition>();
        public ImmutableList<IStopCondition> StopConditions => _stopConditions.ToImmutableList();

        protected volatile bool _running;
        public bool Running
        {
            get => _running;
        }

        private TaskCompletionSource<bool> _completionSource = null;

        public string CurrentActivityDescription { get; protected set; }
        public int CurrentActivityIndex { get; protected set; }
        public int ActivitiesCount { get; protected set; }

        public TimeSpan? Remaining
        {
            get
            {
                if (!_stopConditions.Any() || _stopConditions.Any(stopCondition => !stopCondition.Remaining.HasValue))
                {
                    return null;
                }

                return _stopConditions.Min(stopCondition => stopCondition.Remaining.Value);
            }
        }

        private DateTime _startedAt;
        public TimeSpan Elapsed
        {
            get => DateTime.Now.Subtract(_startedAt).Duration();
        }

        public string Name { get; set; }
        public virtual Spectrum Result { get; }

        public IMeasurementSettings Settings { get; private set; }        
        public IAnalysisResult AnalysisResult { get; protected set; }

        private DateTime _lastStopConditionsChecked;
        private readonly object _stopConditionCheckSync = new object();
        public bool EnableStopConditions { get; protected set; }

        public event EventHandler<Spectrum> OnDataUpdate;
        public event EventHandler<bool> OnComplete;
        public event EventHandler<Exception> OnError;

        private int _discardReads = 0;

        private MeasurementBase()
        {
            _adapter = AudioDataAdapterProvider.Get();
        }

        public MeasurementBase(IMeasurementSettings settings) : this()
        {
            Settings = settings;
        }

        internal MeasurementBase(IMeasurementSettings settings, IAnalysisResult result) : this(settings)
        {
            AnalysisResult = result;
        }

        public async Task Run()
        {            
            AnalysisResult = null;

            _running = true;
            _completionSource = new TaskCompletionSource<bool>();
            _discardReads = 0;

            _adapter.SetWriteHandler(OnAdapterWrite);
            _adapter.SetReadHandler(OnAdapterRead);

            _startedAt = DateTime.Now;
            if (!_adapter.Running)
            {
                _adapter.Start();
            }
            else
            {
                _adapter.ResetBuffers();
            }

            RunInternal();

            await _completionSource.Task;
        }

        protected abstract void RunInternal();

        protected void CheckStopConditions()
        {
            if (EnableStopConditions)
            {
                bool met = false;
                foreach (var stopCondition in StopConditions)
                {
                    met = stopCondition.Check();
                    if (met)
                    {
                        break;
                    }
                }

                if (met)
                {
                    StopInternal(false);
                    Update();
                }
            }
        }

        protected void RegisterStopCondition(IStopCondition stopCondition)
        {
            _stopConditions.Add(stopCondition);
        }

        protected void SetStopConditions()
        {
            foreach (var stopCondition in _stopConditions)
            {
                stopCondition.Set();
            }
        }

        protected void RegisterGenerator(int channel, IGenerator generator)
        {
            _generators[channel] = generator;
        }

        protected void RegisterSink(int channel, SpectrumProcessor sink)
        {
            sink.OnItemProcessed += OnItemProcessed;
            _sinks[channel] = sink;
        }

        private void OnItemProcessed(object sender, Spectrum e)
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
                _completionSource.SetResult(interrupted);

                OnComplete?.Invoke(this, !interrupted);
            }
        }

        private void OnAdapterWrite(object sender, AudioDataEventArgs args)
        {
            if (!_running)
            {
                return;
            }

            for (var frame = 0; frame < args.Frames; frame++)
            {
                foreach (var channel in _generators.Keys)
                {
                    args.Buffer[frame * args.Channels + channel - 1] = !args.Discard ? Generators[channel].Next() : 0.0;
                }
            }
        }

        private void OnAdapterRead(object sender, AudioDataEventArgs args)
        {
            if (!_running)
            {
                return;
            }

            if (args.Discard || Elapsed.TotalMilliseconds < 1000.0)
            {
                return;
            }

            if (DateTime.Now.Subtract(_lastStopConditionsChecked).Duration().TotalMilliseconds >= AppSettings.Current.StopConditions.CheckIntervalMilliseconds)
            {
                _lastStopConditionsChecked = DateTime.Now;

                Task.Run(() =>
                {
                    lock (_stopConditionCheckSync)
                    {
                        CheckStopConditions();
                    }
                });
            }

            for (var frame = 0; frame < args.Frames; frame++)
            {
                foreach (var channel in Sinks.Keys)
                {
                    Sinks[channel].Add(args.Buffer[frame * args.Channels + channel - 1]);
                }
            }            
        }

        public abstract void Update();
    }
}
