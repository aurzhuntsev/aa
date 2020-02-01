using AudioMark.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements.Common
{
    public class MeasurementAttribute: StringAttribute
    {
        public MeasurementAttribute(string value) : base(value) { }
    }
}
