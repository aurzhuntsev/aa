using System;
using System.Collections.Generic;
using System.Text;
using static AudioMark.Core.Settings.Device;

namespace AudioMark.Core.AudioData
{
    public interface IAudioDataAdapter
    {
        bool Running { get; }

        delegate void DataReadEventHandler(IAudioDataAdapter sender, double[] data, int length, bool discard);
        delegate int DataWriteEventHandler(IAudioDataAdapter sender, double[] data, bool discard);
        DataReadEventHandler OnRead { get; set; }
        DataWriteEventHandler OnWrite { get; set; }

        IEnumerable<DeviceInfo> EnumerateInputDevices();
        IEnumerable<DeviceInfo> EnumerateOutputDevices();

        DeviceInfo GetDefaultInputDevice();
        DeviceInfo GetDefaultOutputDevice();

        void Initialize();
        void FillOutputBuffer();

        void ValidateDeviceSettings();

        void Start();
        void Stop();
    }
}
