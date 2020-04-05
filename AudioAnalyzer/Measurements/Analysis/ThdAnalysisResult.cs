using AudioMark.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements.Analysis
{
    [Serializable]
    public class ThdAnalysisResult: IAnalysisResult
    {
        public Spectrum Data { get; set; }

        public double TotalThdPlusNoisePercentage { get; set; }
        public double TotalThdPlusNoiseDb { get; set; }

        public double ThdFDb { get; set; }
        public double ThdFPercentage { get; set; }

        public double ThdRDb { get; set; }
        public double ThdRPercentage { get; set; }

        public double FundamentalDb { get; set; }
        public List<double> Harmonics { get; set; }

        public override string ToString()
        {
            var result = new StringBuilder();

            result.AppendLine($"THD+N:\t{TotalThdPlusNoiseDb}dB ({TotalThdPlusNoisePercentage}%)");
            result.AppendLine($"THDf:\t{ThdFDb}dB ({ThdFPercentage}%)");
            result.AppendLine($"THDr:\t{ThdRDb}dB ({ThdRPercentage}%)");
        
            return result.ToString();
        }
    }
}
