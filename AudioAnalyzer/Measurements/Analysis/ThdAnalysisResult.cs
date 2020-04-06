using AudioMark.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements.Analysis
{
    [Serializable]
    public class ThdAnalysisResult: AnalysisResultBase
    {        
        [AnalysisResultField("Bandwidth, hz")]
        public double Bandwidth { get; set; }

        [AnalysisResultField("No. harmonics")]
        public int NumberOfHarmonics { get; set; }

        [AnalysisResultField("Fundamental signal, hz")]
        public double FundamentalFrequency { get; set; }

        [AnalysisResultField("Fundamental signal, dB")]
        public double FundamentalDb { get; set; }

        [AnalysisResultField("THD+N, dB")] 
        public double ThdNDb { get; set; }

        [AnalysisResultField("THD+N, %")]
        public double ThdNPercentage { get; set; }

        [AnalysisResultField("THDf, dB")]
        public double ThdFDb { get; set; }
        
        [AnalysisResultField("THDf, %")]
        public double ThdFPercentage { get; set; }

        [AnalysisResultField("THDr, dB")]
        public double ThdRDb { get; set; }

        [AnalysisResultField("THDr, %")]
        public double ThdRPercentage { get; set; }
        
        [AnalysisResultDictionary("{0} harmonic, dB")]
        public Dictionary<int, double> Harmonics { get; set; }
    }
}
