using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AudioMark.Core.Measurements
{
    public interface IMeasurement
    {
        string Name { get; set; }

        int CurrentActivityIndex { get; }
        string CurrentActivityDescription { get; }
        int ActivitiesCount { get; }
        TimeSpan? Remaining { get; }
        TimeSpan Elapsed { get; }
        bool Running { get; }

        IAnalysisResult AnalysisResult { get; }
        IMeasurementSettings Settings { get; }

        /* TODO: Should not use object here */
        event EventHandler<object> OnDataUpdate;
        event EventHandler<IAnalysisResult> OnAnalysisComplete;
        event EventHandler<bool> OnComplete;
        event EventHandler<Exception> OnError;

        Task Run();
        void Stop();
    }

    public interface IMeasurement<TResult> : IMeasurement
    {
        TResult Data { get; }
    }
}
