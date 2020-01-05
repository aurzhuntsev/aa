using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioAnalyzer.UI.SpectralView
{
    public class SpectralView
    {
        private const int E = 8;

        public int Width { get; set; }
        public int Height { get; set; }

        public IEnumerable<double> Data { get; set; }

        public int MaxFrequency
        {
            get
            {
                return Data.Count();
            }
        }

        public HorizontalMode HorizontalMode { get; set; }

        public unsafe void Render(IntPtr address)
        {
            for (var bx = 0; bx < Width; bx++)
            {
                for (var by = 0; by < Height; by++)
                {
                    SetPixel(address, bx, by, 0);
                }
            }

            DrawGrid(address);

            var x0 = 0;
            var val = 0.0;
            bool firstPoint = true;
            int px = 0;
            int py = 0;
            for (var x = 0; x < Width; x++)
            {
                var x1 = (int)Math.Pow(10.0, Math.Log10(MaxFrequency) / Width * x);
                if (x1 - x0 > 0)
                {
                    val = Data.Skip(x0).Take(x1 - x0).Max();
                }

                x0 = x1;
                var y = ToViewY(val == 0 ? Math.Pow(10.0, -E + 1) : val);
                if (firstPoint)
                {
                    SetPixel(address, x, y, 0xffffffff);
                    firstPoint = false;
                }
                else
                {
                    DrawLine(address, px, py, x, y, 0xffffffff);
                }

                px = x;
                py = y;
            }
        }

        private void DrawGrid(IntPtr address)
        {
            var step = 1;
            var current = 0.0;
            while ((current = Math.Pow(10.0, step)) < MaxFrequency)
            {
                var x = ToViewX(current);
                for (var y = 0; y < Height; y += 6)
                {
                    SetPixel(address, x, y, 0xff333333);
                    SetPixel(address, x, y + 1, 0xff333333);
                    SetPixel(address, x, y + 2, 0xff333333);
                    SetPixel(address, x, y + 3, 0xff333333);
                }

                for (var cx = current + Math.Pow(10.0, step); cx < Math.Pow(10.0, step + 1) && cx < MaxFrequency; cx += Math.Pow(10.0, step))
                {
                    var lx = ToViewX(cx);
                    for (var y = 0; y < Height; y += 4)
                    {
                        SetPixel(address, lx, y, 0xff111111);
                        SetPixel(address, lx, y + 1, 0xff333333);
                    }
                }
                step++;
            }

            step = 1;
            current = 1.0;

            for (current = 1; current < E; current++)
            {
                var y = ToViewY(Math.Pow(10.0, -current));
                for (var x = 0; x < Width; x++)
                {
                    SetPixel(address, x, (int)y, 0xff111111);
                }
            }
        }

        private unsafe void SetPixel(IntPtr address, int x, int y, uint color)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                x = 0;
                y = 0;
             //   throw new Exception("Pixel coordinates are our of view bounds.");
            }

            *((uint*)address + (uint)(y * Width + x)) = color;
        }

        private void DrawLine(IntPtr address, int x0, int y0, int x1, int y1, uint color)
        {
            float x, y, dx, dy, step;
            int i;
            dx = (float)(x1 - x0);
            dy = (float)(y1 - y0);

            if (Math.Abs(dx) >= Math.Abs(dy))
            {
                step = Math.Abs(dx);
            }
            else
            {
                step = Math.Abs(dy);
            }

            dx = dx / step;
            dy = dy / step;
            x = x0;
            y = y0;
            i = 1;

            while (i <= step)
            {
                SetPixel(address, (int)x, (int)y, color);
                x = x + dx;
                y = y + dy;
                i = i + 1;             
            }
        }

        private int ToViewX(double value)
        {
            if (HorizontalMode == HorizontalMode.Logarithimc)
            {
                return (int)((Width / Math.Log10(MaxFrequency)) * Math.Log10(value));
            }
            else if (HorizontalMode == HorizontalMode.Linear)
            {
                return (int)((Width / MaxFrequency) * value);
            }

            throw new Exception($"{HorizontalMode} horizontal scale mode is not supported.");
        }

        private int ToViewY(double value)
        {
            var k = (Height / Math.Log10(Math.Pow(10.0, -E)));
            return (int)(-k * Math.Log10(1.0 / value));
        }
    }
}
