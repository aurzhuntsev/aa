using AudioMark.Core.Common;
using AudioMark.Core.Fft;
using AudioMark.Core.Measurements.Analysis;
using AudioMark.Core.Measurements.Settings;
using AudioMark.Core.Settings;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioAnalyzer.Tests.Measurements
{
    public class ImdMeasurementTests
    {
        [SetUp]
        public void Setup()
        {
            AppSettings.TestMode = true;
        }

        public (Spectrum, ImdModMeasurementSettings) CreateImdMeasurement()
        {
            const int size = 1000;
            const int f1 = 100;
            const int f2 = 110;

            var data = new Spectrum(size, size);
            var arr = new double[size];

            arr[f1] = 0.1;
            arr[f2] = 0.1;

            // 2nd
            arr[-f1 + f2] = 0.1; // 10
            arr[f1 + f2] = 0.1; // 210

            // 3rd
            arr[2 * f1 - f2] = 0.1; // 90
            arr[2 * f1 + f2] = 0.1; // 310
            arr[-2 * f1 + 2 * f2] = 0.1; // 20
            arr[-f1 + 2 * f2] = 0.1; // 120
            arr[f1 + 2 * f2] = 0.1; // 320
            arr[2 * f1 + 2 * f2] = 0.1; // 420

            // random item represeting the noise
            arr[444] = 0.1;

            data.Set(arr);

            var settings = new ImdModMeasurementSettings()
            {
                TestSignalOptions = new AudioMark.Core.Measurements.Settings.Common.SignalSettings()
                {
                    Frequency = f1,
                },
                SecondarySignalFrequency = f2,
                MaxOrder = 3
            };

            return (data, settings);
        }

        [Test]
        public void ShouldCorrectlyAnalyzeImdWithNoFreqLimitAnd3rdOrder()
        {
            var p = CreateImdMeasurement();
            var m = (new ImdAnalytics()).Analyze(p.Item1, p.Item2) as ImdAnalysisResult;

            // 2 x 2nd order + 6 x x3rd order + 1 x noise = 9
            Assert.LessOrEqual(Math.Abs(m.TotalImdPlusNoiseDb - (-Math.Sqrt(Math.Pow(0.1, 2.0) * 9.0) / Math.Sqrt(Math.Pow(0.1, 2.0) * 11.0)).ToDbTp()), double.Epsilon);
            Assert.LessOrEqual(Math.Abs(m.ImdF2ForGivenOrderDb - (-Math.Sqrt(Math.Pow(0.2, 2.0) + Math.Pow(0.6, 2.0)) / 0.1).ToDbTp()), double.Epsilon);
            Assert.LessOrEqual(Math.Abs(m.ImdF1F2ForGivenOrderDb - (-Math.Sqrt(Math.Pow(0.2, 2.0) + Math.Pow(0.6, 2.0)) / 0.2).ToDbTp()), double.Epsilon);

            Assert.LessOrEqual(Math.Abs(-m.OrderedImd[2] - 0.2.ToDbTp()), double.Epsilon);
            Assert.LessOrEqual(Math.Abs(-m.OrderedImd[3] - 0.6.ToDbTp()), double.Epsilon);
        }

        [Test]
        public void ShouldCorrectlyAnalyzeImdWithNoFreqLimitAnd2ndOrder()
        {
            var p = CreateImdMeasurement();
            p.Item2.MaxOrder = 2;
            var m = (new ImdAnalytics()).Analyze(p.Item1, p.Item2) as ImdAnalysisResult;

            Assert.LessOrEqual(Math.Abs(m.TotalImdPlusNoiseDb - (-Math.Sqrt(Math.Pow(0.1, 2.0) * 9.0) / Math.Sqrt(Math.Pow(0.1, 2.0) * 11.0)).ToDbTp()), double.Epsilon);
            Assert.LessOrEqual(Math.Abs(m.ImdF2ForGivenOrderDb - (-Math.Sqrt(Math.Pow(0.2, 2.0)) / 0.1).ToDbTp()), double.Epsilon);
            Assert.LessOrEqual(Math.Abs(m.ImdF1F2ForGivenOrderDb - (-Math.Sqrt(Math.Pow(0.2, 2.0)) / 0.2).ToDbTp()), double.Epsilon);

            Assert.LessOrEqual(Math.Abs(-m.OrderedImd[2] - 0.2.ToDbTp()), double.Epsilon);
            Assert.True(!m.OrderedImd.ContainsKey(3));
        }

        [Test]
        public void ShouldCorrectlyAnalyzeImdWithFreqLimitAnd3rdOrder()
        {
            var p = CreateImdMeasurement();
            p.Item2.LimitMaxFrequency = true;
            p.Item2.MaxFrequency = 300;

            var m = (new ImdAnalytics()).Analyze(p.Item1, p.Item2) as ImdAnalysisResult;

            Assert.LessOrEqual(Math.Abs(m.TotalImdPlusNoiseDb - (-Math.Sqrt(Math.Pow(0.1, 2.0) * 5.0) / Math.Sqrt(Math.Pow(0.1, 2.0) * 7.0)).ToDbTp()), double.Epsilon);
            Assert.LessOrEqual(Math.Abs(m.ImdF2ForGivenOrderDb - (-Math.Sqrt(Math.Pow(0.2, 2.0) + Math.Pow(0.3, 2.0)) / 0.1).ToDbTp()), double.Epsilon);
            Assert.LessOrEqual(Math.Abs(m.ImdF1F2ForGivenOrderDb - (-Math.Sqrt(Math.Pow(0.2, 2.0) + Math.Pow(0.3, 2.0)) / 0.2).ToDbTp()), double.Epsilon);
        }
    }
}
