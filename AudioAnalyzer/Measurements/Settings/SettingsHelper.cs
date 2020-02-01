using AudioMark.Core.Common;
using AudioMark.Core.Measurements.Common;
using AudioMark.Core.Measurements.Settings.Common;
using AudioMark.Core.Measurements.StopConditions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements.Settings
{
    public static class SettingsHelper
    {
        public static void ApplyStopConditions(this IGlobalOptions source, Activity<SpectralData> target, SpectralData data)
        {
            if (source.StopConditions.Value.TimeoutEnabled)
            {
                target.RegisterStopCondition(new TimeoutStopCondition(source.StopConditions.Value.Timeout * 1000));
            }
            if (source.StopConditions.Value.ToleranceMatchingEnabled)
            {
                target.RegisterStopCondition(new ToleranceAchievedStopCondition(data, source.StopConditions.Value.Tolerance, source.StopConditions.Value.Confidence));
            }
        }
    }
}
