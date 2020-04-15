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
using AudioMark.Core.Fft;

namespace AudioMark.Core.Measurements
{
    [Measurement("Total Harmonic Distortion")]
    public class ThdMeasurement : SingleMeasurement
    {
        public new ThdMeasurementSettings Settings
        {
            get => (ThdMeasurementSettings)base.Settings;
        }

        public ThdMeasurement(ThdMeasurementSettings settings) : base(settings)
        {
            Name = $"{DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss")} - THD@{Settings.TestSignalOptions.Frequency}hz";
        }

        public ThdMeasurement(ThdMeasurementSettings settings, ThdAnalysisResult result) : base(settings, result) { }

        protected override IGenerator GetGenerator()
        {
            return new SineGenerator(AppSettings.Current.Device.SampleRate,
                Settings.TestSignalOptions.Frequency, 
                Settings.TestSignalOptions.InputOutputOptions.OutputLevel.FromDbTp());
        }

        protected override IAnalytics GetAnalytics()
        {
            return new ThdAnalytics();
        }

        protected override bool IsCorrectionApplicable(Spectrum data, int index)
        {
            return !data.GetFrequencyIndices(Settings.TestSignalOptions.Frequency, Settings.WindowHalfSize).Contains(index);
        }        
    }
}
