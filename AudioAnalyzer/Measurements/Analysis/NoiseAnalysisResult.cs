using AudioMark.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements.Analysis
{
    /* TODO: Add noise (and THD?) weighting */

    [Serializable]
    public class NoiseAnalysisResult : AnalysisResultBase
    {
        [AnalysisResultField("Bandwidth, hz")]
        public double Bandwidth { get; set; }

        [AnalysisResultField("Average level, dB")]
        public double AverageLevelDbTp { get; set; }
    }
}
