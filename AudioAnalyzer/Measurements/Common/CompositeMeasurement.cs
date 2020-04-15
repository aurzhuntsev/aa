using AudioMark.Core.Common;
using AudioMark.Core.Fft;
using AudioMark.Core.Measurements.Analysis;
using AudioMark.Core.Measurements.Settings.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioMark.Core.Measurements.Common
{
    public abstract class CompositeMeasurement : MeasurementBase
    {
        public override TimeSpan? Remaining => null;

        private SingleMeasurement[] Measurements { get; set; }
        private int ActiveMeasurementIndex { get; set; }

        public CompositeMeasurement(IMeasurementSettings settings) : base(settings)
        {
        }

        internal CompositeMeasurement(IMeasurementSettings settings, IAnalysisResult result) : base(settings, result)
        {
        }

        public override void Update()
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

            RunNextMeasurement();
        }

        private void RunNextMeasurement()
        {            
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
            OnMeasurementComplete(Measurements[ActiveMeasurementIndex]);

            ActiveMeasurementIndex++;
            if (ActiveMeasurementIndex < Measurements.Length - 1)
            {
                RunNextMeasurement();
            }
            else
            {
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
        protected abstract void OnMeasurementComplete(SingleMeasurement measurement);
    }
}
