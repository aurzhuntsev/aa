using AudioMark.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements.Analysis
{
    public class AnalysisResultFieldAttribute: StringAttribute
    {
        public AnalysisResultFieldAttribute(string description) : base(description) { }
    }
}
