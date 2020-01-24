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

namespace AudioMark.Core.Measurements
{
    [Serializable]
    public class InputOutputLevel
    {
        public double OutputLevel { get; set; } = -AppSettings.Current.Device.ClippingLevel;
        public bool MatchInputLevel { get; set; } = true;
        public double InputLevel { get; set; } = 3.0;
        public SignalLevelMode InputLevelMode { get; set; } = SignalLevelMode.dBFS;

        public override string ToString()
        {
            if (MatchInputLevel)
            {
                return $"In {InputLevel}{InputLevelMode}";
            }
            return $"Out {OutputLevel}dBTP";
        }
    }

    [Measurement("Total Harmonic Distortion")]
    public class ThdMeasurement : MeasurementBase<SpectralData>
    {
       
        private Activity<SpectralData> _dataActivity;
        public SineGenerator TestSignalGenerator { get; set; }
        public SineGenerator WarmupSignalGenerator { get; set; }

        public override SpectralData Data => (AnalysisResult as ThdAnalysisResult)?.Data;

        public new ThdMeasurementSettings Settings
        {
            get => (ThdMeasurementSettings)base.Settings;
        }

        public ThdMeasurement(ThdMeasurementSettings settings): base(settings)
        {
            /* TODO: Remove try/catch */
            try
            {
                Name = $"THD - {Settings.TestSignalOptions.InputOutputOptions}@{Settings.TestSignalOptions.Frequency}hz";
            }
            catch { }
        }

        public ThdMeasurement(ThdMeasurementSettings settings, ThdAnalysisResult result) : base(settings, result) { }
        

        protected override void Initialize()
        {
            RegisterActivity(CreateSetupActivity());

            TestSignalGenerator = new SineGenerator(AppSettings.Current.Device.SampleRate, Settings.TestSignalOptions.Frequency, Settings.TestSignalOptions.InputOutputOptions.OutputLevel.FromDbTp());
            if (!Settings.OverrideWarmUpSignalOptions)
            {
                WarmupSignalGenerator = TestSignalGenerator;
            }
            else
            {
                WarmupSignalGenerator = new SineGenerator(AppSettings.Current.Device.SampleRate, Settings.WarmUpSignalOptions.Frequency, Settings.TestSignalOptions.InputOutputOptions.OutputLevel.FromDbTp());
            }

            if (Settings.TestSignalOptions.InputOutputOptions.MatchInputLevel)
            {
                /* TODO: Add input level tune activity */
            }

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
            warmUpActivity.AddGenerator(AppSettings.Current.Device.PrimaryOutputChannel, WarmupSignalGenerator);

            var sink = new SpectralDataProcessor(Settings.Fft.Value.WindowSize, Settings.Fft.Value.WindowOverlapFactor, AppSettings.Current.Device.SampleRate / 2);
            sink.Data.DefaultValueSelector = (data) => data.LastValue;
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
            sink.Data.DefaultValueSelector = (data) => data.Mean;
            dataActivity.AddSink(AppSettings.Current.Device.PrimaryInputChannel, sink);

            if (Settings.StopConditions.Value.TimeoutEnabled)
            {
                dataActivity.RegisterStopCondition(new TimeoutStopCondition(Settings.StopConditions.Value.Timeout * 1000));
            }
            if (Settings.StopConditions.Value.ToleranceMatchingEnabled)
            {
                dataActivity.RegisterStopCondition(new ToleranceAchievedStopCondition(sink.Data, Settings.StopConditions.Value.Tolerance, Settings.StopConditions.Value.Confidence));
            }

            return dataActivity;
        }

        protected override IAnalysisResult Analyze()
        {
            var result = new ThdAnalysisResult();
            var data = ((SpectralDataProcessor)_dataActivity.GetSink(AppSettings.Current.Device.PrimaryInputChannel)).Data;
            result.Data = data;

            var first = data.AtFrequency(Settings.TestSignalOptions.Frequency, Settings.AnalysisOptions.WindowHalfSize);
            var evens = new List<SpectralData.StatisticsItem>();
            var odds = new List<SpectralData.StatisticsItem>();

            var harmonic = 2;
            var freq = 2 * Settings.TestSignalOptions.Frequency;

            while (freq < data.MaxFrequency)
            {
                var label = string.Empty;
                if (harmonic == 2) {
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

                var values = data.AtFrequency(freq, Settings.AnalysisOptions.WindowHalfSize).ToList();
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

                if (Settings.AnalysisOptions.LimitMaxFrequency)
                {
                    if (freq > Settings.AnalysisOptions.MaxFrequency)
                    {
                        break;
                    }
                }

                if (Settings.AnalysisOptions.LimitMaxHarmonics)
                {
                    if (harmonic - 1 > Settings.AnalysisOptions.MaxHarmonics)
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
            result.ThdFDb = 20.0 * Math.Log10(thdf);

            result.ThdRPercentage = 100.0 * thdr;
            result.ThdRDb = 20.0 * Math.Log10(thdr);

            result.EvenToOdd = evensSquareSum / oddsSquareSum;

            return result;
        }
    }
}
