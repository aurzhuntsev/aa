using AudioMark.Core.Fft;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioAnalyzer.Tests.Fft
{
    public class WindowsTests
    {
        private static double[] _hann1 =
        {   0, 0.0102, 0.0405, 0.0896, 0.1555, 0.2355, 0.3263, 0.4243, 0.5253, 0.6253,
            0.7202, 0.8061, 0.8794, 0.9372, 0.9771, 0.9974, 0.9974, 0.9771,
            0.9372, 0.8794, 0.8061, 0.7202, 0.6253, 0.5253, 0.4243, 0.3263,
            0.2355, 0.1555, 0.0896, 0.0405, 0.0102, 0
        };

        private static double[] _flatTop1 =
        {
            -0.0004, -0.0015, -0.0056, -0.0144, -0.0290, -0.0480, -0.0654, -0.0695,
            -0.0450, 0.0237, 0.1457, 0.3182, 0.5228, 0.7277, 0.8942, 0.9878,
            0.9878, 0.8942, 0.7277, 0.5228, 0.3182, 0.1457, 0.0237, -0.0450,
            -0.0695, -0.0654, -0.0480, -0.0290, -0.0144, -0.0056, -0.0015, -0.0004
        };

        // n=32, nbars=7, sll=--50dB
        private static double[] _taylor1 =
        {
            0.0947, 0.1320, 0.2022, 0.2992, 0.4183, 0.5570, 0.7133, 0.8837,
            1.0626, 1.2430, 1.4172, 1.5778, 1.7174, 1.8289, 1.9065, 1.9462,
            1.9462, 1.9065, 1.8289, 1.7174, 1.5778, 1.4172, 1.2430, 1.0626,
            0.8837, 0.7133, 0.5570, 0.4183, 0.2992, 0.2022, 0.1320, 0.0947
        };

        [Test]
        public void ShouldProperlyComputeTaylorWindow()
        {
            var result1 = WindowsHelper.Taylor(32, 7, -50);
            Assert.AreEqual(result1.Length, _taylor1.Length);

            for (var i = 0; i < result1.Length; i++)
            {
                Assert.LessOrEqual(Math.Abs(Math.Round(result1[i], 4) - _taylor1[i]), double.Epsilon);
            }
        }

        [Test]
        public void ShouldProperlyComputeHannWindow()
        {
            var result1 = WindowsHelper.Hann(32);
            Assert.AreEqual(result1.Length, _hann1.Length);

            for (var i = 0; i < result1.Length; i++)
            {
                Assert.LessOrEqual(Math.Abs(Math.Round(result1[i], 4) - _hann1[i]), double.Epsilon);
            }
        }

        [Test]
        public void ShouldProperlyComputeFlatTopWindow()
        {
            var result1 = WindowsHelper.FlatTop(32);
            Assert.AreEqual(result1.Length, _flatTop1.Length);

            for (var i = 0; i < result1.Length; i++)
            {
                Assert.LessOrEqual(Math.Abs(Math.Round(result1[i], 4) - _flatTop1[i]), double.Epsilon);
            }
        }
    }
}
