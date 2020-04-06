using AudioMark.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements.Analysis
{
    [Serializable]
    public class NoiseAnalysisResult : AnalysisResultBase
    {
        [AnalysisResultField("Bandwidth, hz")]
        public double Bandwidth { get; set; }

        [AnalysisResultField("Noise power, dB")]
        public double NoisePowerDbFs { get; set; }

        [AnalysisResultField("Average level, dB")]
        public double AverageLevelDbTp { get; set; }
    }
}
