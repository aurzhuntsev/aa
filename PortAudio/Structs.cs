using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PortAudioWrapper
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PaStreamCallbackTimeInfo
    {
        public double InputBufferAdcTime;
        public double CrrentTime;
        public double OutputBufferDacTime;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PaVersionInfo
    {
        public int VersionMajor;
        public int VersionMinor;
        public int VersionSubMinor;
        public string VersionControlRevision;
        public string VersionText;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PaHostApiInfo
    {
        public int StructVersion;
        public PaHostApiTypeId Type;
        public string Name;
        public int DeviceCount;
        public int DefaultInputDevice;
        public int DefaultOutputDevice;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PaHostErrorInfo
    {
        public PaHostApiTypeId HostApiType;
        public int ErrorCode;
        public string ErrorText;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PaDeviceInfo
    {
        public int StructVersion;

        public string Name;
        public int HostApi;

        public int MaxInputChannels;
        public int MaxOutputChannels;

        public double DefaultLowInputLatency;
        public double DefaultLowOutputLatency;
        public double DefaultHighInputLatency;
        public double DefaultHighOutputLatency;
        public double DefaultSampleRate;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PaStreamParameters
    {
        public int Device;
        public int ChannelCount;
        public PaSampleFormat SampleFormat;
        public double SuggestedLatency;
        public IntPtr HostApiSpecificStreamInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PaStreamInfo
    {
        public int StructVersion;
        public double InputLatency;
        public double OutputLatency;
        public double SampleRate;
    }
}
