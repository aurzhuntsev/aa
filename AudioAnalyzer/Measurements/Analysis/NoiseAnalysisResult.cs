using AudioMark.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements.Analysis
{
    [Serializable]
    public class NoiseAnalysisResult : IAnalysisResult
    {
        public SpectralData Data { get; set; }
        public double NoisePowerDbFs { get; set; }
        public double AverageLevelDbTp { get; set; }
    }
}
