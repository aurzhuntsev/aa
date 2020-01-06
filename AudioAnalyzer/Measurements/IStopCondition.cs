using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements
{
    public delegate void StopConditionMetEventHandler(IStopCondition sender);

    public interface IStopCondition
    {
        TimeSpan Remaining { get; }
        event StopConditionMetEventHandler OnMet;

        void Set();
        void Check();
    }
}
