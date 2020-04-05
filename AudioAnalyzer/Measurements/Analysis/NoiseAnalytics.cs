using AudioMark.Core.Common;
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
                var right = noiseSettings.LimitHighFrequency ? result.Data.GetFrequencyIndices(noiseSettings.HighFrequency, 0).First() : result.Data.Size;
                var sum = Enumerable.Range(0, right).Sum(s => result.Data.Statistics[s].Mean * result.Data.Statistics[s].Mean);
                var avg = Enumerable.Range(0, right).Average(s => result.Data.Statistics[s].Mean);

                result.NoisePowerDbFs = -Math.Sqrt(sum / right).ToDbTp() * 0.5;
                result.AverageLevelDbTp = -avg.ToDbTp();

                return result;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
