using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioMark.Core.Common
{
    public class SpectralData
    {
        public class MeanAndStandartDeviation
        {
            public double Mean { get; set; }
            public double StandardDeviation { get; set; }
        }

        public int Size { get; set; }
        public double MaxFrequency { get; set; }
        public int Count { get; private set; }

        List<double>[] data = null;
        double[] sums = null;

        public List<double>[] Data => data;

        private object sync = new object();

        public SpectralData(int size, double maxFrequency)
        {
            Size = size;
            MaxFrequency = maxFrequency;

            data = new List<double>[Size];
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = new List<double>();
            }

            sums = new double[Size];
        }

        public void Add(double[] values)
        {
            for (var i = 0; i < Size; i++)
            {
                data[i].Add(values[i]);
                sums[i] += values[i];
            }

            Count++;
        }

        public MeanAndStandartDeviation GetMeanAndStandardDeviation(int index)
        {
            var result = new MeanAndStandartDeviation();

            if (Count < 1)
            {
                result.Mean = double.NaN;
            }
            else
            {
                result.Mean = sums[index] / Count;

                if (Count < 2)
                {
                    result.StandardDeviation = double.NaN;
                }
                else
                {
                    double sum = 0.0;
                    foreach (var value in data[index])
                    {
                        var diff = (value - result.Mean);
                        sum += diff * diff ;
                    }

                    result.StandardDeviation = Math.Sqrt(sum / (Count - 1));
                }
            }

            return result;
        }
    }
}
