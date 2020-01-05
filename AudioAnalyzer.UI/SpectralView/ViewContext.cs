using System;
using System.Collections.Generic;
using System.Text;

namespace AudioAnalyzer.UI.SpectralView
{

    public class ViewContext
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public HorizontalMode HorizontalScaleMode { get; set; } /* TODO: Better naming */
    }
}
