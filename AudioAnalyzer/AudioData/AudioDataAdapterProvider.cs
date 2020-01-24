using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.AudioData
{
    public static class AudioDataAdapterProvider
    {
        private static PortAudioDataAdapter _adapter;
        public static void Initialize()
        {
            _adapter = new PortAudioDataAdapter();

            _adapter.ValidateDeviceSettings();
            _adapter.Initialize();           
        }

        public static IAudioDataAdapter Get()
        {
            if (_adapter == null)
            {
                throw new Exception("Adapter not initalized.");
            }

            return _adapter;
        }
    }
}
