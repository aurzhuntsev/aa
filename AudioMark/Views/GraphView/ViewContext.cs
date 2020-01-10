using Avalonia;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Views.GraphView
{
    public class ViewContext
    {
        public readonly int MinValueExponent = 10;

        public Rect Bounds { get; private set; }                
        public double MaxFrequency { get; private set; }

        private double _kFrequencyToViewX = 0.0;
        private double _kDbToViewY = 0.0;
        
        public int FreqToViewX(double value)
        {
            if (value == 0.0)
            {
                return 0;
            }

            return (int)(_kFrequencyToViewX * Math.Log10(value));
        }

        public  int DbToViewY(double value)
        {
            return (int)(-_kDbToViewY * Math.Log10(1.0 / value));
        }

        public void Update(Rect bounds, double maxFrequency)
        {
            Bounds = bounds;
            MaxFrequency = maxFrequency;

            _kFrequencyToViewX = (Bounds.Width / Math.Log10(MaxFrequency));
            _kDbToViewY = (Bounds.Height / Math.Log10(Math.Pow(10.0, -MinValueExponent)));
        }

        internal object DbToViewY(object frequency)
        {
            throw new NotImplementedException();
        }
    }
}
