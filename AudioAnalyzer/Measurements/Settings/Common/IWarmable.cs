using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements.Settings.Common
{
    public interface IWarmable
    {
        bool WarmUpEnabled { get; set; }
        int WarmUpDurationSeconds { get; set; }
    }
}
