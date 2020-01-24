using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements
{
    [Serializable]
    internal class MeasurementSerializationContainer
    {
        public string TypeName { get; set; }
        public string Name { get; set; }

        public IMeasurementSettings Settings { get; set; }
        public IAnalysisResult Result { get; set; }
    }
}
