using System;
using System.Collections.Generic;
using System.Text;
using static AudioMark.Core.Settings.Device;

namespace AudioMark.Core.AudioData
{
    public interface IAudioDataAdapter
    {
        bool Running { get; }

        delegate void DataReadEventHandler(IAudioDataAdapter sender, double[] data, int length);
        delegate int DataWriteEventHandler(IAudioDataAdapter sender, double[] data);
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
