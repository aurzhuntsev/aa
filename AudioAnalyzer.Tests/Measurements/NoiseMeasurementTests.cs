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
    public class NoiseMeasurementTests
    {
        [SetUp]
        public void Setup()
        {
            AppSettings.TestMode = true;
        }

        private (Spectrum, NoiseMeasurementSettings) CreateNoiseMeasurement() 
        {
            const int size = 1000;            

            var data = new Spectrum(size, size);
            var arr = new double[size];
            arr[100] = 0.1;
            arr[200] = 0.1;
            arr[300] = 0.1;
            arr[400] = 0.1;

            data.Set(arr);

            var settings = new NoiseMeasurementSettings()
            {                
            };

            return (data, settings);
        }

        [Test]
        public void ShouldProperlyComputeNoisePower()
        {
            var msmt = CreateNoiseMeasurement();

            msmt.Item2.LimitHighFrequency = true;
            msmt.Item2.HighFrequency = 1000;

            var result = (new NoiseAnalytics()).Analyze(msmt.Item1, msmt.Item2) as NoiseAnalysisResult;
            Assert.LessOrEqual(Math.Abs(-10.0 * Math.Log10(1.0 / Math.Sqrt(0.04 / 1000.0)) - result.NoisePowerDbFs), double.Epsilon);
            Assert.LessOrEqual(Math.Abs(-20.0 * Math.Log10(1.0 / (0.4 / 1000.0)) - result.AverageLevelDbTp), double.Epsilon);
        }

        [Test]
        public void ShouldApplyHighFrequencyLimit()
        {
            var msmt = CreateNoiseMeasurement();

            msmt.Item2.LimitHighFrequency = true;
            msmt.Item2.HighFrequency = 200;

            var result = (new NoiseAnalytics()).Analyze(msmt.Item1, msmt.Item2) as NoiseAnalysisResult;
            Assert.LessOrEqual(Math.Abs(-10.0 * Math.Log10(1.0 / Math.Sqrt(0.01 / 200.0)) - result.NoisePowerDbFs), double.Epsilon);
            Assert.LessOrEqual(Math.Abs(-20.0 * Math.Log10(1.0 / (0.1 / 200.0)) - result.AverageLevelDbTp), double.Epsilon);
        }
    }
}
