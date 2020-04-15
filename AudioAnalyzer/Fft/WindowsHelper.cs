using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioMark.Core.Fft
{

    public static class WindowsHelper
    {
        public const int DefaultTaylorBars = 17;
        public const double DefaultTaylorSLL = -145;


        private static double Multiply(this IEnumerable<int> value, Func<double, double> selector)
        {
            var result = 1.0;
            foreach (var v in value)
            {
                result *= selector(v);
            }
            return result;
        }

        /* per https://prod-ng.sandia.gov/techlib-noauth/access-control.cgi/2017/174042.pdf */
        public static double[] Taylor(int n, int bars, double sideLobesLevel)
        {
            var nu = Math.Pow(10.0, -sideLobesLevel / 20.0);
            var a = Math.Log(nu + Math.Sqrt(Math.Pow(nu, 2.0) - 1)) / Math.PI;
            var s2 = Math.Pow(bars, 2.0) / (Math.Pow(a, 2.0) + Math.Pow(bars - 0.5, 2.0));

            var fm = new Func<double, double>((m) =>
            {
                var fNom = Enumerable.Range(1, bars - 1).Multiply(n => 1.0 - (m * m / s2) / (a * a + Math.Pow(n - 0.5, 2.0)));
                fNom = -(Math.Pow(-1.0, m) / 2.0) * fNom;

                var fDen = Enumerable.Range(1, bars - 1).Multiply(n => n == m ? 1 : 1.0 - m * m / (n * n));
                return fNom / fDen;
            });

            var result = new double[n];

            for (var i = 0; i < n; i++)
            {
                result[i] = 1.0 + 2.0 * Enumerable.Range(1, bars - 1).Select(m => fm(m) * Math.Cos(2.0 * Math.PI * m * ((double)i - n * 0.5 + 0.5) / n)).Sum();
            }

            return result;
        }

        public static double[] Hann(int n)
        {
            var result = new double[n];

            for (var i = 0; i < n; i++)
            {
                result[i] = 0.5 * (1.0 - Math.Cos(2.0 * Math.PI * i / (n - 1)));
            }

            return result;
        }

        public static double[] FlatTop(int n)
        {
            const double a0 = 0.21557895;
            const double a1 = 0.41663158;
            const double a2 = 0.277263158;
            const double a3 = 0.083578947;
            const double a4 = 0.006947368;

            var result = new double[n];
            for (var i = 0; i < n; i++)
            {
                var k = Math.PI * i / (n - 1);
                result[i] = a0 - a1 * Math.Cos(2.0 * k) + a2 * Math.Cos(4.0 * k) - a3 * Math.Cos(6.0 * k) + a4 * Math.Cos(8.0 * k);
            }

            return result;
        }

        public static double[] Rectangular(int n)
        {
            var result = new double[n];
            for (var i = 0; i < n; i++)
            {
                result[i] = 1.0;
            }

            return result;
        }
    }
}
