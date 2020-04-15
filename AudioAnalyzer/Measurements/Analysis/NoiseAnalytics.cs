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
    public class NoiseAnalytics : IAnalytics
    {
        public IAnalysisResult Analyze(Spectrum data, IMeasurementSettings settings)
        {
            var result = new NoiseAnalysisResult();
            result.Data = data;

            if (settings is NoiseMeasurementSettings noiseSettings)
            {
                var maxFrequency = noiseSettings.LimitHighFrequency ? noiseSettings.HighFrequency : data.Size;
                var right = noiseSettings.LimitHighFrequency ? result.Data.GetFrequencyIndices(noiseSettings.HighFrequency, 0).First() : result.Data.Size;
                var avg = -Enumerable.Range(2, right).Average(s => result.Data.Statistics[s].Mean).ToDbTp();
                
                result.Bandwidth = maxFrequency;                
                result.AverageLevelDbTp = avg;
                return result;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
