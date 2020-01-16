using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using static PortAudioWrapper.Imports;

namespace PortAudioWrapper
{
    public class PortAudioStreamEventArgs : EventArgs
    {
        public double[] Buffer { get; set; }
        public int ActualLength { get; set; }
        public uint Errors { get; set; }
    }

    public class PortAudioStream : SafeHandle
    {
        const double BufferScaleFactor = 2.0;

        public override bool IsInvalid => handle == IntPtr.Zero;

        private PortAudioDoubleBuffer InputBuffer { get; set; }
        private UnmanagedStruct<PaStreamParameters> InputParameters = new UnmanagedStruct<PaStreamParameters>();
        private int _inputChannelsCount;

        private PortAudioDoubleBuffer OutputBuffer { get; set; }
        private UnmanagedStruct<PaStreamParameters> OutputParameters = new UnmanagedStruct<PaStreamParameters>();
        private int _outputChannelsCount;

        private PaStreamCallback _nativeCallback;

        private EventWaitHandle _callbackWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        private Thread _callbackThread = null;
        private volatile bool _callbackProcessed = false;
        private volatile bool _running = false;
        private volatile uint _callbackError = 0;
        private volatile int _lastFramesCount = 0;

        private byte[] _inputBuffer;
        private int _inputChunkSizeBytes;
        private PortAudioStreamEventArgs _readEventArgs;

        private byte[] _outputBuffer;
        private int _outputChunkSizeBytes;
        private PortAudioStreamEventArgs _writeEventArgs;

        public event EventHandler<PortAudioStreamEventArgs> OnRead;
        public event EventHandler<PortAudioStreamEventArgs> OnWrite;

        public PortAudioStream(PaStreamParameters inputParameters, PaStreamParameters outputParameters,
            double sampleRate, uint framesPerBuffer, uint streamFlags) : base(IntPtr.Zero, true)
        {
            IntPtr streamPointer = IntPtr.Zero;

            InputParameters.Value = inputParameters;
            if (InputParameters.Value.SuggestedLatency == 0)
            {
                throw new InvalidOperationException("PortAudioStream only supports stream with SuggestedLatency specified.");
            }

            _inputBuffer = new byte[(int)(InputParameters.Value.SuggestedLatency * sampleRate * BufferScaleFactor * 4)];
            _inputChunkSizeBytes = inputParameters.ChannelCount * 4; /* TODO:!!! Match sample format! */
            _inputChannelsCount = inputParameters.ChannelCount;

            OutputParameters.Value = outputParameters;
            if (OutputParameters.Value.SuggestedLatency == 0)
            {
                throw new InvalidOperationException("PortAudioStream only supports stream with SuggestedLatency specified.");
            }

            _outputBuffer = new byte[(int)(OutputParameters.Value.SuggestedLatency * sampleRate * BufferScaleFactor * 4)];
            _outputChunkSizeBytes = outputParameters.ChannelCount * 4; /* TODO:!!! Match sample format! */
            _outputChannelsCount = outputParameters.ChannelCount;

            _nativeCallback = new PaStreamCallback(NativeCallbackHandler);

            int result = Imports.Pa_OpenStream(out streamPointer,
                InputParameters.Pointer, OutputParameters.Pointer,
                sampleRate, framesPerBuffer, streamFlags,
                Marshal.GetFunctionPointerForDelegate(_nativeCallback),
                IntPtr.Zero);
            SetHandle(streamPointer);

            if (result != (int)PaErrorCode.PaNoError)
            {
                throw new PortAudioException(result);
            }
        }

        public void Start()
        {
            _running = true;

            _callbackThread = new Thread(CallbackHandler);
            _callbackThread.Start();

            Imports.Pa_StartStream(handle);
        }

        public void Stop()
        {            
            _running = false;
            _callbackThread.Join();

            Imports.Pa_StopStream(handle);
        }

        private void CallbackHandler(object obj)
        {

            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            _readEventArgs = new PortAudioStreamEventArgs()
            {
                Buffer = new double[_inputBuffer.Length]
            };

            _writeEventArgs = new PortAudioStreamEventArgs()
            {
                Buffer = new double[_outputBuffer.Length]
            };

            while (_running)
            {
                _callbackWaitHandle.WaitOne();
                _callbackWaitHandle.Reset();

                /* TODO: Implement different formats support */
                int actualReadLength = _lastFramesCount * _inputChannelsCount;
                for (var i = 0; i < actualReadLength; i++)
                {
                    _readEventArgs.Buffer[i] = (double)System.BitConverter.ToSingle(_inputBuffer, i * 4);
                }
                _readEventArgs.ActualLength = actualReadLength;
                _readEventArgs.Errors = _callbackError;
                OnRead?.Invoke(this, _readEventArgs);

                int actualWriteLength = _lastFramesCount * _outputChannelsCount;
                _writeEventArgs.ActualLength = actualWriteLength;
                _writeEventArgs.Errors = _callbackError;
                OnWrite?.Invoke(this, _writeEventArgs);

                for (var i = 0; i < actualWriteLength; i++)
                {
                    var bytes = BitConverter.GetBytes((float)_writeEventArgs.Buffer[i]);
                    for (var j = 0; j < 4; j++)
                    {
                        _outputBuffer[4 * i + j] = bytes[j];
                    }
                }

                _callbackError = 0;
                _callbackProcessed = true;
            }
        }

        protected override bool ReleaseHandle()
        {
            return true;
        }

        private unsafe int NativeCallbackHandler(IntPtr input, IntPtr output, uint frameCount, IntPtr timeInfo, uint statusFlags, IntPtr userData)
        {
            _callbackError = statusFlags;
            if (!_callbackProcessed)
            {
                _callbackError |= 0x00000032; /* TODO: Add stream flags/callback errors constants */
            }

            _lastFramesCount = (int)frameCount;
            Marshal.Copy(input, _inputBuffer, 0, _inputChunkSizeBytes * _lastFramesCount);
            Marshal.Copy(_outputBuffer, 0, output, _outputChunkSizeBytes * _lastFramesCount);

            _callbackProcessed = false;
            _callbackWaitHandle.Set();

            return (int)PaStreamCallbackResult.PaContinue;
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
