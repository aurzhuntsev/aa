using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Settings
{
    public class StopConditions
    {
        public bool EnableToleranceMatching { get; set; }
        public double Confidence { get; set; }
        public double Tolerance { get; set; }

        public bool EnableTimeout { get; set; }
        public double Timeout { get; set; }
    }
}
