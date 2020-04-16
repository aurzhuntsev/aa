using AudioMark.Core.Common;
using AudioMark.Core.Fft;
using AudioMark.Core.Measurements.Analysis;
using AudioMark.Core.Measurements.Settings.Common;
using AudioMark.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioMark.Core.Measurements.Common
{
    public abstract class CompositeMeasurement : MeasurementBase
    {
        public override TimeSpan? Remaining => null;

        protected SingleMeasurement[] Measurements { get; set; }
        private int ActiveMeasurementIndex { get; set; }

        private double _previousCompletedFrequency;

        public CompositeMeasurement(IMeasurementSettings settings) : base(settings)
        {
        }

        internal CompositeMeasurement(IMeasurementSettings settings, IAnalysisResult result) : base(settings, result)
        {
        }

        protected override bool CheckSignalPresent(Spectrum data)
        {
            return true;
        }

        protected override void RunInternal()
        {
            Measurements = GetMeasurements().ToArray();
            ActivitiesCount = Measurements.Count();
            ActiveMeasurementIndex = 0;

            Result = new Spectrum(Settings.Fft.Value.WindowSize, AppSettings.Current.Device.SampleRate / 2);
            Result.DefaultValue = Spectrum.DefaultValueType.Mean;

            RunNextMeasurement();
        }

        private void RunNextMeasurement()
        {            
            if (!_running)
            {
                return;
            }

            /* TODO: monitor */
            var measurement = Measurements[ActiveMeasurementIndex];
            measurement.DataUpdate += OnActiveDataUpdate;
            measurement.Complete += OnActiveComplete;
            measurement.Error += OnActiveError;
            _ = measurement.Run();
        }

        private void OnActiveError(object sender, Exception e)
        {
            OnError(e);
        }

        private void OnActiveComplete(object sender, bool e)
        {
            ActiveMeasurementIndex++;
            if (ActiveMeasurementIndex < Measurements.Length - 1)
            {
                RunNextMeasurement();
            }
            else
            {
                Update();
                OnDataUpdate(Result);
                OnComplete(true);
            }
        }

        private void OnActiveDataUpdate(object sender, Spectrum e)
        {
            OnDataUpdate(e);
        }

        protected override void StopInternal(bool interrupted)
        {
            var activeMeasurement = Measurements[ActiveMeasurementIndex];
            activeMeasurement.Stop();
        }

        protected abstract IEnumerable<SingleMeasurement> GetMeasurements();        
    }
}
