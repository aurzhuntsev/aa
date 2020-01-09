using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioMark.Core.Common
{
    public class SpectralData
    {        
        public class StatisticsRecord
        {
            public double Sum { get; set; } = 0;
            public double Min { get; set; } = double.MaxValue;
            public double Max { get; set; } = double.MinValue;
            public double Mean { get; set; } = double.NaN;
        }

        public int Size { get; set; }
        public double MaxFrequency { get; set; }
        public int Count { get; private set; }

        public List<double>[] Data { get; private set; }
        public StatisticsRecord[] Statistics { get; private set; }

        private object _sync = new object();

        public SpectralData(int size, double maxFrequency)
        {
            Size = size;
            MaxFrequency = maxFrequency;

            Data = new List<double>[Size];
            for (var i = 0; i < size; i++)
            {
                Data[i] = new List<double>();
            }

            Statistics = new StatisticsRecord[Size];
            for (var i = 0; i < size; i++)
            {
                Statistics[i] = new StatisticsRecord();
            }
        }

        public void Add(double[] values)
        {
            lock (_sync)
            {
                for (var i = 0; i < Size; i++)
                {
                    var value = values[i];
                    Data[i].Add(value);

                    var statistics = Statistics[i];
                    statistics.Sum += value;
                    if (value < statistics.Min)
                    {
                        statistics.Min = value;
                    }
                    if (value > statistics.Max)
                    {
                        statistics.Max = value;
                    }

                    statistics.Mean = statistics.Sum / Count;
                }

                Count++;
            }
        }

        public void Set(double[] values)
        {
            lock (_sync)
            {
                for (var i = 0; i < Size; i++)
                {
                    var value = values[i];
                    if (Data[i].Count == 0)
                    {
                        Data[i].Add(value);
                    }
                    else if (Data[i].Count == 1)
                    {
                        Data[i][Data[i].Count - 1] = value;
                    }
                    else
                    {
                        throw new InvalidOperationException("Cannot Set because there are multiple items in the list.");
                    }

                    var statistics = Statistics[i];
                    statistics.Sum = value;                    
                    statistics.Min = value;
                    statistics.Max = value;
                    statistics.Mean = value;
                }

                Count = Math.Max(1, Count);
            }
        }

        public double GetStandartDeviation(int index)
        {
            if (Count < 2)
            {
                return double.NaN;
            }

            double sum = 0.0;
            var data = Data[index];
            var mean = Statistics[index].Mean;

            for (var i = 0; i < Count; i++)
            {
                var value = data[i];
                var diff = (value - mean);
                sum += diff * diff;
            }

            return Math.Sqrt(sum / (Count - 1));
        }
    }
}
