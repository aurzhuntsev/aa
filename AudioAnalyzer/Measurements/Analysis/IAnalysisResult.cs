using AudioMark.Core.Common;
using AudioMark.Core.Fft;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements.Analysis
{
    public interface IAnalysisResult
    {
        Spectrum Data { get; }
    }
}
