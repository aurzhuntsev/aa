using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Settings
{
    public class StopConditions
    {
        public int CheckIntervalMilliseconds { get; set; }

        public bool ToleranceMatchingEnabled { get; set; }
        public double Confidence { get; set; }
        public double Tolerance { get; set; }

        public bool TimeoutEnabled { get; set; }
        public int Timeout { get; set; }
    }
}
