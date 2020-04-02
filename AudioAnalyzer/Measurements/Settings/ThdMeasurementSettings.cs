using AudioMark.Core.Common;
using AudioMark.Core.Generators;
using AudioMark.Core.Measurements.Settings.Common;
using AudioMark.Core.Settings;
using System;
using System.Collections.Generic;
using System.Text;



namespace AudioMark.Core.Measurements.Settings
{
    [Serializable]
    public class ThdMeasurementSettings : IMeasurementSettings, IGlobalOptions, ICorrectionProfile
    {
        public SignalSettings TestSignalOptions { get; set; } = new SignalSettings();
              
        public bool WarmUpEnabled { get; set; } = true;
        public int WarmUpDurationSeconds { get; set; } = 10;

        public string CorrectionProfileName { get; set; }
        public SpectralData CorrectionProfile { get; set; }
        public bool ApplyCorrectionProfile { get; set; }

        public int WindowHalfSize { get; set; } = 0;
        public bool LimitMaxHarmonics { get; set; } = true;
        public int MaxHarmonics { get; set; } = 10;
        public bool LimitMaxFrequency { get; set; } = false;
        public double MaxFrequency { get; set; } = 20000.0;
                      
        public OverridableSettings<AudioMark.Core.Settings.StopConditions> StopConditions { get; } = new OverridableSettings<AudioMark.Core.Settings.StopConditions>(AppSettings.Current.StopConditions);
        public OverridableSettings<Fft> Fft { get; } = new OverridableSettings<Fft>(AppSettings.Current.Fft);        
    }
}
