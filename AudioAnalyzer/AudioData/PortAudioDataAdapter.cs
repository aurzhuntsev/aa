using AudioMark.Core.Settings;
using PortAudioWrapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;

namespace AudioMark.Core.AudioData
{
    public class PortAudioDataAdapter : BaseAudioDataAdapter
    {
        private class PaDevice
        {
            public int Index { get; set; }
            public PaDeviceInfo Info { get; set; }
        }

        private class PaHostApi
        {
            public int Index { get; set; }
            public PaHostApiInfo Info { get; set; }
        }

        private static PaSampleFormat ToPortAudioSampleFormat(SampleFormat sampleFormat)
        {
            if (sampleFormat == SampleFormat.Float32)
            {
                return PaSampleFormat.PaFloat32;
            }
            else if (sampleFormat == SampleFormat.Int16)
            {
                return PaSampleFormat.PaInt16;
            }
            else if (sampleFormat == SampleFormat.Int24)
            {
                return PaSampleFormat.PaInt24;
            }

            throw new NotSupportedException(sampleFormat.ToString());
        }

        private static Lazy<List<PaDevice>> deviceCache = new Lazy<List<PaDevice>>(() =>
        {
            var deviceList = new List<PaDevice>();
            var deviceCount = PortAudio.Instance.GetDeviceCount();

            for (var deviceIndex = 0; deviceIndex < deviceCount; deviceIndex++)
            {
                deviceList.Add(new PaDevice()
                {
                    Index = deviceIndex,
                    Info = PortAudio.Instance.GetDeivceInfo(deviceIndex)
                });
            }

            return deviceList;
        });

        private static Lazy<List<PaHostApi>> hostApiCache = new Lazy<List<PaHostApi>>(() =>
        {
            var apiList = new List<PaHostApi>();
            var apiCount = PortAudio.Instance.GetHostApiCount();

            for (var apiIndex = 0; apiIndex < apiCount; apiIndex++)
            {
                apiList.Add(new PaHostApi()
                {
                    Index = apiIndex,
                    Info = PortAudio.Instance.GetHostApiInfo(apiIndex)
                });
            }

            return apiList;
        });

        private static string GetApiName(int apiIndex) => hostApiCache.Value.FirstOrDefault(api => api.Index == apiIndex).Info.Name;

        private Lazy<List<DeviceInfo>> inputDevices = new Lazy<List<DeviceInfo>>(() =>
        {
            return EnumerateDevices(true).ToList();
        });

        private Lazy<List<DeviceInfo>> outputDevices = new Lazy<List<DeviceInfo>>(() =>
        {
            return EnumerateDevices(false).ToList();
        });

        private PortAudioStream stream = null;

        public PortAudioDataAdapter() : base()
        {
         
        }

        public override void Initialize()
        {
            base.Initialize();
            PortAudio.Initialize();

            var inputStreamParameters = new PaStreamParameters()
            {
                ChannelCount = AppSettings.Current.Device.InputDevice.ChannelsCount,
                Device = AppSettings.Current.Device.InputDevice.Index,
                SampleFormat = ToPortAudioSampleFormat(AppSettings.Current.Device.InputDevice.SampleFormat),
                SuggestedLatency = 0.128
            };

            var outputStreamParameters = new PaStreamParameters()
            {
                ChannelCount = AppSettings.Current.Device.OutputDevice.ChannelsCount,
                Device = AppSettings.Current.Device.OutputDevice.Index,
                SampleFormat = ToPortAudioSampleFormat(AppSettings.Current.Device.OutputDevice.SampleFormat),
                SuggestedLatency = 0.128
            };

            stream = new PortAudioStream(inputStreamParameters, outputStreamParameters, AppSettings.Current.Device.SampleRate, 0, 0x00000004 | 0x00000002,
                DataCallback);
        }

