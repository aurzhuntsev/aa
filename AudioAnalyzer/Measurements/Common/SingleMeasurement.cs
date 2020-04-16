using AudioMark.Core.AudioData;
using AudioMark.Core.Common;
using AudioMark.Core.Fft;
using AudioMark.Core.Generators;
using AudioMark.Core.Measurements.Analysis;
using AudioMark.Core.Measurements.Settings.Common;
using AudioMark.Core.Measurements.StopConditions;
using AudioMark.Core.Settings;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioMark.Core.Measurements.Common
{
    public abstract class SingleMeasurement : MeasurementBase
    {
        private enum Phase
        {
            AwaitingInput, WarmUp, Gathering
        }

        private SpectrumProcessor _processor = null;
        public override Spectrum Result => _processor?.Data;

        protected IAudioDataAdapter _adapter;

        private Dictionary<int, IGenerator> _generators = new Dictionary<int, IGenerator>();
        public ImmutableDictionary<int, IGenerator> Generators => _generators.ToImmutableDictionary();

        private Dictionary<int, SpectrumProcessor> _sinks = new Dictionary<int, SpectrumProcessor>();
        public ImmutableDictionary<int, SpectrumProcessor> Sinks => _sinks.ToImmutableDictionary();

        private List<IStopCondition> _stopConditions = new List<IStopCondition>();
        public ImmutableList<IStopCondition> StopConditions => _stopConditions.ToImmutableList();

        private Dictionary<int, bool> _testSignalPresentMap = new Dictionary<int, bool>();

        private readonly object _processingSync = new object();

        private DateTime _inputSignalReceivedAt;
        private Phase _phase = Phase.AwaitingInput;

        private volatile bool _enableInput = false;

        public override TimeSpan? Remaining
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


        public SingleMeasurement(IMeasurementSettings settings) : base(settings)
        {
            Initialize();
        }

        internal SingleMeasurement(IMeasurementSettings settings, IAnalysisResult result) : base(settings, result)
        {
            Initialize();
        }

        public override void Update()
        {
            ApplyCorrectionProfile(Result);

            var analytics = GetAnalytics();
            AnalysisResult = analytics.Analyze(_processor.Data, Settings);
        }

        protected override void RunInternal()
        {
            _enableInput = true;
            RunAdapter();

            /* TODO: Implement non-warmable case */
            ActivitiesCount = 3;
        }

        protected override void StopInternal(bool interrupted)
        {
            _adapter.Stop();
        }

        protected void RunAdapter()
        {
            _adapter = AudioDataAdapterProvider.Get();

            _adapter.SetWriteHandler(OnAdapterWrite);
            _adapter.SetReadHandler(OnAdapterRead);

            if (!_adapter.Running)
            {
                _adapter.Start();
            }
            else
            {
                _adapter.ResetBuffers();
            }
        }

        protected void ApplyStopConditions()
        {
            if (Settings is IGlobalOptions source)
            {
                if (source.StopConditions.Value.TimeoutEnabled)
                {
                    RegisterStopCondition(new TimeoutStopCondition(source.StopConditions.Value.Timeout * 1000));
                }
                if (source.StopConditions.Value.ToleranceMatchingEnabled)
                {
                    RegisterStopCondition(new ToleranceAchievedStopCondition(_processor.Data, source.StopConditions.Value.Tolerance, source.StopConditions.Value.Confidence));
                }
            }
        }

        protected void ApplyCorrectionProfile(Spectrum target)
        {
            if (Settings is ICorrectionProfile correctionProfile)
            {
                if (correctionProfile.ApplyCorrectionProfile && correctionProfile.CorrectionProfile != null)
                {
                    target.SetCorrectionProfile(correctionProfile.CorrectionProfile, IsCorrectionApplicable);
                }
                else
                {
                    target.SetCorrectionProfile(null, null);
                }
            }
        }

        protected virtual bool IsCorrectionApplicable(Spectrum data, int index)
        {
            return true;
        }

        protected override bool CheckSignalPresent(Spectrum data)
        {
            if (Settings is ITestSignal testSignal)
            {
                var signalValue = -data.ValueAtFrequency(testSignal.TestSignalOptions.Frequency, x => x.LastValue, testSignal.WindowHalfSize).ToDbTp();
                return signalValue >= testSignal.SignalDetectionThresholdDb;
            }

            return true;
        }

        protected void CheckStopConditions()
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
                Stop(false);
                Update();
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
            _testSignalPresentMap[channel] = false;
        }

        private void Initialize()
        {
            RegisterGenerator(AppSettings.Current.Device.PrimaryOutputChannel, GetGenerator());

            _processor = new SpectrumProcessor(AppSettings.Current.Fft.WindowSize,
                                               AppSettings.Current.Fft.WindowOverlapFactor,
                                               (int)(AppSettings.Current.Device.SampleRate * 0.5),
                                               AppSettings.Current.Fft.WindowFunction);

            RegisterSink(AppSettings.Current.Device.PrimaryInputChannel, _processor);

            ApplyStopConditions();
            ApplyCorrectionProfile(_processor.Data);
        }

        private void OnItemProcessed(object sender, Spectrum e)
        {
            lock (_processingSync)
            {
                var sink = sender as SpectrumProcessor;
                OnDataUpdate(e);

                if (_phase == Phase.AwaitingInput)
                {
                    ProcessAwaitInput(sink);
                }
                else if (_phase == Phase.WarmUp)
                {
                    ProcessWarmUp(sink);
                }
                else if (_phase == Phase.Gathering)
                {
                    ProcessGathering(sink);
                }
            }
        }

        private void ProcessAwaitInput(SpectrumProcessor sink)
        {
            CurrentActivityDescription = "Awaiting signal...";

            var allInputSingnalsPresent = _testSignalPresentMap.Values.All(v => v == true);
            if (!allInputSingnalsPresent)
            {
                var expectedLoopbackDelay = Math.Max(
                    (AppSettings.Current.Device.InputDevice.LatencyMilliseconds + AppSettings.Current.Device.OutputDevice.LatencyMilliseconds),
                    2.0 * 1000.0 * Settings.Fft.Value.WindowSize / AppSettings.Current.Device.SampleRate
                );

                if (Elapsed.TotalMilliseconds > expectedLoopbackDelay)
                {
                    var channel = Sinks.First(s => s.Value == sink).Key;
                    _testSignalPresentMap[channel] = CheckSignalPresent(sink.Data);

                    if (!_testSignalPresentMap[channel])
                    {
                        var channels = _testSignalPresentMap.Where(m => m.Value == false).Select(m => m.Key.ToString()).OrderBy(k => k);

                        var message = $"No test signal detected in channels {channels.Aggregate((a, b) => (a + ", " + b))}.";
                        OnError(new Exception(message));
                        Stop(true);

                        return;
                    }


                    _inputSignalReceivedAt = DateTime.Now;
                    if (Settings is IWarmable warmable)
                    {
                        _phase = Phase.WarmUp;
                    }
                    else
                    {
                        SetGatheringPhase(sink);
                    }

                    sink.Data.Reset();
                }                
            }
        }

        private void ProcessWarmUp(SpectrumProcessor sink)
        {
            sink.Data.DefaultValue = Spectrum.DefaultValueType.Last;
            CurrentActivityDescription = "Warming up...";
            CurrentActivityIndex = 1;

            var warmable = Settings as IWarmable;
            var warmUpDurationSeconds = warmable.WarmUpEnabled ? warmable.WarmUpDurationSeconds : 0;
            if (DateTime.Now.Subtract(_inputSignalReceivedAt).Duration().TotalSeconds >= warmUpDurationSeconds)
            {
                SetGatheringPhase(sink);
            }
        }

        private void SetGatheringPhase(SpectrumProcessor sink)
        {
            SetStopConditions();
            sink.Data.Reset();
            sink.Data.DefaultValue = Spectrum.DefaultValueType.Mean;

            _phase = Phase.Gathering;
        }

        private void ProcessGathering(SpectrumProcessor sink)
        {
            CheckStopConditions();

            CurrentActivityDescription = "Gathering data...";
            CurrentActivityIndex = 2;
        }

        private void OnAdapterWrite(object sender, AudioDataEventArgs args)
        {
            if (!_running)
            {
                return;
            }

            if (_phase != Phase.AwaitingInput && args.Discard)
            {
                OnError(new Exception("Unable to generate signal on time."));
                Stop(true);
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

            if (_phase != Phase.AwaitingInput && args.Discard)
            {
                OnError(new Exception("Unable to process signal on time."));
                Stop(true);
                return;
            }

            foreach (var channel in Sinks.Keys)
            {
                for (var frame = 0; frame < args.Frames; frame++)
                {
                    Sinks[channel].Add(args.Buffer[frame * args.Channels + channel - 1]);
                }
            }

        }

        protected abstract IGenerator GetGenerator();
        protected abstract IAnalytics GetAnalytics();
    }
}
