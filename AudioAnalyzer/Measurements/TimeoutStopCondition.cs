using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements
{
    public class TimeoutStopCondition : IStopCondition
    {
        private int _timeout = 0;

        private DateTime _startedAt;
        public TimeSpan? Remaining
        {
            get
            {
                var remaining = _timeout - (int)DateTime.Now.Subtract(_startedAt).Duration().TotalMilliseconds;
                if (remaining <= 0)
                {
                    return new TimeSpan(0);
                }

                return new TimeSpan(0, 0, 0, 0, remaining);
            }
        }
        public event StopConditionMetEventHandler OnMet;

        public TimeoutStopCondition(int timeoutMilliseconds)
        {
            _timeout = timeoutMilliseconds;
        }

        public void Check()
        {
            if (Remaining.HasValue && Remaining.Value.TotalMilliseconds == 0)
            {
                OnMet?.Invoke(this);
            }
        }

        public void Set()
        {
            _startedAt = DateTime.Now;
        }
    }
}
