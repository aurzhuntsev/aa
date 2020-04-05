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
    public class NoiseMeasurement : SingleMeasurement
    {    
        public new NoiseMeasurementSettings Settings
        {
            get => (NoiseMeasurementSettings)base.Settings;
        }

        public NoiseMeasurement(IMeasurementSettings settings) : base(settings)
        {
            Name = $"{DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss")} - Noise";
        }

        public NoiseMeasurement(IMeasurementSettings settings, IAnalysisResult result) : base(settings, result)
        {
        }
        
        protected override IGenerator GetGenerator()
        {
            if (Settings.GenerateDummySignal)
            {
                return new SineGenerator(AppSettings.Current.Device.SampleRate, 
                    Settings.DummySignalOptions.Frequency, 
                    Settings.DummySignalOptions.InputOutputOptions.OutputLevel.FromDbTp());
            }

            return new SilenceGenerator();
        }

        protected override IAnalytics GetAnalytics()
        {
            return new NoiseAnalytics();
        }
    }
}
