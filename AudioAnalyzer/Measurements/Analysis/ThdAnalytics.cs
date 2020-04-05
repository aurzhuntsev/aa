using AudioMark.Core.Common;
using AudioMark.Core.Measurements.Settings;
using AudioMark.Core.Measurements.Settings.Common;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioMark.Core.Measurements.Analysis
{
    public class ThdAnalytics : IAnalytics
    {
        public IAnalysisResult Analyze(Spectrum data, IMeasurementSettings settings)
        {
            var thdSettings = settings as ThdMeasurementSettings;
            if (thdSettings == null)
            {
                throw new InvalidOperationException();
            }

            var result = new ThdAnalysisResult();
            result.Data = data;

            var f = thdSettings.TestSignalOptions.Frequency;
            var fi = data.GetFrequencyIndices(f, thdSettings.WindowHalfSize);
            var frss = data.RssAtFrequency(f, x => x.Mean, thdSettings.WindowHalfSize);
            double totalThd = 0.0;
            double total = 0.0;

            var maxFrequency = thdSettings.LimitMaxFrequency ? thdSettings.MaxFrequency : data.MaxFrequency;

            for (var i = 0; i < maxFrequency; i++)
            {
                total += Math.Pow(data.Statistics[i].Mean, 2.0);

                if (!fi.Contains(i))
                {
                    totalThd += Math.Pow(data.Statistics[i].Mean, 2.0);
                }
            }
            totalThd = Math.Sqrt(totalThd) / Math.Sqrt(total);

            result.TotalThdPlusNoisePercentage = 100.0 * totalThd;
            result.TotalThdPlusNoiseDb = -totalThd.ToDbTp();

            var freq = 2.0 * f;
            var harm = 2;
            List<double> harmonics = new List<double>();

            while (freq < maxFrequency)
            {               
                harmonics.Add(data.RssAtFrequency(freq, x => x.Mean, thdSettings.WindowHalfSize));
                harm++;
                freq = harm * thdSettings.TestSignalOptions.Frequency;

                if (thdSettings.LimitMaxHarmonics)
                {
                    if (harm - 1 > thdSettings.MaxHarmonics)
                    {
                        break;
                    }
                }
            }
            
            var thdf = Math.Sqrt(harmonics.Select(x => Math.Pow(x, 2.0)).Sum()) / frss;
            var thdr = thdf / Math.Sqrt(1.0 + thdf * thdf);

            result.ThdFPercentage = 100.0 * thdf;
            result.ThdFDb = -thdf.ToDbTp();

            result.ThdRPercentage = 100.0 * thdr;
            result.ThdRDb = -thdr.ToDbTp();            

            return result;
        }
    }
}
