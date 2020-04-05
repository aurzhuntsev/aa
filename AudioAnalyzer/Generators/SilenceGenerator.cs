using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Generators
{
    public class SilenceGenerator : IGenerator
    {
        public double SampleRate { get; private set; }

        public double Amplitude { get; } = 0.0;

        public SilenceGenerator()
        {            
        }

        public double Next()
        {
            return 0.0;
        }
    }
}
