using AudioMark.Core.Common;
using AudioMark.Core.Fft;
using AudioMark.Core.Measurements.Settings.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements.Analysis
{
    public class FrequencyAnalytics : IAnalytics
    {
        public IAnalysisResult Analyze(Spectrum data, IMeasurementSettings settings)
        {
            var result = new FrequencyAnalysisResult();
            var frequencySettings = settings as ITestSignal;

            result.FrequencyValueDb = -data.ValueAtFrequency(frequencySettings.TestSignalOptions.Frequency, x => x.Mean, frequencySettings.WindowHalfSize).ToDbTp();

            return result;
        }
    }
}
