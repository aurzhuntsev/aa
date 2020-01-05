using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements
{
    public interface IMeasurement
    {
        IEnumerable<IActivity> Activities { get; }
        IActivity CurrentActivity { get; }


    }
}
