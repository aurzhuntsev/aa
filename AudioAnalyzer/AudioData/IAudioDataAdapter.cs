using System;
using System.Collections.Generic;
using System.Text;
using static AudioMark.Core.Settings.Device;

namespace AudioMark.Core.AudioData
{
    public class AudioDataEventArgs: EventArgs
    {
        public double[] Buffer { get; set; }
        public int Length { get; set; }
        public int Channels { get; set; }        
        public bool Discard { get; set; }

        public int Frames
        {
            get => Length / Channels;
        }
    }

    public interface IAudioDataAdapter
    {
        bool Running { get; }
               
        EventHandler<AudioDataEventArgs> OnRead { get; }
        EventHandler<AudioDataEventArgs> OnWrite { get; }
        EventHandler<Exception> OnError { get; }

        void SetReadHandler(EventHandler<AudioDataEventArgs> readHandler);
        void SetWriteHandler(EventHandler<AudioDataEventArgs> writeHandler);
        void SetErrorHandler(EventHandler<Exception> errorHandler);

        IEnumerable<string> EnumerateSystemApis();
        IEnumerable<DeviceInfo> EnumerateInputDevices();
        IEnumerable<DeviceInfo> EnumerateOutputDevices();        

        DeviceInfo GetDefaultInputDevice();
        DeviceInfo GetDefaultOutputDevice();
        bool ValidateInputDevice(DeviceInfo device);
        bool ValidateOutputDevice(DeviceInfo device);

        void Initialize();        
        void ValidateDeviceSettings();

        void Start();
        void Stop();
        void ResetBuffers();
    }
}
