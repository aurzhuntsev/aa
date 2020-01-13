using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Generators
{
    public class SilenceGenerator : IGenerator
    {
        public double SampleRate { get; private set; }

        public SilenceGenerator()
        {            
        }

        public double Next()
        {
            return 0.0;
        }
    }
}
