using System;
using System.Collections.Generic;
using System.Text;
using static AudioMark.Core.Settings.Device;

namespace AudioMark.Core.AudioData
{
    public delegate void DataReadEventHandler(IAudioDataAdapter sender, double[] data, int length, bool discard);
    public delegate int DataWriteEventHandler(IAudioDataAdapter sender, double[] data, bool discard);

    public interface IAudioDataAdapter
    {
        bool Running { get; }
               
        DataReadEventHandler OnRead { get; }
        DataWriteEventHandler OnWrite { get; }
        EventHandler<Exception> OnError { get; }

        void SetReadHandler(DataReadEventHandler readHandler);
        void SetWriteHandler(DataWriteEventHandler writeHandler);
        void SetErrorHandler(EventHandler<Exception> errorHandler);

        IEnumerable<string> EnumerateSystemApis();
        IEnumerable<DeviceInfo> EnumerateInputDevices();
        IEnumerable<DeviceInfo> EnumerateOutputDevices();        

        DeviceInfo GetDefaultInputDevice();
        DeviceInfo GetDefaultOutputDevice();
        bool ValidateInputDevice(DeviceInfo device);
        bool ValidateOutputDevice(DeviceInfo device);

        void Initialize();
        void FillOutputBuffer();

        void ValidateDeviceSettings();

        void Start();
        void Stop();
        void ResetBuffers();
    }
}
