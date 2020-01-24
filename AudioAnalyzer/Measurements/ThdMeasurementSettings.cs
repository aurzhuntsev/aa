using AudioMark.Core.Generators;
using AudioMark.Core.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements
{
    [Serializable]
    public class ThdMeasurementSettings : IMeasurementSettings
    {
        [Serializable]
        public class SignalOptions
        {
            public double Frequency { get; set; } = 1000.0;
            public InputOutputLevel InputOutputOptions { get; set; } = new InputOutputLevel();
        }

        [Serializable]
        public class ThdAnalysisOptions
        {
            public int WindowHalfSize { get; set; } = 0;

            public bool LimitMaxHarmonics { get; set; } = true;
            public int MaxHarmonics { get; set; } = 10;

            public bool LimitMaxFrequency { get; set; } = false;
            public double MaxFrequency { get; set; } = 20000.0;
        }

        public SignalOptions TestSignalOptions { get; set; } = new SignalOptions();
        
        public bool WarmUpEnabled { get; set; } = true;
        public bool OverrideWarmUpSignalOptions { get; set; } = false;
        public int WarmUpDurationSeconds { get; set; } = 10;
        public SignalOptions WarmUpSignalOptions { get; set; } = new SignalOptions();        

        public ThdAnalysisOptions AnalysisOptions { get; set; } = new ThdAnalysisOptions();

        public OverridableSettings<StopConditions> StopConditions { get; set; } = new OverridableSettings<StopConditions>(AppSettings.Current.StopConditions);
        public OverridableSettings<Fft> Fft { get; set; } = new OverridableSettings<Fft>(AppSettings.Current.Fft);

    }
}
