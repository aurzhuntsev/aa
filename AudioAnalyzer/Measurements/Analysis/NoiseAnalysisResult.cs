using AudioMark.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements.Analysis
{
    [Serializable]
    public class NoiseAnalysisResult : IAnalysisResult
    {
        public Spectrum Data { get; set; }
        public double NoisePowerDbFs { get; set; }
        public double AverageLevelDbTp { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Noise power:\t{NoisePowerDbFs}");
            sb.AppendLine($"Average level:\t{AverageLevelDbTp}");

            return sb.ToString();
        }
    }
}
