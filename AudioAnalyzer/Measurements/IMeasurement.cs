using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements
{
    public interface IMeasurement
    {
        IEnumerable<ActivityBase> Activities { get; }
        ActivityBase CurrentActivity { get; }

        string Title { get; }

        void Run();
    }
}
