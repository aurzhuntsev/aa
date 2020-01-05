using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements
{
    public class ActivityCompleteEventArgs
    {        
    }

    public class ActivityErrorEventArgs
    {
        public Exception Exception { get; internal set; }
    }

    public delegate void ActivityCompleteEventHandler(IActivity sender, ActivityCompleteEventArgs arg);
    public delegate void ActivityErrorEventHandler(IActivity sender, ActivityErrorEventArgs arg);

    public interface IActivity
    {
        IEnumerable<IStopCondition> StopConditions { get; }
        
        bool Running { get; }
        TimeSpan Elapsed { get; }
        TimeSpan Remaining { get; }

        string Name { get; }
        string Description { get; }

        event ActivityCompleteEventHandler OnComplete;
        event ActivityErrorEventHandler OnError;

        void Start();
        void Stop();        
    }
}
