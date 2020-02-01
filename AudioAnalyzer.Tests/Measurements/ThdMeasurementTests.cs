using AudioMark.Core.Common;
using AudioMark.Core.Measurements;
using AudioMark.Core.Measurements.Analysis;
using AudioMark.Core.Measurements.Settings;
using AudioMark.Core.Settings;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using AudioMark.Core.Measurements.Common;

namespace AudioAnalyzer.Tests.Measurements
{
    public class ThdMeasurementTests
    {
        [SetUp]
        public void Setup()
        {
            AppSettings.TestMode = true;
        }

        private ThdMeasurement Create100HzThdMeasurement()
        {
            const int size = 1000;
            const int baseFreq = 100;

            var data = new SpectralData(size, size);
            var arr = new double[size];
            arr[baseFreq] = 1.0;
            arr[baseFreq + 1] = 1.0;
            arr[baseFreq - 1] = 1.0;

            arr[baseFreq * 2] = 0.2;
            arr[baseFreq * 3] = 0.2;
            arr[baseFreq * 4] = 0.2;
            arr[baseFreq * 5] = 0.2;
            data.Set(arr);

            var result = new ThdAnalysisResult()
            {
                Data = data
            };

            var settings = new ThdMeasurementSettings()
            {
                TestSignalOptions = new AudioMark.Core.Measurements.Settings.Common.SignalSettings()
                {
                    Frequency = baseFreq
                },
            };

            return new ThdMeasurement(settings, result);
        }

        private void AssertResultMatchesThdOf(double value, ThdAnalysisResult result)
        {            
            Assert.LessOrEqual(Math.Abs(100.0 * value - result.ThdFPercentage), double.Epsilon);
            Assert.LessOrEqual(Math.Abs(-20.0 * Math.Log10(1.0 / value) - result.ThdFDb), double.Epsilon);

            Assert.LessOrEqual(Math.Abs(100.0 * (value / Math.Sqrt(1.0 + value * value)) - result.ThdRPercentage), double.Epsilon);
            Assert.LessOrEqual(Math.Abs(-20.0 * Math.Log10(1.0 / (value / Math.Sqrt(1.0 + value * value))) - result.ThdRDb), double.Epsilon);         
        }

        [Test]
        public void ShouldCreateCorrectThdAnalysisReport()
        {
            var thd = Create100HzThdMeasurement();
            thd.UpdateAnalysisResult();

            var result = thd.AnalysisResult as ThdAnalysisResult;
            AssertResultMatchesThdOf(0.4, result);    
        }

        [Test]
        public void ShouldCorrectlyApplyMaxFrequencyLimit()
        {
            var thd = Create100HzThdMeasurement();
            thd.Settings.LimitMaxFrequency = true;
            thd.Settings.MaxFrequency = 300.0;

            thd.UpdateAnalysisResult();

            var result = thd.AnalysisResult as ThdAnalysisResult;
            AssertResultMatchesThdOf(0.2 * Math.Sqrt(2.0), result);
        }

        [Test]
        public void ShouldCorrectlyApplyMaxHarmonicsLimit()
        {
            var thd = Create100HzThdMeasurement();
            thd.Settings.LimitMaxHarmonics = true;
            thd.Settings.MaxHarmonics = 2;

            thd.UpdateAnalysisResult();

            var result = thd.AnalysisResult as ThdAnalysisResult;
            AssertResultMatchesThdOf(0.2 * Math.Sqrt(2.0), result);
        }

        [Test]
        public void ShouldProperlyUseWindowSize()
        {
            var thd = Create100HzThdMeasurement();

            thd.Settings.WindowHalfSize = 1;
            thd.UpdateAnalysisResult();

            var result = thd.AnalysisResult as ThdAnalysisResult;
            AssertResultMatchesThdOf(0.4 / Math.Sqrt(3.0), result);        
        }
    }
}
