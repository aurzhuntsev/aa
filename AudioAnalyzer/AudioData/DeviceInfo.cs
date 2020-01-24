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
        public string ApiName { get; set; }
        public string Name { get; set; }        
        public SampleFormat SampleFormat { get; set; }        
        public int LatencyMilliseconds { get; set; }
        public int SampleRate { get; set; }

        public int Index { get; set; }
        public int ChannelsCount { get; set; }
    }
}
