using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using AudioMark.Core.Common;

namespace AudioMark.Core.Measurements
{
    public class ActivityCompleteEventArgs
    {
    }

    public class ActivityErrorEventArgs
    {
        public Exception Exception { get; internal set; }
    }

    public delegate void ActivityCompleteEventHandler(ActivityBase sender, ActivityCompleteEventArgs arg);
    public delegate void ActivityErrorEventHandler(ActivityBase sender, ActivityErrorEventArgs arg);
    
    public abstract class ActivityBase
    {
        private List<IStopCondition> _stopConditions = new List<IStopCondition>();
        IEnumerable<IStopCondition> StopConditions => _stopConditions;

        public bool Running { get; protected set; }
        private EventWaitHandle _waitHandle { get; } = new EventWaitHandle(true, EventResetMode.ManualReset);

        private DateTime _startedAt;
        public TimeSpan Elapsed
        {
            get => DateTime.Now.Subtract(_startedAt).Duration();
        }

        public TimeSpan? Remaining
        {

            get
            {
                if (StopConditions.Any(stopCondition => !stopCondition.Remaining.HasValue))
                {
                    return null;
                }
                return StopConditions.Min(stopCondition => stopCondition.Remaining.Value);
            }
        }

        public string Description { get; }

        public event ActivityCompleteEventHandler OnComplete;
        public event ActivityErrorEventHandler OnError;        

        public ActivityBase(string description)
        {
            Description = description;
            _stopConditions = new List<IStopCondition>();
        }

        public virtual void Start()
        {
            _startedAt = DateTime.Now;

            foreach (var stopCondition in StopConditions)
            {
                stopCondition.Set();
            }

            Running = true;
            _waitHandle.Reset();
        }

        public virtual void Stop()
        {
            Running = false;
            _waitHandle.Set();

            OnComplete?.Invoke(this, new ActivityCompleteEventArgs());
        }

        public void AddStopCondition(IStopCondition stopCondition)
        {
            stopCondition.OnMet += (s) =>
            {
                Stop();
            };

            _stopConditions.Add(stopCondition);
        }

        public void CheckStopConditions()
        {
            foreach (var stopCondition in StopConditions)
            {
                stopCondition.Check();
            }
        }

        public void WaitToComplete()
        {
            _waitHandle.WaitOne();
        }
    }
}
