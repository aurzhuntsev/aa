using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Generators
{
    public class SineGenerator : IGenerator
    {
        public double SampleRate { get; private set; }

        public double Frequency { get; private set; }
        public double Amplitude { get; set; }
        public double Phase { get; private set; }

        private double increment = 0.0;
        private double currentValue = 0.0;
        private double[] table = null;

        public SineGenerator(int sampleRate, double frequency, double amplitude = 1.0, double phase = 0.0)
        {
            SampleRate = sampleRate;
            Frequency = frequency;
            Amplitude = amplitude;
            Phase = phase;

            increment = (2.0 * Math.PI * Frequency) / sampleRate;
        }

        public double Next()
        {
            var result = Amplitude * Math.Sin(currentValue + Phase);
            currentValue += increment;

            return result;
        }
    }
}
