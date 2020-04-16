using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements.Analysis
{
    public class FrequencyResponseAnalysisResult : AnalysisResultBase
    {
        [AnalysisResultField("Min. level, dB")]
        public double MinValueDb { get; set; }
        
        [AnalysisResultField("Min. level @ frequency, hz")]
        public double MinValueFrequency { get; set; }

        [AnalysisResultField("Max. level, dB")]
        public double MaxValueDb { get; set; }

        [AnalysisResultField("Max. level @ frequency, hz")]
        public double MaxValueFrequency { get; set; }

        [AnalysisResultField("Ripple, dB")]
        public double RippleDb { get; set; }

        public FrequencyResponseAnalysisResult()
        {
        }
    }
}
