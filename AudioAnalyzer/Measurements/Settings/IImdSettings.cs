using AudioMark.Core.Measurements.Settings.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements.Settings
{
    public interface IImdSettings : IMeasurementSettings, ITestSignal, IGlobalOptions, ICorrectionProfile, IWarmable
    {
        public double F1Frequency { get; }
        public double F2Frequency { get; }
        public double SignalsRate { get; set; }

        int MaxOrder { get; set; }
        bool LimitMaxFrequency { get; set; }
        double MaxFrequency { get; set; }
    }
}
