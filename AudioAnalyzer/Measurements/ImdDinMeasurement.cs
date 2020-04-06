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
    [Measurement("Intermodulation Distortion (DIN)")]
    public class ImdDinMeasurement : SingleMeasurement
    {
        public new ImdDinMeasurementSettings Settings
        {
            get => (ImdDinMeasurementSettings)base.Settings;
        }

        public ImdDinMeasurement(IMeasurementSettings settings) : base(settings)
        {
        }

        public ImdDinMeasurement(IMeasurementSettings settings, IAnalysisResult result) : base(settings, result)
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
                new PulseGenerator(AppSettings.Current.Device.SampleRate, Settings.F1Frequency, 0.5),
                new SineGenerator(AppSettings.Current.Device.SampleRate, Settings.F2Frequency, Settings.SignalsRate)
            );
        }
    }
}
