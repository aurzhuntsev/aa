using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Common
{
    public class MathNetFft : IFft
    {
        public void ForwardReal(double[] values)
        {
            var n = values.Length % 2 == 0 ? values.Length - 2 : values.Length - 1;
            MathNet.Numerics.IntegralTransforms.Fourier.ForwardReal(values, n, MathNet.Numerics.IntegralTransforms.FourierOptions.NoScaling);

            for (var i = 0; i < n; i++)
            {
                values[i] /= (double)n;
            }
        }
    }
}
