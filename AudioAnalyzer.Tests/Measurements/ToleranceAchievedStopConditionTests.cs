using AudioMark.Core.Common;
using AudioMark.Core.Measurements;
using AudioMark.Core.Measurements.StopConditions;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AudioAnalyzer.Tests.Measurements
{
    public class ToleranceAchievedStopConditionTests
    {
        [Test]
        public void SHouldProperlyEstimateTimeRemaining()
        {
            var target = new ToleranceAchievedStopCondition(new Spectrum(1, 1), 0, 0);
            var methodInfo = target.GetType()
                .GetMethod("EstimateRemainingTime", BindingFlags.NonPublic | BindingFlags.Instance);

            var recordsList = new List<ToleranceAchievedStopCondition.Record>();
            recordsList.Add(new ToleranceAchievedStopCondition.Record(3.0, 0));
            recordsList.Add(new ToleranceAchievedStopCondition.Record(2.0, 1));
            recordsList.Add(new ToleranceAchievedStopCondition.Record(1.0, 2));

            var result = methodInfo.Invoke(target, new object[] { recordsList, 0.0 });

            Assert.AreEqual(result, 1);

            recordsList = new List<ToleranceAchievedStopCondition.Record>();
            recordsList.Add(new ToleranceAchievedStopCondition.Record(1, 0));
            recordsList.Add(new ToleranceAchievedStopCondition.Record(2, 1));
            recordsList.Add(new ToleranceAchievedStopCondition.Record(4, 2)); // a = 1

            result = Utility.InvokePrivateMethod("EstimateRemainingTime", target, new object[] { recordsList, 7.0 });                
            /* TODO: I have a strong feeling this is wrong oO */
            Assert.AreEqual(Math.Round((double)result, 2), 1.16);
        }

        [Test]
        public void ShouldMeetConditionUnderProperConditions()
        {
            var source = new[]
            {
                1,
                0.91718318202401661,
                1.0970023217597056,
                0.95831764305863421,
                1.0041640521977861,
                1.0705406552043466,
                0.96028158159939647,
                0.90059476918568593,
                1.0452920400282797,
                1.0570941757676631,
                100500,
                200500
            };

            var data = new Spectrum(1, 1);
            var target = new ToleranceAchievedStopCondition(data, 0.05, 0.95);
            var met = false;
            foreach (var s in source)
            {
                data.Set(new[] { s });
                met = target.Check();

                if (met)
                {
                    break;
                }
            }

            Assert.IsTrue(met);
            Assert.AreEqual(data.Count, 10);
        }
    }
}
