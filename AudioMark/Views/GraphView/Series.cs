using AudioMark.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Views.GraphView
{
    public class Series
    {
        public SpectralData Data { get; set; }
        public int ColorIndex { get; set; }
        public bool Visible { get; set; }
    }
}
