using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Common
{
    public static class DoubleExtensionscs
    {
        public static double ToDbTp(this double value)
        {
            var result = 20.0 * Math.Log10(1.0 / value);
            if (double.IsInfinity(result))
            {
                return 0.0;
            }

            return result;
        }

        public static double FromDbTp(this double value)
        {
            return Math.Pow(10.0, value / 20.0);
        }
    }
}
