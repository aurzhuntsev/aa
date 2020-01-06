using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.AudioData
{
    public static class AudioDataAdapterProvider
    {
        public static IAudioDataAdapter Get()
        {
            var result = new PortAudioDataAdapter();
            
            result.ValidateDeviceSettings();
            result.Initialize();

            return result;
        }
    }
}
