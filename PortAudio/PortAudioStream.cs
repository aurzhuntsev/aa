using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using static PortAudioWrapper.Imports;

namespace PortAudioWrapper
{
    public class PortAudioStream : SafeHandle
    {
        public override bool IsInvalid => handle == IntPtr.Zero;
        private Random r = new Random();
        private PortAudioDoubleBuffer InputBuffer { get; set; }
        private UnmanagedStruct<PaStreamParameters> InputParameters = new UnmanagedStruct<PaStreamParameters>();

        private PortAudioDoubleBuffer OutputBuffer { get; set; }
        private UnmanagedStruct<PaStreamParameters> OutputParameters = new UnmanagedStruct<PaStreamParameters>();

        private PaStreamCallback nativeCallback;
        private Func<PortAudioDoubleBuffer, PortAudioDoubleBuffer, PaStreamCallbackTimeInfo, uint, PaStreamCallbackResult> callback = null;

        public PortAudioStream(PaStreamParameters? inputParameters, PaStreamParameters? outputParameters,
            double sampleRate, uint framesPerBuffer, uint streamFlags,
            Func<PortAudioDoubleBuffer, PortAudioDoubleBuffer, PaStreamCallbackTimeInfo, uint, PaStreamCallbackResult> callback) : base(IntPtr.Zero, true)
        {
            IntPtr streamPointer = IntPtr.Zero;

            if (inputParameters.HasValue)
            {
                InputParameters.Value = inputParameters.Value;
            }

            if (outputParameters.HasValue)
            {
                OutputParameters.Value = outputParameters.Value;
            }

            nativeCallback = new PaStreamCallback(NativeCallbackHandler);
            this.callback = callback;

            int result = Imports.Pa_OpenStream(out streamPointer,
                InputParameters.Pointer, OutputParameters.Pointer,
                sampleRate, framesPerBuffer, streamFlags,
                Marshal.GetFunctionPointerForDelegate(nativeCallback),
                IntPtr.Zero);
            SetHandle(streamPointer);

            if (result != (int)PaErrorCode.PaNoError)
            {
                var ex = new PortAudioException(result);
                throw ex;
            }
        }

        public void Start()
        {
            Imports.Pa_StartStream(handle);
        }

        public void Stop()
        {
            Imports.Pa_StopStream(handle);
        }

        protected override bool ReleaseHandle()
        {
            return true;
        }

        private unsafe int NativeCallbackHandler(IntPtr input, IntPtr output, uint frameCount, IntPtr timeInfo, uint statusFlags, IntPtr userData)
        {
            if (InputParameters.HasValue)
            {
                if (InputBuffer == null)
                {
                    InputBuffer = CreatePortAudioFloatBuffer(input, InputParameters.Value);
                }

                InputBuffer.Length = (int)frameCount * InputParameters.Value.ChannelCount;
            }

            if (OutputParameters.HasValue)
            {
                if (OutputBuffer == null)
                {
                    OutputBuffer = CreatePortAudioFloatBuffer(output, OutputParameters.Value);
                }

                OutputBuffer.Length = (int)frameCount * OutputParameters.Value.ChannelCount;
            }

            return (int)callback(InputBuffer, OutputBuffer, Marshal.PtrToStructure<PaStreamCallbackTimeInfo>(timeInfo), statusFlags);
        }

        private PortAudioDoubleBuffer CreatePortAudioFloatBuffer(IntPtr pointer, PaStreamParameters streamParameters)
        {
            if (streamParameters.SampleFormat == PaSampleFormat.PaFloat32)
            {
                return new PortAudioDoubleFloatBuffer(pointer);
            }
            else if (streamParameters.SampleFormat == PaSampleFormat.PaInt24)
            {
                return new PortAudioDoubleInt24Buffer(pointer);
            }

            throw new Exception($"Sample format of {streamParameters.SampleFormat} is not supported.");
        }
    }
}
