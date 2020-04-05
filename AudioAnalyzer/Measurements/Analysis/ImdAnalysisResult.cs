using AudioMark.Core.Common;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace AudioMark.Core.Measurements.Analysis
{
    public class ImdAnalysisResult : IAnalysisResult
    {
        public Spectrum Data { get; set; }

        public Dictionary<int, double> OrderedImd { get; set; }        
        
        public double F1Db { get; set; }
        public double F2Db { get; set; }

        public double TotalImdPlusNoiseDb { get; set; }
        public double TotalImdPlusNoisePercentage { get; set; }

        public int MaxOrder { get; set; }
        public double Bandwidth { get; set; }

        public double ImdF2ForGivenOrderDb { get; set; }
        public double ImdF2ForGivenOrderPercentage { get; set; }

        public double ImdF1F2ForGivenOrderDb { get; set; }
        public double ImdF1F2ForGivenOrderPercentage { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Bandwidth:\t{Bandwidth}hz");
            sb.AppendLine($"Max IMD order:\t{MaxOrder}");
            sb.AppendLine($"IMD+N:\t{TotalImdPlusNoiseDb}dB");
            sb.AppendLine($"IMD (div F2):\t{ImdF2ForGivenOrderDb}dB");
            sb.AppendLine($"IMD (div F1+F2):\t{ImdF1F2ForGivenOrderDb}dB");
            
            sb.AppendLine($"F1:\t{F1Db}dB");
            sb.AppendLine($"F2:\t{F2Db}dB");

            foreach (var key in OrderedImd.Keys)
            {
                sb.AppendLine($"{key} order:\t{OrderedImd[key]}dB");
            }

            return sb.ToString();
        }
    }
}