using AudioMark.Core.Common;
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
                var measurement = new FrequencyMeasurement(GetFrequencyMeasurementSettings(freq));
                yield return measurement;
            }
        }

        private FrequencyMeasurementSettings GetFrequencyMeasurementSettings(double frequency)
        {
            return new FrequencyMeasurementSettings()
            {
                TestSignalOptions = new SignalSettings()
                {
                    Frequency = frequency,
                    InputOutputOptions = new InputOutputLevel()
                    {
                        OutputLevel = Settings.TestSignalOptions.InputOutputOptions.OutputLevel
                    }
                },
                WindowHalfSize = Settings.WindowHalfSize
            };
        }

        public override void Update()
        {
            var previousFrequency = 0.0;
            for (var i = 0; i < Measurements.Length; i++)
            {
                var measurement = Measurements[i];
                var measurementSettings = measurement.Settings as FrequencyMeasurementSettings;          

                measurementSettings.WindowHalfSize = Settings.WindowHalfSize;

                measurement.Update();
                var measurementResult = measurement.AnalysisResult as FrequencyAnalysisResult;

                Result.SetAtFrequency(measurementSettings.TestSignalOptions.Frequency,
                                      (-measurementResult.FrequencyValueDb).FromDbTp(),
                                      x => x.Mean);
                if (i > 0)
                {
                    Result.Interpolate(previousFrequency, measurementSettings.TestSignalOptions.Frequency, x => x.Mean);
                }

                previousFrequency = measurementSettings.TestSignalOptions.Frequency;
            }

            AnalysisResult = (new FrequencyResponseAnalytics()).Analyze(Result, Settings);

            OnDataUpdate(Result);
        }
    }
}
