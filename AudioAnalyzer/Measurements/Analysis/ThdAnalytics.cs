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
            result.FundamentalFrequency = f;

            var fi = data.GetFrequencyIndices(f, thdSettings.WindowHalfSize);
            
            var frss = data.RssAtFrequency(f, x => x.Mean, thdSettings.WindowHalfSize);
            result.FundamentalDb = frss;

            double totalThd = 0.0;
            double total = 0.0;

            var maxFrequency = thdSettings.LimitMaxFrequency ? thdSettings.MaxFrequency : data.MaxFrequency;
            var right = data.GetFrequencyIndices(maxFrequency, 1).First();
            result.Bandwidth = maxFrequency;

            for (var i = 0; i < right; i++)
            {
                total += Math.Pow(data.Statistics[i].Mean, 2.0);

                if (!fi.Contains(i))
                {
                    totalThd += Math.Pow(data.Statistics[i].Mean, 2.0);
                }
            }
            totalThd = Math.Sqrt(totalThd) / Math.Sqrt(total);

            result.ThdNPercentage = 100.0 * totalThd;
            result.ThdNDb = -totalThd.ToDbTp();

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

            result.Harmonics = new Dictionary<int, double>();
            for (var i = 0; i < harmonics.Count; i++)
            {
                result.Harmonics.Add(i + 2, -harmonics[i].ToDbTp());
            }
            result.NumberOfHarmonics = harmonics.Count();

            return result;
        }
    }
}
