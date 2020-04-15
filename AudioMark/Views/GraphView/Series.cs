using AudioMark.Core.Common;
using AudioMark.Core.Fft;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Views.GraphView
{
    public class Series
    {
        public Spectrum Data { get; set; }
        public int ColorIndex { get; set; }
        public bool Visible { get; set; }
    }
}
