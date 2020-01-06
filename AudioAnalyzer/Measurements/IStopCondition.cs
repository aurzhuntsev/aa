using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements
{
    public interface IStopCondition
    {
        TimeSpan Remaining { get; }
        bool Met { get; }
    }
}