        private static IEnumerable<DeviceInfo> EnumerateDevices(bool enumerateInputDevices)
        {
            var result = new List<DeviceInfo>();
            var streamParameters = new PaStreamParameters();
            var devices = enumerateInputDevices
                            ? deviceCache.Value.Where(device => device.Info.MaxInputChannels > 0)
                            : deviceCache.Value.Where(device => device.Info.MaxOutputChannels > 0);

            var sampleFormats = ((SampleFormat[])Enum.GetValues(typeof(SampleFormat)))
                .Select(value => new { SampleFormat = value, PaSampleFormat = ToPortAudioSampleFormat(value) })
                .ToList();

            foreach (var device in devices)
            {
                streamParameters.Device = device.Index;
                foreach (var sampleFormat in sampleFormats)
                {
                    streamParameters.SampleFormat = sampleFormat.PaSampleFormat;
                    foreach (var sampleRate in AppSettings.Current.Device.DefaultSampleRates)
                    {
                        var maxChannels = enumerateInputDevices
                                        ? device.Info.MaxInputChannels
                                        : device.Info.MaxOutputChannels;

                        int channelCount;
                        for (channelCount = 1; channelCount < maxChannels + 1; channelCount++)
                        {
                            streamParameters.ChannelCount = channelCount;
                            var isFormatSupported = enumerateInputDevices
                                                    ? PortAudio.Instance.IsFormatSupported(streamParameters, null, sampleRate)
                                                    : PortAudio.Instance.IsFormatSupported(null, streamParameters, sampleRate);

                            if (!isFormatSupported)
                            {
                                break;
                            }
                        }

                        if (channelCount - 1 > 0)
                        {
                            result.Add(new DeviceInfo()
                            {
                                Index = device.Index,

                                ApiName = GetApiName(device.Info.HostApi),
                                Name = device.Info.Name,
                                ChannelsCount = channelCount - 1,

                                SampleFormat = sampleFormat.SampleFormat,
                                SampleRate = sampleRate
                            });
                        }
                    }
                }
            }

            return result;
        }

        public override IEnumerable<DeviceInfo> EnumerateInputDevices() => inputDevices.Value;
        public override IEnumerable<DeviceInfo> EnumerateOutputDevices() => outputDevices.Value;

        public override DeviceInfo GetDefaultInputDevice() => inputDevices.Value.Where(device => device.Index == PortAudio.Instance.GetDefaultInputDeviceIndex())
                .OrderBy(device => device.SampleFormat)
                .ThenBy(device => device.SampleRate)
                .FirstOrDefault();

        public override DeviceInfo GetDefaultOutputDevice() => outputDevices.Value.Where(device => device.Index == PortAudio.Instance.GetDefaultOutputDeviceIndex())
                .OrderBy(device => device.SampleFormat)
                .ThenBy(device => device.SampleRate)
                .FirstOrDefault();

        protected override void StartDevices()
        {
            stream.Start();
        }

        protected override void StopDevices()
        {
            stream.Stop();
        }
        
        private PaStreamCallbackResult DataCallback(PortAudioDoubleBuffer inputBuffer, PortAudioDoubleBuffer outputBuffer, PaStreamCallbackTimeInfo timeInfo, uint streamFlags)
        {
            if (!Running)
            {
                return PaStreamCallbackResult.PaComplete;
            }

            int read = 0;
            while (read < inputBuffer.Length)
            {
                if (!InputBuffer.WriteNoWait((buffer) =>
                {
                    for (var i = 0; i < buffer.Length; i++)
                    {
                        buffer[i] = inputBuffer[read];                        
                        read++;                        
                    }
                    return buffer.Length;
                }))
                {
                    InputWaitHandle.Set();
                    break;
                    /* TODO: Handle underflow */
                };                
            }
            InputWaitHandle.Set();

            int wrote = 0;
            while (wrote < outputBuffer.Length)
            {
                if (!OutputBuffer.ReadNoWait((buffer, length) =>
                {
                    for (var i = 0; i < length; i++)
                    {
                        outputBuffer[wrote] = buffer[i];
                        wrote++;
                    }
                }))
                {
                    OutputWaitHandle.Set();
                    break;
                    /* TODO: Handle underflow */
                }
            }
            OutputWaitHandle.Set();
            
            return PaStreamCallbackResult.PaContinue;
        }
    }
}
