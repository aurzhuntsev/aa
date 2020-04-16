using AudioMark.Core.Measurements.Settings.Common;
using AudioMark.Core.Settings;
using AudioMark.Core.Fft;
using System;
using System.Collections.Generic;
using System.Text;
using AudioMark.Core.Fft;

namespace AudioMark.Core.Measurements.Settings
{
    public class FrequencyMeasurementSettings : IMeasurementSettings, ITestSignal
    {
        public SignalSettings TestSignalOptions { get; set; } = new SignalSettings();
        public int WindowHalfSize { get; set; } = 1;
        public double SignalDetectionThresholdDb { get; set; } = -90.0;
        
        public OverridableSettings<AudioMark.Core.Settings.StopConditions> StopConditions { get; } = new OverridableSettings<AudioMark.Core.Settings.StopConditions>(AppSettings.Current.StopConditions);
        public OverridableSettings<AudioMark.Core.Settings.Fft> Fft { get; } = new OverridableSettings<AudioMark.Core.Settings.Fft>(AppSettings.Current.Fft);

        public FrequencyMeasurementSettings()
        {
            StopConditions.Overriden = true;
            StopConditions.Value.Timeout = 3;
        }
    }
}
