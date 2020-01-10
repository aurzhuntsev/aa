using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioMark.Core.Common
{
    public class SpectralData
    {        
        public class StatisticsItem
        {            
            public double LastValue { get; set; }
            public double Sum { get; set; } = 0;
            public double Min { get; set; } = double.MaxValue;
            public double Max { get; set; } = double.MinValue;
            public double Mean { get; set; } = double.NaN;
            public double StandardDeviation { get; set; } = double.NaN;            

            internal double PreviousValue { get; set; } = double.NaN;
            internal double PreviousMean { get; set; } = double.NaN;
            internal double M2 { get; set; } = 0.0;
        }

        public int Size { get; set; }
        public double MaxFrequency { get; set; }
        public int Count { get; set; } = 0;

        public StatisticsItem[] Statistics { get; private set; }

        private object _sync = new object();

        public SpectralData(int size, double maxFrequency)
        {
            Size = size;
            MaxFrequency = maxFrequency;
            
            Statistics = new StatisticsItem[Size];
            for (var i = 0; i < size; i++)
            {
                Statistics[i] = new StatisticsItem();
            }
        }

        public void Set(double[] values)
        {
            lock (_sync)
            {
                Count++;

                for (var i = 0; i < Size; i++)
                {
                    var value = values[i];
                    var stat = Statistics[i];

                    stat.PreviousValue = stat.LastValue;
                    stat.LastValue = value;

                    stat.Sum += value;
                    stat.PreviousMean = stat.Mean;
                    stat.Mean = stat.Sum / Count;

                    if (value > stat.Max)
                    {
                        stat.Max = value;
                    }
                    if (value < stat.Min)
                    {
                        stat.Min = value;
                    }

                    if (Count > 1)
                    {
                        stat.M2 = stat.M2 + (value - stat.PreviousMean) * (value - stat.Mean);
                        stat.StandardDeviation = Math.Sqrt(stat.M2 / (Count - 1.0));
                    }
                }
            }
        }
    }
}
