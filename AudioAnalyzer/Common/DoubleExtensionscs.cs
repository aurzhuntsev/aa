using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Common
{
    public static class DoubleExtensionscs
    {
        public static double ToDbTp(this double value)
        {
            return 20.0 * Math.Log10(1.0 / value);
        }

        public static double FromDbTp(this double value)
        {
            return Math.Pow(10.0, value / 20.0);
        }
    }
}
