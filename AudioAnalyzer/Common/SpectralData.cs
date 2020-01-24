using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioMark.Core.Common
{
    [Serializable]
    public class SpectralData
    {
        [Serializable]
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

            public string Label { get; set; }
        }

        public int Size { get; set; }
        public int MaxFrequency { get; set; }
        public double FrequencyPerBin => (double)MaxFrequency / Size;

        public int Count { get; set; } = 0;

        [field: NonSerialized]
        public Func<StatisticsItem, double> DefaultValueSelector { get; set; } = (x) => x.Mean;

        public StatisticsItem[] Statistics { get; private set; }

        private object _sync = new object();

        public SpectralData(int size, int maxFrequency)
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

        /* TODO: windowHalfSize greater than zero is not tested */
        public IEnumerable<StatisticsItem> AtFrequency(double frequency, int windowHalfSize = 0)
        {
            var index = (int)Math.Round(frequency * (double)Size / MaxFrequency);

            if (windowHalfSize > 0)
            {
                var left = Math.Max(0, index - windowHalfSize);
                foreach (var item in Statistics.Skip(left).Take(windowHalfSize))
                {
                    yield return item;
                }
            }

            yield return Statistics[index];

            if (windowHalfSize > 0)
            {
                var right = Math.Min(Size - 1, index + windowHalfSize);
                foreach (var item in Statistics.Skip(right).Take(windowHalfSize))
                {
                    yield return item;
                }
            }
        }
    }
}
