using AudioMark.Core.Common;
using AudioMark.Core.Measurements.Settings.Common;
using AudioMark.Core.Settings;
using System;
using System.Collections.Generic;
using System.Text;
using AudioMark.Core.Fft;
namespace AudioMark.Core.Measurements.Settings
{
    public class FrequencyResponseMeasurementSettings : IRangeMeasurementSettings, ICorrectionProfile, ITestSignal
    {
        public double LowFrequency { get; set; } = 20.0;
        public double HighFrequency { get; set; } = 20000.0;
        public int Points { get; set; } = 25;
        public DistributionModes DistributionMode { get; set; } = DistributionModes.Linear;

        public string CorrectionProfileName { get; set; }
        public Spectrum CorrectionProfile { get; set; }
        public bool ApplyCorrectionProfile { get; set; }

        public OverridableSettings<AudioMark.Core.Settings.StopConditions> StopConditions { get; } = new OverridableSettings<AudioMark.Core.Settings.StopConditions>(AppSettings.Current.StopConditions);
        public OverridableSettings<AudioMark.Core.Settings.Fft> Fft { get; } = new OverridableSettings<AudioMark.Core.Settings.Fft>(AppSettings.Current.Fft);

        public SignalSettings TestSignalOptions { get; set; } = new SignalSettings();
        public int WindowHalfSize { get; set; } = 1;
        public double SignalDetectionThresholdDb { get; set; } = -30.0;
        public double MinLogStep { get; set; } = 10.0;

        public List<double> Frequencies { get; } = new List<double>() { 20.0, 20000.0 };
    }
}
