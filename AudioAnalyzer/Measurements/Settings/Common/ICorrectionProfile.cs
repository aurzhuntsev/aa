using AudioMark.Core.Common;
using AudioMark.Core.Fft;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements.Settings.Common
{
    public interface ICorrectionProfile
    {
        string CorrectionProfileName { get; set; }
        Spectrum CorrectionProfile { get; set; }
        bool ApplyCorrectionProfile { get; set; }
    }
}
