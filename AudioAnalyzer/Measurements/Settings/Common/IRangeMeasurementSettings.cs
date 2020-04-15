using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements.Settings.Common
{
    public enum DistributionMode
    {
        Logarithmic, Linear, List
    }

    public interface IRangeMeasurementSettings : IMeasurementSettings
    {
        public double LowFrequency { get; set; }
        public double HighFrequency { get; set; }
        public int Points { get; set; }
        public DistributionMode DistributionMode { get; set; }
        
        public double MinLogStep { get; set; }
        public List<double> Frequencies { get; }
    }
}
