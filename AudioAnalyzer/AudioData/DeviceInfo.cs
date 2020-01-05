using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.AudioData
{
    public enum SampleFormat
    {
        Float32, Int24, Int16
    }

    public class DeviceInfo
    {
        public string Name { get; set; }
        public int Index { get; set; }
        public int ChannelsCount { get; set; }
        public SampleFormat SampleFormat { get; set; }
        public double SampleRate { get;  set; }
        public string ApiName { get; set; }
    }
}
