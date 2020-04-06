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
    [Measurement("Intermodulation Distortion (DFD)")]
    public class ImdDfdMeasurement : SingleMeasurement
    {
        public new ImdDfdMeasurementSettings Settings
        {
            get => (ImdDfdMeasurementSettings)base.Settings;
        }

        public ImdDfdMeasurement(IMeasurementSettings settings) : base(settings)
        {
        }

        public ImdDfdMeasurement(IMeasurementSettings settings, IAnalysisResult result) : base(settings, result)
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
                new SineGenerator(AppSettings.Current.Device.SampleRate, Settings.F1Frequency),
                new SineGenerator(AppSettings.Current.Device.SampleRate, Settings.F2Frequency, Settings.SignalsRate)
            );
        }
    }
}
