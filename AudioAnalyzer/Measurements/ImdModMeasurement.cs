using AudioMark.Core.Common;
using AudioMark.Core.Generators;
using AudioMark.Core.Measurements.Analysis;
using AudioMark.Core.Measurements.Common;
using AudioMark.Core.Measurements.Settings;
using AudioMark.Core.Measurements.Settings.Common;
using AudioMark.Core.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements
{
    [Measurement("Intermodulation Distortion (MOD)")]
    public class ImdModMeasurement : SingleMeasurement
    {
        public new ImdModMeasurementSettings Settings
        {
            get => (ImdModMeasurementSettings)base.Settings;
        }

        public ImdModMeasurement(IMeasurementSettings settings) : base(settings)
        {
        }

        public ImdModMeasurement(IMeasurementSettings settings, IAnalysisResult result) : base(settings, result)
        {
        }

        protected override IAnalytics GetAnalytics()
        {
            return new ImdAnalytics();
        }

        protected override IGenerator GetGenerator()
        {
            return new CompositeGenerator(
                AppSettings.Current.Device.SampleRate,
                Settings.TestSignalOptions.InputOutputOptions.OutputLevel.FromDbTp(),
                new SineGenerator(AppSettings.Current.Device.SampleRate, Settings.TestSignalOptions.Frequency),
                new SineGenerator(AppSettings.Current.Device.SampleRate, Settings.SecondarySignalFrequency, Settings.SignalsRate)
            );
        }
    }
}
