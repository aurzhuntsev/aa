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
        DBTP = 0, DBFS = 1
    }

    public class InputOutputLevel
    {
        public double OutputLevel { get; set; } = 3.0;
        public bool MatchInputLevel { get; set; } = true;
        public double InputLevel { get; set; } = 3.0;
        public SignalLevelMode InputLevelMode { get; set; } = SignalLevelMode.DBFS;

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
    public class ThdMeasurement : IMeasurement
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

        private List<ActivityBase> _activities = new List<ActivityBase>();
        public IEnumerable<ActivityBase> Activities => _activities;
        
        public ActivityBase CurrentActivity { get; private set; }

        public string Title { get; private set; }

        public void Run()
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

                _activities.Add(warmUpActivity);
            }

            Title = $"THD - {TestSignalOptions.InputOutputOptions}@{TestSignalOptions.Frequency}hz";

            foreach (var activity in Activities)
            {
                CurrentActivity = activity;
                CurrentActivity.Start();
                CurrentActivity.WaitToComplete();
            }
        }
    }
}
