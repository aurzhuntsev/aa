using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements.Settings.Common
{
    [Serializable]
    public class SignalSettings
    {
        public double Frequency { get; set; } = 1000.0;
        public InputOutputLevel InputOutputOptions { get; set; } = new InputOutputLevel();
    }
}
