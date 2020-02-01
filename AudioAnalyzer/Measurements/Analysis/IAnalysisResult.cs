﻿using AudioMark.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements.Analysis
{
    public interface IAnalysisResult
    {
        SpectralData Data { get; }
    }
}