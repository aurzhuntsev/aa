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
    public class ThdMeasurement : MeasurementBase
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

        /* TODO: This should actually be refactored:
         * A single generation task with a current generator logic
         * A single data read event
         * Changing activities should not stop generation/acquisition to avoid buffer population latency 
         */
        protected override void Initialize()
        {
            Activities.Add(CreateSetupActivity());

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
                Activities.Add(CreateWarmUpActivity());
            }

            Activities.Add(CreateAcquisitionActivity());

            Title = $"THD - {TestSignalOptions.InputOutputOptions}@{TestSignalOptions.Frequency}hz";
        }

        private ActivityBase CreateSetupActivity()
        {
            const int SetupActivityTimeoutMilliseconds = 1000;

            var setupActivity = new GeneratorActivity("Setting up...");
            setupActivity.AddGenerator(AppSettings.Current.Device.PrimaryOutputChannel, new SilenceGenerator());
            setupActivity.AddStopCondition(new TimeoutStopCondition(SetupActivityTimeoutMilliseconds));

            var setupData = new SpectralData(1, (int)(AppSettings.Current.Device.SampleRate / 2.0));
            var setupDataProcessor = new SpectralDataProcessor(AppSettings.Current.Fft.WindowSize, AppSettings.Current.Fft.WindowOverlapFactor);

            setupActivity.OnRead += (buffer, discard) =>
            {
                setupDataProcessor.Add(buffer[AppSettings.Current.Device.PrimaryInputChannel - 1]);
            };

            return setupActivity;
        }

        private ActivityBase CreateWarmUpActivity()
        {
            var warmUpActivity = new GeneratorActivity("Warming up...");
            warmUpActivity.AddGenerator(AppSettings.Current.Device.PrimaryOutputChannel, WarmupSignalGenerator);
            warmUpActivity.AddStopCondition(new TimeoutStopCondition(WarmUpDurationSeconds * 1000));

            var warmUpData = new SpectralData(AppSettings.Current.Fft.WindowSize, (int)(AppSettings.Current.Device.SampleRate / 2.0))
            {
                DefaultValueSelector = (x) => x.LastValue
            };

            int discardCount = 0;
            var warmUpDataProcessor = new SpectralDataProcessor(AppSettings.Current.Fft.WindowSize, AppSettings.Current.Fft.WindowOverlapFactor)
            {
                OnItemProcessed = (data) =>
                {
                    warmUpData.Set(data);
                    InvokeDataUpdate(warmUpData);
                }
            };

            warmUpActivity.OnRead += (buffer, discard) =>
            {
                /* TODO: Shoud actually discard one (?) buffer */
                if (discardCount < AppSettings.Current.Device.SampleRate / 2)
                {
                    discardCount++;
                    return;
                }

                if (!discard)
                {
                    warmUpDataProcessor.Add(buffer[AppSettings.Current.Device.PrimaryInputChannel - 1]);
                }
                else
                {
                    warmUpDataProcessor.Reset();
                }
            };

            return warmUpActivity;
        }

        /* TODO: Test and Warmup activities are the same, refactor */
        private ActivityBase CreateAcquisitionActivity()
        {
            var dataActivity = new GeneratorActivity("Acquiring data...");
            dataActivity.AddGenerator(AppSettings.Current.Device.PrimaryOutputChannel, TestSignalGenerator);

            /* TODO: Implement actual stop conditions */
            if (StopConditions.Value.TimeoutEnabled)
            {
                dataActivity.AddStopCondition(new TimeoutStopCondition(WarmUpDurationSeconds * 1000));
            }

            var data = new SpectralData(AppSettings.Current.Fft.WindowSize, (int)(AppSettings.Current.Device.SampleRate / 2.0))
            {
                DefaultValueSelector = (x) => x.Mean
            };

            if (StopConditions.Value.ToleranceMatchingEnabled)
            {
                dataActivity.AddStopCondition(new ToleranceAchievedStopCondition(data, StopConditions.Value.Tolerance, StopConditions.Value.Confidence));
            }

            int discardCount = 0;
            var dataProcessor = new SpectralDataProcessor(AppSettings.Current.Fft.WindowSize, AppSettings.Current.Fft.WindowOverlapFactor)
            {
                OnItemProcessed = (item) =>
                {
                    data.Set(item);
                    InvokeDataUpdate(data);
                }
            };

            dataActivity.OnRead += (buffer, discard) =>
            {
                /* TODO: Shoud actually discard one (?) buffer */
                if (discardCount < AppSettings.Current.Device.SampleRate)
                {
                    discardCount++;
                    return;
                }

                if (!discard)
                {
                    dataProcessor.Add(buffer[AppSettings.Current.Device.PrimaryInputChannel - 1]);
                }
                else
                {
                    dataProcessor.Reset();
                }
            };

            return dataActivity;
        }
    }
}
