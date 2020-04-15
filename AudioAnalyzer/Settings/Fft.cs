using AudioMark.Core.Common;
using System;

namespace AudioMark.Core.Settings
{
    public enum WindowFunctions
    {
        [StringAttribute("Taylor (-50dB, 7 bars)")]
        Taylor ,

        [StringAttribute("None (Rectangular)")]
        None,
        
        [StringAttribute("Hann")]
        Hann,

        [StringAttribute("Flat top")]
        FlatTop
    }

    [Serializable]
    public class Fft
    {
        public WindowFunctions WindowFunction { get; set; }
        public int WindowSize { get; set; }
        public double WindowOverlapFactor { get; set; }
    }
}