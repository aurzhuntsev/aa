using AudioMark.Core.Common;
using AudioMark.Core.Measurements;
using AudioMark.Core.Measurements.Analysis;
using AudioMark.Core.Measurements.Settings;
using AudioMark.Core.Settings;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioAnalyzer.Tests.Measurements
{
    public class ThdMeasurementTests
    {
        [SetUp]
        public void Setup()
        {
            AppSettings.TestMode = true;
        }

        private (Spectrum, ThdMeasurementSettings) Create100HzThdAnalyticsParams()
        {
            const int size = 1000;
            const int baseFreq = 100;

            var data = new Spectrum(size, size);
            var arr = new double[size];
            arr[baseFreq] = 1.0;
            arr[baseFreq + 1] = 1.0;
            arr[baseFreq - 1] = 1.0;

            arr[baseFreq * 2] = 0.2;
            arr[baseFreq * 3] = 0.2;
            arr[baseFreq * 4] = 0.2;
            arr[baseFreq * 5] = 0.2;

            arr[444] = 0.1;
            data.Set(arr);

            var settings = new ThdMeasurementSettings()
            {
                TestSignalOptions = new AudioMark.Core.Measurements.Settings.Common.SignalSettings()
                {
                    Frequency = baseFreq
                },
            };

            return (data, settings);
        }

        private void AssertResultMatchesThdOf(double value, ThdAnalysisResult result)
        {
            Assert.LessOrEqual(Math.Abs(100.0 * value - result.ThdFPercentage), double.Epsilon);
            Assert.LessOrEqual(Math.Abs(-20.0 * Math.Log10(1.0 / value) - result.ThdFDb), double.Epsilon);

            Assert.LessOrEqual(Math.Abs(100.0 * (value / Math.Sqrt(1.0 + value * value)) - result.ThdRPercentage), double.Epsilon);
            Assert.LessOrEqual(Math.Abs(-20.0 * Math.Log10(1.0 / (value / Math.Sqrt(1.0 + value * value))) - result.ThdRDb), double.Epsilon);
        }

        [Test]
        public void ShouldProperlyComputeThdN()
        {
            var p = Create100HzThdAnalyticsParams();
            var result = (new ThdAnalytics()).Analyze(p.Item1, p.Item2) as ThdAnalysisResult;

            Assert.LessOrEqual(Math.Abs(Math.Sqrt(0.17) / Math.Sqrt(1.17) - result.ThdNPercentage / 100.0), 1E-14d); // rounding issues :(
            Assert.LessOrEqual(Math.Abs(-(Math.Sqrt(0.17) / Math.Sqrt(1.17)).ToDbTp() - result.ThdNDb), 1E-14d);
        }

        [Test]
        public void ShouldCreateCorrectThdAnalysisReport()
        {
            var p = Create100HzThdAnalyticsParams();
            var result = (new ThdAnalytics()).Analyze(p.Item1, p.Item2) as ThdAnalysisResult;
            AssertResultMatchesThdOf(0.4, result);
        }

        [Test]
        public void ShouldCorrectlyApplyMaxFrequencyLimit()
        {
            var p = Create100HzThdAnalyticsParams();
            p.Item2.LimitMaxFrequency = true;
            p.Item2.MaxFrequency = 300.0;

            var result = (new ThdAnalytics()).Analyze(p.Item1, p.Item2) as ThdAnalysisResult;

            AssertResultMatchesThdOf(0.2, result);
        }

        [Test]
        public void ShouldCorrectlyApplyMaxHarmonicsLimit()
        {
            var p = Create100HzThdAnalyticsParams();
            p.Item2.LimitMaxHarmonics = true;
            p.Item2.MaxHarmonics = 2;

            var result = (new ThdAnalytics()).Analyze(p.Item1, p.Item2) as ThdAnalysisResult;
            AssertResultMatchesThdOf(0.2 * Math.Sqrt(2.0), result);
        }

        [Test]
        public void ShouldProperlyUseWindowSize()
        {
            var p = Create100HzThdAnalyticsParams();

            p.Item2.WindowHalfSize = 1;

            var result = (new ThdAnalytics()).Analyze(p.Item1, p.Item2) as ThdAnalysisResult;
            AssertResultMatchesThdOf(0.4 / Math.Sqrt(3.0), result);
        }
    }
}
