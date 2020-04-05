using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioMark.Core.Generators
{
    public class CompositeGenerator : IGenerator
    {
        public double SampleRate { get; private set; }
        public double Amplitude { get; } = 1.0;

        public List<IGenerator> Generators { get; } = new List<IGenerator>();

        public CompositeGenerator(double sampleRate, double amplitude = 1.0)
        {
            SampleRate = sampleRate;
            Amplitude = amplitude;
        }

        public CompositeGenerator(double sampleRate, double amplitude = 1.0, params IGenerator[] generators) : this(sampleRate, amplitude)
        {
            Generators.AddRange(generators);
        }

        public double Next()
        {
            var k = Generators.Sum(g => g.Amplitude) / Amplitude;
            return Generators.Sum(g => g.Next()) / k;
        }
    }
}
