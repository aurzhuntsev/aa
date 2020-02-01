using AudioMark.Core.Common;
using AudioMark.Core.Generators;
using AudioMark.Core.Measurements.Analysis;
using AudioMark.Core.Measurements.Common;
using AudioMark.Core.Measurements.Settings;
using AudioMark.Core.Measurements.Settings.Common;
using AudioMark.Core.Measurements.StopConditions;
using AudioMark.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioMark.Core.Measurements
{
    [Measurement("Noise")]
    public class NoiseMeasurement : MeasurementBase<SpectralData>
    {
        private Activity<SpectralData> _dataActivity;

        public new NoiseMeasurementSettings Settings
        {
            get => (NoiseMeasurementSettings)base.Settings;
        }

        public override SpectralData Result => AnalysisResult?.Data;

        public NoiseMeasurement(IMeasurementSettings settings) : base(settings)
        {
            Name = $"{DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss")} - Noise";
        }

        public NoiseMeasurement(IMeasurementSettings settings, IAnalysisResult result) : base(settings, result)
        {
        }
        
        /* TODO: Refactor to reduce copypaste */
        public override void UpdateAnalysisResult()
        {
            var result = new NoiseAnalysisResult();

            var data = Result == null ? ((SpectralDataProcessor)_dataActivity.GetSink(AppSettings.Current.Device.PrimaryInputChannel)).Data : Result;
            SetCorrectionProfile(data);

            result.Data = data;

            var right = Settings.LimitHighFrequency ? result.Data.GetFrequencyIndices(Settings.HighFrequency, 0).First() : result.Data.Size - 1;
            var sum = Enumerable.Range(0, right).Sum(s => result.Data.Statistics[s].Mean * result.Data.Statistics[s].Mean);
            var avg = Enumerable.Range(0, right).Average(s => result.Data.Statistics[s].Mean);

            result.NoisePowerDbFs = -20.0 * Math.Log10(1.0 / Math.Sqrt(sum));
            result.AverageLevelDbTp = -20.0 * Math.Log10(1.0 / avg);

            AnalysisResult = result;
        }

        protected override void Initialize()
        {
            RegisterActivity(CreateSetupActivity());
            
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

        private Activity<SpectralData> CreateAcquisitionActivity()
        {
            var activity = new Activity<SpectralData>("Gathering noise...");
            if (Settings.GenerateDummySignal)
            {
                var generator = new SineGenerator(AppSettings.Current.Device.SampleRate, Settings.DummySignalOptions.Frequency, Settings.DummySignalOptions.InputOutputOptions.OutputLevel);
                activity.AddGenerator(AppSettings.Current.Device.PrimaryOutputChannel, generator);
            }
            else
            {
                activity.AddGenerator(AppSettings.Current.Device.PrimaryOutputChannel, new SilenceGenerator());
            }

            var sink = new SpectralDataProcessor(Settings.Fft.Value.WindowSize, Settings.Fft.Value.WindowOverlapFactor, AppSettings.Current.Device.SampleRate / 2);
            sink.Data.DefaultValue = SpectralData.DefaultValueType.Mean;
            SetCorrectionProfile(sink.Data);

            activity.AddSink(AppSettings.Current.Device.PrimaryInputChannel, sink);

            Settings.ApplyStopConditions(activity, sink.Data);

            return activity;
        }

        private void SetCorrectionProfile(SpectralData target)
        {
            if (Settings.ApplyCorrectionProfile && Settings.CorrectionProfile != null)
            {
                target.SetCorrectionProfile(Settings.CorrectionProfile, (data, index) => true);
            }
            else
            {
                target.SetCorrectionProfile(null, null);
            }
        }

    }
}
