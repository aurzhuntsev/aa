using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace AudioMark.Core.Fft
{
    [Serializable]
    public class Spectrum
    {
        [Serializable]
        public class StatisticsItem
        {
            public double LastValue { get; internal set; }
            public double Sum { get; internal set; } = 0;
            public double Min { get; internal set; } = double.MaxValue;
            public double Max { get; internal set; } = double.MinValue;
            public double Mean { get; internal set; } = double.NaN;
            public double StandardDeviation { get; set; } = double.NaN;
            public string Label { get; internal set; }

            internal double PreviousValue { get; set; } = double.NaN;
            internal double PreviousMean { get; set; } = double.NaN;
            internal double M2 { get; set; } = 0.0;

        }

        public enum DefaultValueType
        {
            Last, Mean
        }

        private object _sync = new object();

        private StatisticsItem[] _statistics;
        private StatisticsItem[] _correctedStatistics;
        private Spectrum _correctionProfile;

        [NonSerialized]
        private Func<Spectrum, int, bool> _correctionApplicableItemSelector;

        public int Size { get; set; }
        public int MaxFrequency { get; set; }
        public int Count { get; set; } = 0;
        /* TODO: Rename */
        public DefaultValueType DefaultValue { get; set; }

        public double FrequencyPerBin => (double)MaxFrequency / Size;

        public StatisticsItem[] Statistics
        {
            get
            {
                if (_correctionProfile == null)
                {
                    return _statistics;
                }

                return _correctedStatistics;
            }
        }

        public Spectrum(int size, int maxFrequency)
        {
            Size = size;
            MaxFrequency = maxFrequency;

            Reset();
        }

        public void Set(double[] values)
        {
            lock (_sync)
            {
                Count++;

                for (var i = 0; i < Size; i++)
                {
                    var value = values[i];
                    var stat = _statistics[i];

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

                UpdateCorrectedStatistics();
            }
        }

        private void UpdateCorrectedStatistics()
        {
            if (_correctionProfile == null)
            {
                return;
            }

            if (_correctedStatistics == null)
            {
                _correctedStatistics = new StatisticsItem[Size];
                for (var i = 0; i < Size; i++)
                {
                    _correctedStatistics[i] = new StatisticsItem();
                }
            }

            for (var i = 0; i < Size; i++)
            {
                _correctedStatistics[i].Label = _statistics[i].Label;
                if (!_correctionApplicableItemSelector(this, i))
                {
                    _correctedStatistics[i].LastValue = _statistics[i].LastValue;
                    _correctedStatistics[i].Max = _statistics[i].Max;
                    _correctedStatistics[i].Min = _statistics[i].Min;
                    _correctedStatistics[i].Mean = _statistics[i].Mean;
                    _correctedStatistics[i].Sum = _statistics[i].Sum;
                    _correctedStatistics[i].StandardDeviation = _statistics[i].StandardDeviation;
                }
                else
                {
                    _correctedStatistics[i].LastValue = Math.Abs(_statistics[i].LastValue - _correctionProfile.Statistics[i].LastValue);
                    _correctedStatistics[i].Max = Math.Abs(_statistics[i].Max - _correctionProfile.Statistics[i].Max);
                    _correctedStatistics[i].Min = Math.Abs(_statistics[i].Min - _correctionProfile.Statistics[i].Min);
                    _correctedStatistics[i].Mean = Math.Abs(_statistics[i].Mean - _correctionProfile.Statistics[i].Mean);
                    _correctedStatistics[i].Sum = Math.Abs(_statistics[i].Sum - _correctionProfile.Statistics[i].Sum);
                    _correctedStatistics[i].StandardDeviation = Math.Abs(_statistics[i].StandardDeviation - _correctionProfile.Statistics[i].StandardDeviation);
                }
            }
        }

        public void SetCorrectionProfile(Spectrum profile, Func<Spectrum, int, bool> applicableItemSelector)
        {
            if (profile == null)
            {
                _correctionProfile = null;
                _correctionApplicableItemSelector = null;
                return;
            }

            _correctionProfile = profile;
            if (profile.Size != Size)
            {
                throw new InvalidOperationException("Correction profile window size does not match the current one. ");
            }
            if (profile.MaxFrequency != MaxFrequency)
            {
                throw new InvalidOperationException("Correction profile sample rate does not match the current one. ");
            }

            if (applicableItemSelector == null)
            {
                throw new ArgumentNullException(nameof(applicableItemSelector));
            }
            _correctionApplicableItemSelector = applicableItemSelector;

            UpdateCorrectedStatistics();
        }

        public IEnumerable<int> GetFrequencyIndices(double frequency, int windowHalfSize)
        {
            var index = (int)Math.Round(frequency * (double)Size / MaxFrequency);
            return Enumerable.Range(index - windowHalfSize, windowHalfSize * 2 + 1);
        }

        /* TODO: windowHalfSize greater than zero is not tested */
        public IEnumerable<StatisticsItem> AtFrequency(double frequency, int windowHalfSize = 0)
        {
            return GetFrequencyIndices(frequency, windowHalfSize).Select(i => Statistics[i]);
        }

        public double ValueAtFrequency(double frequency, Func<StatisticsItem, double> selector, int windowHalfSize = 0)
        {
            return AtFrequency(frequency, windowHalfSize).Sum(i => selector(i));
        }

        public Func<StatisticsItem, double> GetDefaultValueSelector()
        {
            if (DefaultValue == DefaultValueType.Last)
            {
                return s => s != null ? s.LastValue : 0.0;
            }
            else if (DefaultValue == DefaultValueType.Mean)
            {
                return s => s != null ? s.Mean : 0.0;
            }

            throw new Exception();
        }

        public void Reset()
        {
            _statistics = new StatisticsItem[Size];
            for (var i = 0; i < Size; i++)
            {
                _statistics[i] = new StatisticsItem();
            }

            _correctedStatistics = null;

            Count = 0;
        }

        public void SetAtFrequency(double frequency, double value, Expression<Func<StatisticsItem, double>> selector)
        {            
            var index = GetFrequencyIndices(frequency, 0).First();
            SetAtIndex(index, value, selector);  
        }

        private void SetAtIndex(int index, double value, Expression<Func<StatisticsItem, double>> selector)
        {
            var prop = (PropertyInfo)((MemberExpression)selector.Body).Member;
            prop.SetValue(Statistics[index], value);
        }

        public void Interpolate(double startFrequency, double endFrequency, Expression<Func<StatisticsItem, double>> selector)
        {
            var startIndex = GetFrequencyIndices(startFrequency, 0).First();
            var endIndex = GetFrequencyIndices(endFrequency, 0).First();
            var l = selector.Compile();

            var v0 = l(Statistics[startIndex]);
            var v1 = l(Statistics[endIndex]);

            var k = (v1 - v0) / (endIndex - startIndex);

            for (var i = startIndex + 1; i < endIndex - 1; i++)
            {
                var v = v0 + k * (i - startIndex);
                SetAtIndex(i, v, selector);
            }
        }
    }
}
