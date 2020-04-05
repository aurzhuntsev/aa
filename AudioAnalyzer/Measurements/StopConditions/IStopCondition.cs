using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements.StopConditions
{    
    public interface IStopCondition
    {
        TimeSpan? Remaining { get; }
        
        void Set();
        bool Check();
    }
}
