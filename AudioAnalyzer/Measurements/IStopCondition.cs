using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements
{    
    public interface IStopCondition
    {
        TimeSpan? Remaining { get; }
        event EventHandler OnMet;
        event EventHandler<Exception> OnError;

        void Set();
        void Check();
    }
}
