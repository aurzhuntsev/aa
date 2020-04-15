using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements.Settings.Common
{
    public interface ITestSignal
    {
        SignalSettings TestSignalOptions { get; set; }
        int WindowHalfSize { get; set; }
        double SignalDetectionThresholdDb { get; set; }
    }
}
