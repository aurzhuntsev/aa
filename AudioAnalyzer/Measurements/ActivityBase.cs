using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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
        IEnumerable<IStopCondition> StopConditions { get; }
        
        public bool Running { get; protected set; }

        private DateTime _startedAt;
        public TimeSpan Elapsed
        {
            get => DateTime.Now.Subtract(_startedAt).Duration();
        }

        public TimeSpan Remaining
        {
            get => StopConditions.Min(stopCondition => stopCondition.Remaining);
        }
        
        public string Description { get; }

        event ActivityCompleteEventHandler OnComplete;
        event ActivityErrorEventHandler OnError;

        public ActivityBase(string description)
        {
            Description = description;
        }

        public virtual void Start()
        {
            _startedAt = DateTime.Now;
            Running = true;
        }

        public virtual void Stop()
        {
            Running = false;
            OnComplete(this, new ActivityCompleteEventArgs());
        }
    }
}
