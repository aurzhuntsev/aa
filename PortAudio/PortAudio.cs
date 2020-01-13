using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace PortAudioWrapper
{

    public class PortAudio : SafeHandle
    {
        private static Lazy<PortAudio> _instance = new Lazy<PortAudio>(() => new PortAudio());

        public static PortAudio Instance => _instance.Value;

        public override bool IsInvalid => false;

        private Thread _thread = null;

        private PortAudio() : base(IntPtr.Zero, true)
        {
            ThrowIfError(Imports.Pa_Initialize());
        }

        protected override bool ReleaseHandle()
        {
            try
            {
                ThrowIfError(Imports.Pa_Terminate());
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void Initialize()
        {
            _ = _instance.Value;
        }

        public int GetDefaultInputDeviceIndex()
        {
            return Imports.Pa_GetDefaultInputDevice();
        }

        public int GetDefaultOutputDeviceIndex()
        {
            return Imports.Pa_GetDefaultOutputDevice();
        }

        private void ThrowIfError(int paError)
        {
            if (paError != (int)PaErrorCode.PaNoError)
            {
                throw new PortAudioException(paError);
            }
        }

        public int GetDeviceCount()
        {
            return Imports.Pa_GetDeviceCount();
        }

        public PaDeviceInfo GetDeivceInfo(int deviceIndex)
        {
            var ptr = Imports.Pa_GetDeviceInfo(deviceIndex);
            if (ptr == IntPtr.Zero)
            {
                throw new ExternalException(); /*TODO change */
            }

            return Marshal.PtrToStructure<PaDeviceInfo>(ptr);
        }

        public bool IsFormatSupported(PaStreamParameters? inputStreamParameters, PaStreamParameters? outputStreamParameters, double sampleRate)
        {
            var inputStreamParametersPtr = IntPtr.Zero;
            var outputStreamParametersPtr = IntPtr.Zero;

            if (inputStreamParameters.HasValue)
            {
                inputStreamParametersPtr = Marshal.AllocHGlobal(Marshal.SizeOf<PaStreamParameters>());
                Marshal.StructureToPtr<PaStreamParameters>(inputStreamParameters.Value, inputStreamParametersPtr, false);
            }

            if (outputStreamParameters.HasValue)
            {
                outputStreamParametersPtr = Marshal.AllocHGlobal(Marshal.SizeOf<PaStreamParameters>());
                Marshal.StructureToPtr<PaStreamParameters>(outputStreamParameters.Value, outputStreamParametersPtr, false);
            }

            var isFormatSupported = (Imports.Pa_IsFormatSupported(inputStreamParametersPtr, outputStreamParametersPtr, sampleRate) == 0);

            if (inputStreamParametersPtr != IntPtr.Zero)
            {
                Marshal.DestroyStructure<PaStreamParameters>(inputStreamParametersPtr);
                Marshal.FreeHGlobal(inputStreamParametersPtr);
            }

            if (outputStreamParametersPtr != IntPtr.Zero)
            {
                Marshal.DestroyStructure<PaStreamParameters>(outputStreamParametersPtr);
                Marshal.FreeHGlobal(outputStreamParametersPtr);
            }

            return isFormatSupported;
        }

        public int GetHostApiCount()
        {
            return Imports.Pa_GetHostApiCount();
        }

        public PaHostApiInfo GetHostApiInfo(int hostApiIndex)
        {
            var ptr = Imports.Pa_GetHostApiInfo(hostApiIndex);
            if (ptr == IntPtr.Zero)
            {
                throw new ExternalException(); /*TODO change */
            }

            return Marshal.PtrToStructure<PaHostApiInfo>(ptr);
        }
    }
}
