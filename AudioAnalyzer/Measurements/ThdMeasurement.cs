using AudioMark.Core.Common;
using AudioMark.Core.Generators;
using PortAudioWrapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AudioMark.Core.Settings;
using AudioMark.Core.AudioData;
using System.Diagnostics;
using MathNet.Numerics;
using System.Linq;
using AudioMark.Core.Measurements.Settings;
using AudioMark.Core.Measurements.Analysis;
using AudioMark.Core.Measurements.Common;
using AudioMark.Core.Measurements.StopConditions;

namespace AudioMark.Core.Measurements
{
    [Measurement("Total Harmonic Distortion")]
    public class ThdMeasurement : MeasurementBase<SpectralData>
    {
        private Activity<SpectralData> _dataActivity;
        public SineGenerator TestSignalGenerator { get; set; }        

        public override SpectralData Result => AnalysisResult?.Data;

        public new ThdMeasurementSettings Settings
        {
            get => (ThdMeasurementSettings)base.Settings;
        }

        public ThdMeasurement(ThdMeasurementSettings settings) : base(settings)
        {
            Name = $"{DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss")} - THD@{Settings.TestSignalOptions.Frequency}hz";
        }

        public ThdMeasurement(ThdMeasurementSettings settings, ThdAnalysisResult result) : base(settings, result) { }

        protected override void Initialize()
        {
            RegisterActivity(CreateSetupActivity());

            TestSignalGenerator = new SineGenerator(AppSettings.Current.Device.SampleRate, Settings.TestSignalOptions.Frequency, Settings.TestSignalOptions.InputOutputOptions.OutputLevel.FromDbTp());
            
            if (Settings.WarmUpEnabled && Settings.WarmUpDurationSeconds > 0)
            {
                RegisterActivity(CreateWarmUpActivity());
            }

            _dataActivity = CreateAcquisitionActivity();
            
            RegisterActivity(_dataActivity);
        }

        private Activity<SpectralData> CreateSetupActivity()
        {
            const int SetupActivityTimeoutMilliseconds = 1000;

            var setupActivity = new Activity<SpectralData>("Setting up...");
            setupActivity.AddGenerator(AppSettings.Current.Device.PrimaryOutputChannel, new SilenceGenerator());

            var sink = new SpectralDataProcessor(Settings.Fft.Value.WindowSize, Settings.Fft.Value.WindowOverlapFactor, AppSettings.Current.Device.SampleRate / 2)
            {
                Silent = true
            };
            setupActivity.AddSink(AppSettings.Current.Device.PrimaryInputChannel, sink);
            setupActivity.RegisterStopCondition(new TimeoutStopCondition(SetupActivityTimeoutMilliseconds));

            return setupActivity;
        }

        private Activity<SpectralData> CreateWarmUpActivity()
        {
            var warmUpActivity = new Activity<SpectralData>("Warming up...");
            warmUpActivity.AddGenerator(AppSettings.Current.Device.PrimaryOutputChannel, TestSignalGenerator);

            var sink = new SpectralDataProcessor(Settings.Fft.Value.WindowSize, Settings.Fft.Value.WindowOverlapFactor, AppSettings.Current.Device.SampleRate / 2);
            sink.Data.DefaultValue = SpectralData.DefaultValueType.Last;
            warmUpActivity.AddSink(AppSettings.Current.Device.PrimaryInputChannel, sink);

            warmUpActivity.RegisterStopCondition(new TimeoutStopCondition(Settings.WarmUpDurationSeconds * 1000));

            return warmUpActivity;
        }

        /* TODO: Test and Warmup activities are the same, refactor */
        private Activity<SpectralData> CreateAcquisitionActivity()
        {
            var dataActivity = new Activity<SpectralData>("Acquiring data...");
            dataActivity.AddGenerator(AppSettings.Current.Device.PrimaryOutputChannel, TestSignalGenerator);

            var sink = new SpectralDataProcessor(Settings.Fft.Value.WindowSize, Settings.Fft.Value.WindowOverlapFactor, AppSettings.Current.Device.SampleRate / 2);
            sink.Data.DefaultValue = SpectralData.DefaultValueType.Mean;
            SetCorrectionProfile(sink.Data);

            dataActivity.AddSink(AppSettings.Current.Device.PrimaryInputChannel, sink);

            Settings.ApplyStopConditions(dataActivity, sink.Data);

            return dataActivity;
        }

        private void SetCorrectionProfile(SpectralData target)
        {
            if (Settings.ApplyCorrectionProfile && Settings.CorrectionProfile != null)
            {
                target.SetCorrectionProfile(Settings.CorrectionProfile, (data, index) =>
                    !data.GetFrequencyIndices(Settings.TestSignalOptions.Frequency, Settings.WindowHalfSize).Contains(index));
            }
            else
            {
                target.SetCorrectionProfile(null, null);
            }
        }

        public override void UpdateAnalysisResult()
        {
            var result = new ThdAnalysisResult();

            var data = Result == null ? ((SpectralDataProcessor)_dataActivity.GetSink(AppSettings.Current.Device.PrimaryInputChannel)).Data : Result;
            SetCorrectionProfile(data);

            result.Data = data;

            var first = data.AtFrequency(Settings.TestSignalOptions.Frequency, Settings.WindowHalfSize);
            var evens = new List<SpectralData.StatisticsItem>();
            var odds = new List<SpectralData.StatisticsItem>();

            var harmonic = 2;
            var freq = 2 * Settings.TestSignalOptions.Frequency;

            while (freq < data.MaxFrequency)
            {
                var label = string.Empty;
                if (harmonic == 2)
                {
                    label = "2nd";
                }
                else if (harmonic == 3)
                {
                    label = "3rd";
                }
                else
                {
                    label = harmonic + "th";
                }

                var values = data.AtFrequency(freq, Settings.WindowHalfSize).ToList();
                values[(int)Math.Ceiling((double)values.Count() / 2) - 1].Label = $"{label}";

                if (harmonic.IsEven())
                {
                    evens.AddRange(values);
                }
                else
                {
                    odds.AddRange(values);
                }

                harmonic++;
                freq = harmonic * Settings.TestSignalOptions.Frequency;

                if (Settings.LimitMaxFrequency)
                {
                    if (freq > Settings.MaxFrequency)
                    {
                        break;
                    }
                }

                if (Settings.LimitMaxHarmonics)
                {
                    if (harmonic - 1 > Settings.MaxHarmonics)
                    {
                        break;
                    }
                }
            }

            var baseSquareSum = first.Sum(x => x.Mean * x.Mean);
            var evensSquareSum = evens.Sum(x => x.Mean * x.Mean);
            var oddsSquareSum = odds.Sum(x => x.Mean * x.Mean);

            var thdf = Math.Sqrt(evensSquareSum + oddsSquareSum) / Math.Sqrt(baseSquareSum);
            var thdr = thdf / Math.Sqrt(1.0 + thdf * thdf);

            result.ThdFPercentage = 100.0 * thdf;
            result.ThdFDb = -20.0 * Math.Log10(1.0 / thdf);

            result.ThdRPercentage = 100.0 * thdr;
            result.ThdRDb = -20.0 * Math.Log10(1.0 / thdr);

            result.EvenToOdd = evensSquareSum / oddsSquareSum;

            AnalysisResult = result;
        }
    }
}
