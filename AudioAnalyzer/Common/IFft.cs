using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Common
{
    public interface IFft
    {
        void ForwardReal(double[] values);
    }
}
