using AudioMark.Core.Measurements.Analysis;
using AudioMark.Core.Measurements.Common;
using AudioMark.Core.Measurements.Settings;
using AudioMark.Core.Measurements.Settings.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioMark.Core.Measurements
{
    [Measurement("Frequency Response")]
    public class FrequencyResponseMeasurement : CompositeMeasurement
    {
        public new FrequencyResponseMeasurementSettings Settings
        {
            get => (FrequencyResponseMeasurementSettings)base.Settings;
        }

        public FrequencyResponseMeasurement(IMeasurementSettings settings) : base(settings)
        {
        }

        public FrequencyResponseMeasurement(IMeasurementSettings settings, IAnalysisResult result) : base(settings, result)
        {
        }

        protected override IEnumerable<SingleMeasurement> GetMeasurements()
        {
            var frequencies = Settings.GetFrequencies().ToList();

            foreach (var freq in frequencies)
            {
                var measurement = new FrequencyMeasurement(
                    new FrequencyMeasurementSettings()
                    {
                        TestSignalOptions = new SignalSettings()
                        {
                            Frequency = freq,
                            InputOutputOptions = new InputOutputLevel()
                            {
                                OutputLevel = Settings.TestSignalOptions.InputOutputOptions.OutputLevel
                            }
                        }
                    });
                yield return measurement;
            }
        }

        protected override void OnMeasurementComplete(SingleMeasurement measurement)
        {            
        }
    }
}
