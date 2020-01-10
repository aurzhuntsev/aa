using AudioMark.Core.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioAnalyzer.Tests.Common
{
    public class SpectralDataTests
    {
        [Test]
        public void ShouldPopulateCorrectSpectralDataStats()
        {
            var data = new SpectralData(1, 1);
            for (var i = 1; i < 10; i++)
            {
                data.Set(new double[] { i });
            }

            var s = data.Statistics[0];
            Assert.AreEqual(s.LastValue, 9);
            Assert.AreEqual(s.Max, 9);
            Assert.AreEqual(s.Min, 1);
            Assert.AreEqual(s.Mean, 5);
            Assert.AreEqual(s.Sum, 45);            
            Assert.LessOrEqual(2.7386127875258306 - s.StandardDeviation, double.Epsilon);
        }
    }
}
