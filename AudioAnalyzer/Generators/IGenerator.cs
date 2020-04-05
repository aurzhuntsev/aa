using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Generators
{
    public interface IGenerator
    {
        double SampleRate { get; }
        double Amplitude { get; }
        double Next();
    }
}
