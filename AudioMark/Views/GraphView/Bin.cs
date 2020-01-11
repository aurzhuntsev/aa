using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Views.GraphView
{
    public class Bin
    {
        public double Value;
        public double SpectrumBin { get; set; }

        public int Left { get; set; }
        public int Right { get; set; }
        public int From { get; set; }
        public int To { get; set; }

        public bool HasCursor { get; set; }
    }
}
