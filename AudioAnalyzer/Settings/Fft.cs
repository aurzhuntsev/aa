using System;

namespace AudioMark.Core.Settings
{
    [Serializable]
    public class Fft
    {
        public int WindowSize { get; set; }
        public double WindowOverlapFactor { get; set; }
    }
}