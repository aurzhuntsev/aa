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
        
        protected override void Initialize()
        {            
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
                var warmUpActivity = new GeneratorActivity("Warming up...");
                warmUpActivity.AddGenerator(AppSettings.Current.Device.PrimaryOutputChannel, WarmupSignalGenerator);
                warmUpActivity.AddStopCondition(new TimeoutStopCondition(WarmUpDurationSeconds * 1000));

                var warmUpData = new SpectralData(AppSettings.Current.Fft.WindowSize, AppSettings.Current.Device.SampleRate / 2.0);
                var warmUpDataProcessor = new SpectralDataProcessor(AppSettings.Current.Fft.WindowSize, AppSettings.Current.Fft.WindowOverlapFactor)
                {
                    OnItemProcessed = (data) =>
                    {
                        warmUpData.Set(data);
                        InvokeDataUpdate(warmUpData);
                    }                    
                };

                int discardCount = 0;
                warmUpActivity.OnRead += (buffer) =>
                {
                    if (discardCount < AppSettings.Current.Device.SampleRate / 2)
                    {
                        discardCount++;
                    }
                    else
                    {
                        warmUpDataProcessor.Add(buffer[AppSettings.Current.Device.PrimaryInputChannel - 1]);
                    }
                };

                Activities.Add(warmUpActivity);
            }

            Title = $"THD - {TestSignalOptions.InputOutputOptions}@{TestSignalOptions.Frequency}hz";
        }
    }
}
