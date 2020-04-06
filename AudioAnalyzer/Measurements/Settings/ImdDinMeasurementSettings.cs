using AudioMark.Core.Common;
using AudioMark.Core.Measurements.Settings.Common;
using AudioMark.Core.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements.Settings
{
    public class ImdDinMeasurementSettings : IImdSettings
    {
        public SignalSettings TestSignalOptions { get; set; } = new SignalSettings();

        public double SecondarySignalFrequency { get; set; } = 15000;
        public double SignalsRate { get; set; } = 0.2512;

        public bool WarmUpEnabled { get; set; } = true;
        public int WarmUpDurationSeconds { get; set; } = 10;

        public string CorrectionProfileName { get; set; }
        public Spectrum CorrectionProfile { get; set; }
        public bool ApplyCorrectionProfile { get; set; }

        public int WindowHalfSize { get; set; } = 1; /* since default low-frequency signal tend to spread */

        public int MaxOrder { get; set; } = 3;
        public bool LimitMaxFrequency { get; set; } = false;
        public double MaxFrequency { get; set; } = 20000.0;

        public OverridableSettings<AudioMark.Core.Settings.StopConditions> StopConditions { get; } = new OverridableSettings<AudioMark.Core.Settings.StopConditions>(AppSettings.Current.StopConditions);
        public OverridableSettings<Fft> Fft { get; } = new OverridableSettings<Fft>(AppSettings.Current.Fft);

        public double F1Frequency => TestSignalOptions.Frequency;
        public double F2Frequency => SecondarySignalFrequency;

        public ImdDinMeasurementSettings()
        {
            TestSignalOptions.Frequency = 3150;
        }
    }
}
