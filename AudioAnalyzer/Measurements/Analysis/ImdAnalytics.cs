using AudioMark.Core.Common;
using AudioMark.Core.Fft;
using AudioMark.Core.Measurements.Settings;
using AudioMark.Core.Measurements.Settings.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioMark.Core.Measurements.Analysis
{
    public class ImdAnalytics : IAnalytics
    {
        public IAnalysisResult Analyze(Spectrum data, IMeasurementSettings settings)
        {
            var imdSettings = settings as IImdSettings;
            if (imdSettings == null)
            {
                throw new InvalidOperationException();
            }

            var result = new ImdAnalysisResult();
            result.Data = data;

            var maxFrequency = imdSettings.LimitMaxFrequency ? imdSettings.MaxFrequency : data.MaxFrequency;
            var f1 = imdSettings.F1Frequency;
            var f2 = imdSettings.F2Frequency;
            var f1i = data.GetFrequencyIndices(f1, imdSettings.WindowHalfSize);
            var f2i = data.GetFrequencyIndices(f2, imdSettings.WindowHalfSize);
            var f1rss = data.ValueAtFrequency(f1, x => x.Mean, imdSettings.WindowHalfSize);
            var f2rss = data.ValueAtFrequency(f2, x => x.Mean, imdSettings.WindowHalfSize);

            /* Total IMD+Noise is full bandwidth except F1 and F2 frequencies */
            double totalImd = 0.0;
            double total = 0.0;
            var right = data.GetFrequencyIndices(maxFrequency, 0).First();
            for (var f = 0; f < right; f++)
            {
                total += Math.Pow(data.Statistics[f].Mean, 2.0);

                if (!f1i.Contains(f) && !f2i.Contains(f))
                {
                    totalImd += Math.Pow(data.Statistics[f].Mean, 2.0);
                }                
            }
            totalImd = Math.Sqrt(totalImd) / Math.Sqrt(total);

            result.TotalImdPlusNoisePercentage = 100.0 * totalImd;
            result.TotalImdPlusNoiseDb = -totalImd.ToDbTp();

            var orders = new Dictionary<int, double>();
            var order = imdSettings.MaxOrder;

            for (var n = -order + 1; n < order; n++)
            {
                for (var m = -order + 1; m < order; m++)
                {
                    if (n == 0 || m == 0)
                    {
                        continue;
                    }

                    var o = Math.Max(Math.Abs(n), Math.Abs(m)) + 1;
                    var f = n * f1 + m * f2;
                    if (f > 0 && f < maxFrequency)
                    {
                        var imd = data.ValueAtFrequency(f, x => x.Mean, imdSettings.WindowHalfSize);
                        if (!orders.ContainsKey(o))
                        {
                            orders.Add(o, imd);
                        }
                        else
                        {
                            orders[o] += imd;
                        }
                    }
                }
            }

            result.F1Db = -f1rss.ToDbTp();
            result.F2Db = -f2rss.ToDbTp();
            result.F1Frequency = f1;
            result.F2Frequency = f2;

            result.MaxOrder = imdSettings.MaxOrder;
            result.Bandwidth = maxFrequency;

            var sumOfOrders = 0.0;
            for (var i = 2; i <= order; i++)
            {
                sumOfOrders += Math.Pow(orders[i], 2.0);
                orders[i] = -orders[i].ToDbTp();
            }
            sumOfOrders = Math.Sqrt(sumOfOrders);
            result.OrderedImd = orders.OrderBy(o => o.Key).ToDictionary(k => k.Key, v => v.Value); 

            result.ImdF2ForGivenOrderPercentage = (sumOfOrders / f1rss) * 100.0;
            result.ImdF2ForGivenOrderDb = -(sumOfOrders / f1rss).ToDbTp();

            result.ImdF1F2ForGivenOrderPercentage = (sumOfOrders / (f1rss + f2rss)) * 100.0;
            result.ImdF1F2ForGivenOrderDb = -(sumOfOrders / (f1rss + f2rss)).ToDbTp();

            return result;
        }
    }
}
