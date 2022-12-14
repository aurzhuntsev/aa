using AudioMark.Core.Common;
using AudioMark.Core.Fft;
using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements.StopConditions
{
    public class ToleranceAchievedStopCondition : IStopCondition
    {
        public class Record
        {
            public double Value { get; set; }
            public long Time { get; set; }

            public Record(double value, long time)
            {
                Value = value;
                Time = time;
            }
        }

        private const int MinimalNormalCdfSampleSize = 30;

        public Spectrum Data { get; private set; }
        public double Tolerance { get; private set; }
        public double Confidence { get; private set; }

        private List<Record>[] _records;
        private DateTime _lastUpdated;

        private TimeSpan? _remaining = null;
        
        public TimeSpan? Remaining
        {
            get => _remaining;
        }

        public ToleranceAchievedStopCondition(Spectrum data, double tolerance, double confidence)
        {
            Data = data;
            Tolerance = tolerance;
            Confidence = confidence;

            Set();
        }

        private double EstimateRemainingTime(List<Record> r, double target)
        {
            /* hello fifth grade */
            var t2 = (r[2].Time - r[1].Time);
            if (t2 == 0)
            {
                return double.NaN;
            }

            var t1 = (r[1].Time - r[0].Time);
            if (t1 == 0)
            {
                return double.NaN;
            }

            var v2 = (r[2].Value - r[1].Value) / (double)t2;
            var v1 = (r[1].Value - r[0].Value) / (double)t1;
            var a = (v1 - v2) / (0.5 * (t2 + t1));
            var s = target - r[2].Value;
            if (a == 0)
            {
                return s / v2;
            }

            var d = v2 * v2 - 2.0 * a * s;
            if (d >= 0)
            {
                var sqrtd = Math.Sqrt(d);
                var ia = -1.0 / a;
                return Math.Max(ia * (-v2 + sqrtd), ia * (-v2 - sqrtd));
            }

            return double.NaN;
        }

        public bool Check()
        {
            try
            {
                if (Data.Count < 2)
                {
                    return false;
                }

                var k = StudentT.InvCDF(0.0, 1.0, Data.Count - 1, 0.5 * (Confidence + 1.0)) / Math.Sqrt(Data.Count);
                var time = DateTime.Now.Ticks;

                var hasMissingCondition = false;
                var maxRemaining = long.MinValue;
                for (var i = 0; i < Data.Size; i++)
                {
                    var confidenceInterval = k * Data.Statistics[i].StandardDeviation;
                    var toleranceInterval = Data.Statistics[i].Mean * Tolerance;

                    if (toleranceInterval - confidenceInterval < double.Epsilon && toleranceInterval != 0 && confidenceInterval != 0)
                    {
                        hasMissingCondition = true;

                        var recordsList = _records[i];
                        if (recordsList.Count == 3)
                        {
                            recordsList.RemoveAt(0);
                        }
                        recordsList.Add(new Record(confidenceInterval, time));

                        if (recordsList.Count == 3)
                        {
                            var timeRemaining = EstimateRemainingTime(recordsList, toleranceInterval);
                            if (!double.IsNaN(timeRemaining) && !double.IsInfinity(timeRemaining) && maxRemaining < timeRemaining)
                            {
                                maxRemaining = (long)timeRemaining;
                            }
                        }
                    }
                }

                if (!hasMissingCondition)
                {                    
                    return true;
                }

                if (maxRemaining != long.MinValue)
                {
                    /* TODO: Make it more viable; maybe don't show an estimate if more than 60 minutes is remaining or smth like that */
                    if (_remaining == null || _remaining.Value.Ticks > maxRemaining)
                    {
                        _remaining = new TimeSpan(maxRemaining);
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                return true;
            }
        }

        public void Set()
        {
            _records = new List<Record>[Data.Size];
            for (var i = 0; i < _records.Length; i++)
            {
                _records[i] = new List<Record>(3);
            }
        }
    }
}
