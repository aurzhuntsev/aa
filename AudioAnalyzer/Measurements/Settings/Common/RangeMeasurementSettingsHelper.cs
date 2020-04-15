using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements.Settings.Common
{
    public static class RangeMeasurementSettingsHelper
    {
        public static IEnumerable<double> GetFrequencies(this IRangeMeasurementSettings source)
        {
            if (source.DistributionMode == DistributionMode.List)
            {
                foreach (var f in source.Frequencies)
                {
                    yield return f;
                }
            }

            if (source.Points < 2)
            {
                throw new ArgumentException(nameof(source.Points));
            }

            yield return source.LowFrequency;

            if (source.DistributionMode == DistributionMode.Linear)
            {
                var step = (source.HighFrequency - source.LowFrequency) / (source.Points - 1);
                
                for (var i = 1; i < source.Points - 1; i++)
                {
                    var value = Math.Round(source.LowFrequency + step * i);
                    if (value % 2.0 != 0.0)
                    {
                        value += 1.0;
                    }

                    yield return value;
                }                
            }
            else if (source.DistributionMode == DistributionMode.Logarithmic)
            {
                var step = (Math.Log10(source.HighFrequency - source.LowFrequency)) / (source.Points - 1);
                var previousValue = source.LowFrequency; 
                for (var i = 1; i < source.Points - 1; i++)
                {
                    var value = source.LowFrequency + Math.Pow(10.0, step * i);
                    if (value - previousValue < source.MinLogStep)
                    {
                        value = previousValue + source.MinLogStep;
                    }

                    yield return value;
                    previousValue = value;
                }                
            }

            yield return source.HighFrequency;
        }
    }
}
