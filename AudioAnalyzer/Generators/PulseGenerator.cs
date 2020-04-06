using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Generators
{
    public class PulseGenerator : IGenerator
    {
        public double SampleRate { get; set; }

        public double Amplitude
        {
            get => Math.Max(Math.Abs(Low), Math.Abs(High));
        }

        public double Frequency { get; }
        public double Duty { get; }
        public double Low { get; }
        public double High { get; }

        private int _period;
        private int _tick = 0;

        public PulseGenerator(double sampleRate, double frequecny, double duty, double low = 0.0, double high = 1.0)
        {
            SampleRate = sampleRate;
            Frequency = frequecny;
            Duty = duty;

            Low = low;
            High = high;

            _period = (int)(SampleRate / Frequency);
        }

        public double Next()
        {
            _tick++;
            if (_tick > _period)
            {
                _tick = 0;
            }

            return _tick > (int)(_period * Duty) ? Low : High;
        }
    }
}
