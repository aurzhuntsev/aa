using AudioMark.Core.Common;
using AudioMark.Core.Fft;
using AudioMark.Core.Measurements.Settings;
using AudioMark.Core.Measurements.Settings.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioMark.Core.Measurements.Analysis
{
    public class FrequencyResponseAnalytics : IAnalytics
    {
        public IAnalysisResult Analyze(Spectrum data, IMeasurementSettings settings)
        {
            var frSettings = settings as FrequencyResponseMeasurementSettings;
            var result = new FrequencyResponseAnalysisResult();

            var source = data.Statistics.Select(s => s.Mean);

            var min = double.MaxValue;
            var max = double.MinValue;
            var minFrequency = 0.0;
            var maxFrequency = 0.0;

            var index = 0;
            foreach (var item in source)
            {
                if (min > item)
                {
                    min = item;
                    minFrequency = (data.MaxFrequency / data.Size) * index;
                }

                if (max < item)
                {
                    max = item;
                    maxFrequency = (data.MaxFrequency / data.Size) * index; 
                }

                index++;
            }

            result.MinValueDb = min.ToDbTp();
            result.MinValueFrequency = minFrequency;
            result.MaxValueDb = max.ToDbTp();
            result.MaxValueFrequency = maxFrequency;
            result.RippleDb = Math.Abs(-result.MaxValueDb + result.MinValueDb);

            return result;
        }
    }
}
