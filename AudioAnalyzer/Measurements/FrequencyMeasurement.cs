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
    public class FrequencyMeasurement : SingleMeasurement
    {
        public new FrequencyMeasurementSettings Settings => (FrequencyMeasurementSettings)base.Settings;

        public FrequencyMeasurement(IMeasurementSettings settings) : base(settings)
        {
        }

        public FrequencyMeasurement(IMeasurementSettings settings, IAnalysisResult result) : base(settings, result)
        {
        }

        protected override IAnalytics GetAnalytics()
        {
            return new FrequencyAnalytics();
        }

        protected override IGenerator GetGenerator()
        {
            return new SineGenerator(AppSettings.Current.Device.SampleRate,
             Settings.TestSignalOptions.Frequency,
             Settings.TestSignalOptions.InputOutputOptions.OutputLevel.FromDbTp());
        }
    }
}
