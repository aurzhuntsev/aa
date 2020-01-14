using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements
{
    public interface IDataSink<TResult>
    {
        void Add(double value);
        event EventHandler<TResult> OnItemProcessed;
    }
}
