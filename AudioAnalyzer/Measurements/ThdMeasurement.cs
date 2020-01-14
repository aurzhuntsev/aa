using AudioMark.Core.Common;
using AudioMark.Core.Generators;
using PortAudioWrapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AudioMark.Core.Settings;
using AudioMark.Core.AudioData;
using System.Diagnostics;

namespace AudioMark.Core.Measurements
{
    public enum SignalLevelMode
    {
        dBTP = 0, dBFS = 1
    }

    public class InputOutputLevel
    {
        public double OutputLevel { get; set; } = 3.0;
        public bool MatchInputLevel { get; set; } = true;
        public double InputLevel { get; set; } = 3.0;
        public SignalLevelMode InputLevelMode { get; set; } = SignalLevelMode.dBFS;

        public override string ToString()
        {
            if (MatchInputLevel)
            {
                return $"In {InputLevel}{InputLevelMode}";
            }
            return $"Out {OutputLevel}dBTP";
        }
    }

    [Measurement("Total Harmonic Distortion")]
    public class ThdMeasurement : MeasurementBase<SpectralData>
    {       
        public class SignalOptions
        {
            public double Frequency { get; set; } = 1000.0;
            public InputOutputLevel InputOutputOptions { get; set; } = new InputOutputLevel();
        }

        public SignalOptions TestSignalOptions { get; set; } = new SignalOptions();
        public SineGenerator TestSignalGenerator { get; set; }

        public bool WarmUpEnabled { get; set; } = true;
        public bool OverrideWarmUpSignalOptions { get; set; } = false;
        public int WarmUpDurationSeconds { get; set; } = 10;
        public SignalOptions WarmUpSignalOptions { get; set; } = new SignalOptions();
        public SineGenerator WarmupSignalGenerator { get; set; }

        public OverridableSettings<StopConditions> StopConditions { get; set; } = new OverridableSettings<StopConditions>(AppSettings.Current.StopConditions);
        public OverridableSettings<Fft> Fft { get; set; } = new OverridableSettings<Fft>(AppSettings.Current.Fft);

        /* TODO: This should actually be refactored:
         * A single generation task with a current generator logic
         * A single data read event
         * Changing activities should not stop generation/acquisition to avoid buffer population latency 
         */
        protected override void Initialize()
        {
            RegisterActivity(CreateSetupActivity());

            TestSignalGenerator = new SineGenerator(AppSettings.Current.Device.SampleRate, TestSignalOptions.Frequency);
            if (!OverrideWarmUpSignalOptions)
            {
                WarmupSignalGenerator = TestSignalGenerator;
            }
            else
            {
                WarmupSignalGenerator = new SineGenerator(AppSettings.Current.Device.SampleRate, WarmUpSignalOptions.Frequency);
            }

            if (TestSignalOptions.InputOutputOptions.MatchInputLevel)
            {
                /* TODO: Add input level tune activity */
            }

            if (WarmUpEnabled && WarmUpDurationSeconds > 0)
            {
                RegisterActivity(CreateWarmUpActivity());
            }

            RegisterActivity(CreateAcquisitionActivity());

            Title = $"THD - {TestSignalOptions.InputOutputOptions}@{TestSignalOptions.Frequency}hz";
        }

        private Activity<SpectralData> CreateSetupActivity()
        {
            const int SetupActivityTimeoutMilliseconds = 1000;

            var setupActivity = new Activity<SpectralData>("Setting up...");
            setupActivity.AddGenerator(AppSettings.Current.Device.PrimaryOutputChannel, new SilenceGenerator());

            var sink = new SpectralDataProcessor(Fft.Value.WindowSize, Fft.Value.WindowOverlapFactor, AppSettings.Current.Device.SampleRate / 2)
            {
                Silent = true
            };
            setupActivity.AddSink(AppSettings.Current.Device.PrimaryInputChannel, sink);
            setupActivity.RegisterStopCondition(new TimeoutStopCondition(SetupActivityTimeoutMilliseconds));            

            return setupActivity;
        }

        private Activity<SpectralData> CreateWarmUpActivity()
        {
            var warmUpActivity = new Activity<SpectralData>("Warming up...");
            warmUpActivity.AddGenerator(AppSettings.Current.Device.PrimaryOutputChannel, WarmupSignalGenerator);
            
            var sink = new SpectralDataProcessor(Fft.Value.WindowSize, Fft.Value.WindowOverlapFactor, AppSettings.Current.Device.SampleRate / 2);
            sink.Data.DefaultValueSelector = (data) => data.LastValue;
            warmUpActivity.AddSink(AppSettings.Current.Device.PrimaryInputChannel, sink);

            warmUpActivity.RegisterStopCondition(new TimeoutStopCondition(WarmUpDurationSeconds * 1000));

            return warmUpActivity;
        }

        /* TODO: Test and Warmup activities are the same, refactor */
        private Activity<SpectralData> CreateAcquisitionActivity()
        {
            var dataActivity = new Activity<SpectralData>("Acquiring data...");
            dataActivity.AddGenerator(AppSettings.Current.Device.PrimaryOutputChannel, TestSignalGenerator);

            var sink = new SpectralDataProcessor(Fft.Value.WindowSize, Fft.Value.WindowOverlapFactor, AppSettings.Current.Device.SampleRate / 2)
            {
                Silent = true
            };
            sink.Data.DefaultValueSelector = (data) => data.Mean;
            dataActivity.AddSink(AppSettings.Current.Device.PrimaryInputChannel,
                new SpectralDataProcessor(Fft.Value.WindowSize, Fft.Value.WindowOverlapFactor, AppSettings.Current.Device.SampleRate / 2));
            
            if (StopConditions.Value.TimeoutEnabled)
            {
                dataActivity.RegisterStopCondition(new TimeoutStopCondition(StopConditions.Value.Timeout * 1000));
            }
            if (StopConditions.Value.ToleranceMatchingEnabled)
            {
                dataActivity.RegisterStopCondition(new ToleranceAchievedStopCondition(sink.Data, StopConditions.Value.Tolerance, StopConditions.Value.Confidence));
            }
            
            return dataActivity;
        }
    }
}
