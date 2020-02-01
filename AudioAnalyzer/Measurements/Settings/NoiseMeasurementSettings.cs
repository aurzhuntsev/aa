using AudioMark.Core.Common;
using AudioMark.Core.Measurements.Settings.Common;
using AudioMark.Core.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements.Settings
{
    [Serializable]
    public class NoiseMeasurementSettings : IMeasurementSettings, IGlobalOptions, ICorrectionProfile
    {
        public bool GenerateDummySignal { get; set; } = false;
        public SignalSettings DummySignalOptions { get; set; } = new SignalSettings()
        {
            Frequency = AppSettings.Current.Device.SampleRate / 2.0,
            InputOutputOptions = new InputOutputLevel() { OutputLevel = -100.0 }
        };

        public bool LimitHighFrequency { get; set; }
        public double HighFrequency { get; set; } = AppSettings.Current.Device.SampleRate / 2.0;

        public string CorrectionProfileName { get; set; }
        public SpectralData CorrectionProfile { get; set; }
        public bool ApplyCorrectionProfile { get; set; }

        public OverridableSettings<AudioMark.Core.Settings.StopConditions> StopConditions { get; } = new OverridableSettings<AudioMark.Core.Settings.StopConditions>(AppSettings.Current.StopConditions);
        public OverridableSettings<Fft> Fft { get; } = new OverridableSettings<Fft>(AppSettings.Current.Fft);
    }
}
