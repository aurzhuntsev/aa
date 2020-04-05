using AudioMark.Core.Common;
using AudioMark.Core.Generators;
using AudioMark.Core.Measurements.Analysis;
using AudioMark.Core.Measurements.Settings.Common;
using AudioMark.Core.Measurements.StopConditions;
using AudioMark.Core.Settings;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AudioMark.Core.Measurements.Common
{
    public abstract class SingleMeasurement : MeasurementBase
    {
        private SpectrumProcessor _processor = null;
        public override Spectrum Result => _processor?.Data;

        public SingleMeasurement(IMeasurementSettings settings) : base(settings)
        {
            Initialize();
        }

        internal SingleMeasurement(IMeasurementSettings settings, IAnalysisResult result) : base(settings, result)
        {
            Initialize();
        }

        protected abstract IGenerator GetGenerator();
        protected abstract IAnalytics GetAnalytics();

        private void Initialize()
        {
            RegisterGenerator(AppSettings.Current.Device.PrimaryOutputChannel, GetGenerator());

            _processor = new SpectrumProcessor(AppSettings.Current.Fft.WindowSize,
                                               AppSettings.Current.Fft.WindowOverlapFactor,
                                               (int)(AppSettings.Current.Device.SampleRate * 0.5));

            RegisterSink(AppSettings.Current.Device.PrimaryInputChannel, _processor);

            ApplyStopConditions();
            ApplyCorrectionProfile(_processor.Data);
        }

        protected override void RunInternal()
        {
            ActivitiesCount = 2; /* Warmuy and gathering */

            EnableStopConditions = false;            
            CurrentActivityDescription = "Warming up...";

            _processor.Data.DefaultValue = Spectrum.DefaultValueType.Last;


            var warmUpDurationSeconds = 0;
            if (Settings is IWarmable warmable)
            {
                warmUpDurationSeconds = warmable.WarmUpEnabled ? warmable.WarmUpDurationSeconds : 0;
            }

            Task.Run(async () =>
            {
                await Task.Delay(warmUpDurationSeconds * 1000);

                if (Running)
                {
                    EnableStopConditions = true;
                    SetStopConditions();

                    CurrentActivityDescription = "Gathering data...";
                    CurrentActivityIndex++;

                    _processor.Reset();
                    _processor.Data.DefaultValue = Spectrum.DefaultValueType.Mean;
                }
            });
        }

        public override void Update()
        {
            ApplyCorrectionProfile(Result);

            var analytics = GetAnalytics();
            AnalysisResult = analytics.Analyze(_processor.Data, Settings);
        }

        protected void ApplyStopConditions()
        {
            if (Settings is IGlobalOptions source)
            {
                if (source.StopConditions.Value.TimeoutEnabled)
                {
                    RegisterStopCondition(new TimeoutStopCondition(source.StopConditions.Value.Timeout * 1000));
                }
                if (source.StopConditions.Value.ToleranceMatchingEnabled)
                {
                    RegisterStopCondition(new ToleranceAchievedStopCondition(_processor.Data, source.StopConditions.Value.Tolerance, source.StopConditions.Value.Confidence));
                }
            }
        }

        protected void ApplyCorrectionProfile(Spectrum target)
        {
            if (Settings is ICorrectionProfile correctionProfile)
            {
                if (correctionProfile.ApplyCorrectionProfile && correctionProfile.CorrectionProfile != null)
                {
                    target.SetCorrectionProfile(correctionProfile.CorrectionProfile, IsCorrectionApplicable);
                }
                else
                {
                    target.SetCorrectionProfile(null, null);
                }
            }
        }

        protected virtual bool IsCorrectionApplicable(Spectrum data, int index)
        {
            return true;
        }
    }
}
