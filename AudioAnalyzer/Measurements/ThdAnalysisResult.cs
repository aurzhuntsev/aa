using AudioMark.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements
{
    public class ThdAnalysisResult: IAnalysisResult
    {
        public SpectralData Data { get; set; }

        public double ThdFDb { get; set; }
        public double ThdFPercentage { get; set; }

        public double ThdRDb { get; set; }
        public double ThdRPercentage { get; set; }

        public double EvenToOdd { get; set; }

        public override string ToString()
        {
            var result = new StringBuilder();

            result.AppendLine($"THDf:\t{ThdFDb}dB ({ThdFPercentage}%)");
            result.AppendLine($"THDr:\t{ThdRDb}dB ({ThdRPercentage}%)");
            result.AppendLine($"Even/odd:\t{EvenToOdd}");

            return result.ToString();
        }
    }
}
