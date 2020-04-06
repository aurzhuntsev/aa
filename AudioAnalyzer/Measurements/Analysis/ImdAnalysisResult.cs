using AudioMark.Core.Common;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace AudioMark.Core.Measurements.Analysis
{
    public class ImdAnalysisResult : AnalysisResultBase
    {        
        [AnalysisResultField("Bandwidth, hz")]
        public double Bandwidth { get; set; }

        [AnalysisResultField("Max. order")]
        public int MaxOrder { get; set; }

        [AnalysisResultField("F1 frequency, hz")]
        public double F1Frequency { get; set; }

        [AnalysisResultField("F1 level, dB")]
        public double F1Db { get; set; }

        [AnalysisResultField("F2 frequency, hz")]
        public double F2Frequency { get; set; }

        [AnalysisResultField("F2 level, dB")]
        public double F2Db { get; set; }

        [AnalysisResultField("IMD+N, dB")]
        public double TotalImdPlusNoiseDb { get; set; }

        [AnalysisResultField("IMD+N, %")]
        public double TotalImdPlusNoisePercentage { get; set; }

        [AnalysisResultField("IMD div F2, dB")]
        public double ImdF2ForGivenOrderDb { get; set; }

        [AnalysisResultField("IMD div F2, %")]
        public double ImdF2ForGivenOrderPercentage { get; set; }

        [AnalysisResultField("IMD div F1+F2, dB")]
        public double ImdF1F2ForGivenOrderDb { get; set; }

        [AnalysisResultField("IMD div F1+F2, %")]
        public double ImdF1F2ForGivenOrderPercentage { get; set; }

        [AnalysisResultDictionary("{0} order IMD")]
        public Dictionary<int, double> OrderedImd { get; set; }                                           
    }
}