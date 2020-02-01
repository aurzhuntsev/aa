using AudioMark.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements.Settings.Common
{
    public interface ICorrectionProfile
    {
        string CorrectionProfileName { get; set; }
        SpectralData CorrectionProfile { get; set; }
        bool ApplyCorrectionProfile { get; set; }
    }
}
