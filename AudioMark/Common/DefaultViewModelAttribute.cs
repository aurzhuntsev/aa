using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Common
{
    public class DefaultViewModelAttribute : Attribute
    {
        public Type ModelType { get; set; }
        public DefaultViewModelAttribute(Type modelType) => ModelType = modelType;
    }
}
