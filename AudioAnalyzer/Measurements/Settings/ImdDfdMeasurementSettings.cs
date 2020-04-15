using AudioMark.Core.Common;
using AudioMark.Core.Measurements.Settings.Common;
using AudioMark.Core.Settings;
using System;
using System.Collections.Generic;
using System.Text;
using AudioMark.Core.Fft;
namespace AudioMark.Core.Measurements.Settings
{
    public class ImdDfdMeasurementSettings : IGlobalOptions, ICorrectionProfile, IWarmable, IImdSettings
    {
        public SignalSettings TestSignalOptions { get; set; } = new SignalSettings();

        public double FrequencyDifference { get; set; } = 80.0;
        public double SignalsRate { get; set; } = 1;

        public bool WarmUpEnabled { get; set; } = true;
        public int WarmUpDurationSeconds { get; set; } = 10;

        public string CorrectionProfileName { get; set; }
        public Spectrum CorrectionProfile { get; set; }
        public bool ApplyCorrectionProfile { get; set; }

        public int WindowHalfSize { get; set; } = 1;

        public int MaxOrder { get; set; } = 3;
        public bool LimitMaxFrequency { get; set; } = false;
        public double MaxFrequency { get; set; } = 20000.0;

        public OverridableSettings<AudioMark.Core.Settings.StopConditions> StopConditions { get; } = new OverridableSettings<AudioMark.Core.Settings.StopConditions>(AppSettings.Current.StopConditions);
        public OverridableSettings<AudioMark.Core.Settings.Fft> Fft { get; } = new OverridableSettings<AudioMark.Core.Settings.Fft>(AppSettings.Current.Fft);

        public double F1Frequency => Math.Max(0, TestSignalOptions.Frequency - FrequencyDifference * 0.5);
        public double F2Frequency => TestSignalOptions.Frequency + FrequencyDifference * 0.5;

        public double SignalDetectionThresholdDb { get; set; } = -30.0;

        public ImdDfdMeasurementSettings()
        {
            TestSignalOptions.Frequency = 1000.0;
        }
    }
}
