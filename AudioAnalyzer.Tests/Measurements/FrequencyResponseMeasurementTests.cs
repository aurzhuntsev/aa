using AudioMark.Core.Common;
using AudioMark.Core.Fft;
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
    public class FrequencyResponseMeasurementTests
    {
        [SetUp]
        public void Setup()
        {
            AppSettings.TestMode = true;
        }

        private (Spectrum, FrequencyResponseMeasurementSettings) CreateFrequencyResponseMeasurement() 
        {
            const int size = 1000;            

            var data = new Spectrum(size, size);
            var arr = new double[size];

            for (var i = 0; i < arr.Length; i++)
            {
                arr[i] = 0.1;
            }

            arr[100] = 0.05;
            arr[200] = 0.2;
            
            data.Set(arr);

            var settings = new FrequencyResponseMeasurementSettings()
            {                
            };

            return (data, settings);
        }       

        [Test]
        public void ShouldProperlyComputeFrequencyResponseAnalytics()
        {
            var msmt = CreateFrequencyResponseMeasurement();
            var result = (new FrequencyResponseAnalytics()).Analyze(msmt.Item1, msmt.Item2) as FrequencyResponseAnalysisResult;

            Assert.AreEqual(result.MinValueDb, -0.05.ToDbTp());
            Assert.AreEqual(result.MinValueFrequency, 100.0);
            Assert.AreEqual(result.MaxValueDb, -0.2.ToDbTp());
            Assert.AreEqual(result.MaxValueFrequency, 200.0);
            Assert.AreEqual(result.RippleDb, -0.2.ToDbTp() + 0.05.ToDbTp());
        }
    }
}
