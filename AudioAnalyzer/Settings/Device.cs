using AudioMark.Core.AudioData;
using PortAudioWrapper;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace AudioMark.Core.Settings
{    
    public class Device
    {  
        public double[] DefaultSampleRates { get; set; }

        public DeviceInfo InputDevice { get; set; }
        public DeviceInfo OutputDevice { get; set; }

        public string Api { get;set; }        
        public int SampleRate { get; set; }
        public int BufferSize { get; set; }
    
        public int PrimaryInputChannel { get; set; }
        public int PrimaryOutputChannel { get; set; }

        public int SecondaryInputChannel { get; set; }
        public int SecondaryOutputChannel { get; set; }
        public double ClippingLevel { get; set; }
    }
}
