﻿using AudioMark.Core.Common;
using AudioMark.Core.Measurements.Analysis;
using AudioMark.Core.Measurements.Settings.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AudioMark.Core.Measurements.Common
{
    public interface IMeasurement
    {
        string Name { get; set; }
        bool Running { get; }

        TimeSpan? Remaining { get; }
        TimeSpan Elapsed { get; }

        IMeasurementSettings Settings { get; }

        int ActivitiesCount { get; }
        int CurrentActivityIndex { get; }
        string CurrentActivityDescription { get; }        
        
        Spectrum Result { get; }
        IAnalysisResult AnalysisResult { get; }        
        
        event EventHandler<Spectrum> OnDataUpdate;        
        event EventHandler<bool> OnComplete;
        event EventHandler<Exception> OnError;

        Task Run();
        void Stop();

        void Update();        
    }
}
