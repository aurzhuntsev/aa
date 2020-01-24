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

        private bool precalculated = false;
        private double increment = 0.0;
        private double currentValue = 0.0;
        private double[] table = null;
        private int counter = 0;

        public SineGenerator(int sampleRate, double frequency, double amplitude = 1.0, double phase = 0.0)
        {
            SampleRate = sampleRate;
            Frequency = frequency;
            Amplitude = amplitude;
            Phase = phase;

            increment = (2.0 * Math.PI * Frequency) / sampleRate;

            if (sampleRate % frequency == 0)
            {
                precalculated = true;
                table = new double[(int)(sampleRate / frequency)];

                for (var i = 0; i < table.Length; i++)
                {
                    table[i] = Amplitude * Math.Sin(i * increment + Phase);
                }
            }                        
        }

        public double Next()
        {
            double result = 0.0;
            
            //if (precalculated)
            //{
            //    result = table[counter];

            //    counter++;
            //    if (counter == table.Length)
            //    {
            //        counter = 0;
            //    }
            //}
            //else
            //{
                result = Amplitude * Math.Sin(currentValue + Phase);
                currentValue += increment;
            //}

            return result;
        }
    }
}
