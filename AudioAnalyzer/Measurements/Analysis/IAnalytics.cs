using AudioMark.Core.Common;
using AudioMark.Core.Fft;
using AudioMark.Core.Measurements.Analysis;
using AudioMark.Core.Measurements.Settings.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements.Analysis
{
    public interface IAnalytics
    {
        IAnalysisResult Analyze(Spectrum data, IMeasurementSettings settings);
    }
}
