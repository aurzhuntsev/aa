using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.AudioData
{
    public static class AudioDataAdapterProvider
    {
        private static Lazy<PortAudioDataAdapter> _adapter = new Lazy<PortAudioDataAdapter>(() =>
        {
            var result = new PortAudioDataAdapter();

            result.ValidateDeviceSettings();
            result.Initialize();

            return result;
        });

        public static IAudioDataAdapter Get()
        {
            return _adapter.Value;
        }
    }
}
