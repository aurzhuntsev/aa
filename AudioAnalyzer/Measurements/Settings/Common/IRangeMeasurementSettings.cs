using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements.Settings.Common
{
    public enum DistributionModes
    {
        Linear, Logarithmic, List
    }

    public interface IRangeMeasurementSettings : IMeasurementSettings
    {
        public double LowFrequency { get; set; }
        public double HighFrequency { get; set; }
        public int Points { get; set; }
        public DistributionModes DistributionMode { get; set; }
        
        public double MinLogStep { get; set; }
        public List<double> Frequencies { get; }
    }
}
