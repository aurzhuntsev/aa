using AudioMark.Core.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements.Settings.Common
{
    /* TODO: Add input level matching support */
    [Serializable]
    public class InputOutputLevel
    {
        public double OutputLevel { get; set; } = -AppSettings.Current.Device.ClippingLevel;
        
        public override string ToString()
        {
            return $"Out {OutputLevel}dBTP";
        }
    }
}
