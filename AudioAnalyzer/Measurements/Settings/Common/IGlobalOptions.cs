using AudioMark.Core.Settings;
using System;
using System.Collections.Generic;
using System.Text;
using AudioMark.Core.Fft;
namespace AudioMark.Core.Measurements.Settings.Common
{
    public interface IGlobalOptions
    {
        OverridableSettings<AudioMark.Core.Settings.StopConditions> StopConditions { get; }
        OverridableSettings<AudioMark.Core.Settings.Fft> Fft { get; }
    }
}
