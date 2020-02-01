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
        private SpectralData _correctionProfile;

        [NonSerialized]
        private Func<SpectralData, int, bool> _correctionApplicableItemSelector;

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

        public SpectralData(int size, int maxFrequency)
        {
            Size = size;
            MaxFrequency = maxFrequency;

            _statistics = new StatisticsItem[Size];            
            for (var i = 0; i < size; i++)
            {
                _statistics[i] = new StatisticsItem();                
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

        public void SetCorrectionProfile(SpectralData profile, Func<SpectralData, int, bool> applicableItemSelector)
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

        public Func<StatisticsItem, double> GetDefaultValueSelector()
        {
            if (DefaultValue == DefaultValueType.Last)
            {
                return s => s.LastValue;
            }
            else if (DefaultValue == DefaultValueType.Mean)
            {
                return s => s.Mean;
            }

            throw new Exception();
        }
    }
}
