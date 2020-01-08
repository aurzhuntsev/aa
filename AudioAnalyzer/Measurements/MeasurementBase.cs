using AudioMark.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AudioMark.Core.Measurements
{
    public delegate void MeasurementCompleteEventHandler(MeasurementBase sender);
    public delegate void MeasurementErrorEventHandler(MeasurementBase sender, Exception error);
    public delegate void MeasurementDataUpdateEventHandler(MeasurementBase sender, object data);

    public abstract class MeasurementBase
    {
        private List<ActivityBase> _activities = new List<ActivityBase>();
        public List<ActivityBase> Activities => _activities;

        public ActivityBase CurrentActivity { get; protected set; }

        public string Title { get; protected set; }

        public event MeasurementCompleteEventHandler OnComplete;
        public event MeasurementErrorEventHandler OnError;
        public event MeasurementDataUpdateEventHandler OnDataUpdate;

        protected abstract void Initialize();

        public async Task Run()
        {
            Initialize();

            await Task.Run(() =>
            {
                foreach (var activity in Activities)
                {
                    CurrentActivity = activity;

                    CurrentActivity.OnComplete += (a, e) =>
                    {

                    };

                    CurrentActivity.OnError += (a, e) =>
                    {

                    };
                    
                    CurrentActivity.Start();
                    CurrentActivity.WaitToComplete();
                }

                OnComplete?.Invoke(this);
            });
        }

        public void Stop()
        {
            if (CurrentActivity != null)
            {
                CurrentActivity.Stop();
            }
        }

        protected void InvokeDataUpdate(object data)
        {
            OnDataUpdate.Invoke(this, data);
        }
    }
}
