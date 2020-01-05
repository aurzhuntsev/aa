using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Common
{
    public class StringAttribute : Attribute
    {
        public string Value { get; private set; }

        public StringAttribute()
        {
        }

        public StringAttribute(string value)
        {
            Value = value;
        }
    }
 }
