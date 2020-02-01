using AudioMark.Core.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements.Settings.Common
{
    public interface IGlobalOptions
    {
        OverridableSettings<AudioMark.Core.Settings.StopConditions> StopConditions { get; }
        OverridableSettings<Fft> Fft { get; }
    }
}
