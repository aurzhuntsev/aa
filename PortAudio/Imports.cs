using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PortAudioWrapper
{
    public static class Imports
    {
        public const string ImportName = "portaudio/portaudio";

        [DllImport(ImportName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Pa_Initialize();

        [DllImport(ImportName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Pa_Terminate();

        [DllImport(ImportName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Pa_OpenStream(out IntPtr stream, IntPtr inputParameters, IntPtr outputParameters, double sampleRate, uint framesPerBuffer, uint streamFlags, IntPtr streamCallback, in IntPtr userData);

        [DllImport(ImportName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Pa_StartStream(IntPtr stream);

        [DllImport(ImportName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Pa_StopStream(IntPtr stream);

        //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int PaStreamCallback(IntPtr input, IntPtr output, uint frameCount, IntPtr timeInfo, uint statusFlags, IntPtr userData);

        [DllImport(ImportName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Pa_GetDefaultInputDevice();

        [DllImport(ImportName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Pa_GetDefaultOutputDevice();


        [DllImport(ImportName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Pa_GetErrorText(int errorCode);

        [DllImport(ImportName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Pa_OpenDefaultStream(out IntPtr stream, int numInputChannels, int numOutputChannels, int sampleFormat, double sampleRate, uint framesPerBuffer,
          PaStreamCallback streamCallback, in IntPtr userData);

        [DllImport(ImportName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Pa_GetDeviceCount();

        [DllImport(ImportName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Pa_GetDeviceInfo(int device);

        [DllImport(ImportName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Pa_IsFormatSupported(IntPtr inputParameters, IntPtr outputParameters, double sampleRate);

        [DllImport(ImportName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Pa_GetHostApiCount();

        [DllImport(ImportName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Pa_GetHostApiInfo(int hostApi);
    }
}
