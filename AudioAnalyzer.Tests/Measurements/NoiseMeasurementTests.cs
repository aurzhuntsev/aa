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

        private NoiseMeasurement CreateNoiseMeasurement() 
        {
            const int size = 1000;            

            var data = new SpectralData(size, size);
            var arr = new double[size];
            arr[100] = 0.1;
            arr[200] = 0.1;
            arr[300] = 0.1;
            arr[400] = 0.1;

            data.Set(arr);

            var result = new NoiseAnalysisResult()
            {
                Data = data
            };

            var settings = new NoiseMeasurementSettings()
            {                
            };

            return new NoiseMeasurement(settings, result);
        }

        [Test]
        public void ShouldProperlyComputeNoisePower()
        {
            var msmt = CreateNoiseMeasurement();
            msmt.UpdateAnalysisResult();

            var result = msmt.AnalysisResult as NoiseAnalysisResult;
            Assert.LessOrEqual(Math.Abs(-20.0 * Math.Log10(1.0 / Math.Sqrt(0.04)) - result.NoisePowerDbFs), double.Epsilon);
            Assert.LessOrEqual(Math.Abs(-20.0 * Math.Log10(1.0 / (0.4 / 1000.0)) - result.AverageLevelDbTp), double.Epsilon);
        }

        [Test]
        public void ShouldApplyHighFrequencyLimit()
        {
            var msmt = CreateNoiseMeasurement();

            msmt.Settings.LimitHighFrequency = true;
            msmt.Settings.HighFrequency = 200;

            msmt.UpdateAnalysisResult();

            var result = msmt.AnalysisResult as NoiseAnalysisResult;
            Assert.LessOrEqual(Math.Abs(-20.0 * Math.Log10(1.0 / Math.Sqrt(0.01)) - result.NoisePowerDbFs), double.Epsilon);
            Assert.LessOrEqual(Math.Abs(-20.0 * Math.Log10(1.0 / (0.1 / 200.0)) - result.AverageLevelDbTp), double.Epsilon);
        }
    }
}
