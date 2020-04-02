using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static PortAudioWrapper.Imports;

namespace PortAudioWrapper
{
    public class PortAudioStreamEventArgs : EventArgs
    {
        public double[] Buffer { get; set; }
        public int ActualLength { get; set; }
        public PaStreamCallbackFlags Errors { get; set; }
    }

    public class PortAudioStream : SafeHandle
    {
        const double BufferScaleFactor = 2.0;
        const int CallbackThreadWaitTimeoutMilliseconds = 3000;

        public override bool IsInvalid => handle == IntPtr.Zero;

        private UnmanagedStruct<PaStreamParameters> InputParameters = new UnmanagedStruct<PaStreamParameters>();
        private int _inputChannelsCount;
        private PaSampleFormat _inputSampleFormat;

        private UnmanagedStruct<PaStreamParameters> OutputParameters = new UnmanagedStruct<PaStreamParameters>();
        private int _outputChannelsCount;
        private PaSampleFormat _outputSampleFormat;

        private PaStreamCallback _nativeCallback;

        private EventWaitHandle _callbackWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        private Thread _callbackThread = null;
        private volatile bool _callbackReadProcessed = false;
        private volatile bool _callbackWriteProcessed = false;
        private volatile bool _running = false;
        private volatile int _lastFramesCount = 0;

        private PaStreamCallbackFlags _callbackError = 0;
        private Exception _callbackException = null;

        private double[] _inputBuffer;
        private int _inputChunkSizeBytes;
        private PortAudioStreamEventArgs _readEventArgs;

        private double[] _outputBuffer;
        private int _outputChunkSizeBytes;
        private PortAudioStreamEventArgs _writeEventArgs;

        public event EventHandler<PortAudioStreamEventArgs> OnRead;
        public event EventHandler<PortAudioStreamEventArgs> OnWrite;
        public event EventHandler<Exception> OnError;

        public PortAudioStream(PaStreamParameters inputParameters, PaStreamParameters outputParameters,
            double sampleRate, uint framesPerBuffer, uint streamFlags) : base(IntPtr.Zero, true)
        {
            IntPtr streamPointer = IntPtr.Zero;

            InputParameters.Value = inputParameters;
            if (InputParameters.Value.SuggestedLatency == 0)
            {
                throw new InvalidOperationException("PortAudioStream only supports stream with SuggestedLatency specified.");
            }

            _inputBuffer = new double[(int)(InputParameters.Value.SuggestedLatency * sampleRate * BufferScaleFactor)];
            _inputChunkSizeBytes = inputParameters.ChannelCount * Pa_GetSampleSize(inputParameters.SampleFormat);
            _inputChannelsCount = inputParameters.ChannelCount;
            _inputSampleFormat = inputParameters.SampleFormat;

            OutputParameters.Value = outputParameters;
            if (OutputParameters.Value.SuggestedLatency == 0)
            {
                throw new InvalidOperationException("PortAudioStream only supports stream with SuggestedLatency specified.");
            }

            _outputBuffer = new double[(int)(OutputParameters.Value.SuggestedLatency * sampleRate * BufferScaleFactor)];
            _outputChunkSizeBytes = outputParameters.ChannelCount * Pa_GetSampleSize(outputParameters.SampleFormat);
            _outputChannelsCount = outputParameters.ChannelCount;
            _outputSampleFormat = outputParameters.SampleFormat;

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

            _callbackThread = new Thread(CallbackHandlerAsync);
            _callbackThread.Start();

            Imports.Pa_StartStream(handle);
        }

        public void Stop()
        {
            _running = false;

            _callbackWaitHandle.Set();
            _callbackThread.Join(CallbackThreadWaitTimeoutMilliseconds);

            if (IsActive())
            {
                Pa_StopStream(handle);
            }
        }

        public void Abort()
        {
            _running = false;
            Pa_AbortStream(handle);
        }

        private void CallbackHandlerAsync(object obj)
        {
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            _readEventArgs = new PortAudioStreamEventArgs();
            //{
            //    Buffer = new double[_inputBuffer.Length]
            //};

            _writeEventArgs = new PortAudioStreamEventArgs();
            //{
            //    Buffer = new double[_outputBuffer.Length]
            //};

            while (_running)
            {
                try
                {
                    _callbackWaitHandle.WaitOne(CallbackThreadWaitTimeoutMilliseconds);

                    var readTask = Task.Run(() =>
                    {
                        int actualReadLength = _lastFramesCount * _inputChannelsCount;
                        //ReadFromBuffer(_inputBuffer, _readEventArgs.Buffer, actualReadLength, _inputSampleFormat);
                        _readEventArgs.Buffer = _inputBuffer;
                        _readEventArgs.ActualLength = actualReadLength;
                        _readEventArgs.Errors = _callbackError;
                        OnRead?.Invoke(this, _readEventArgs);

                        _callbackReadProcessed = true;
                    });

                    var writeTask = Task.Run(() =>
                    {
                        int actualWriteLength = _lastFramesCount * _outputChannelsCount;
                        _writeEventArgs.Buffer = _outputBuffer;
                        _writeEventArgs.ActualLength = actualWriteLength;
                        _writeEventArgs.Errors = _callbackError;
                        OnWrite?.Invoke(this, _writeEventArgs);

                        //WriteToBuffer(_writeEventArgs.Buffer, _outputBuffer, actualWriteLength);

                        _callbackWriteProcessed = true;
                    });

                    Task.WaitAll(readTask, writeTask);

                    _callbackError = 0;
                }
                catch (Exception e)
                {
                    OnError?.Invoke(this, e);
                    _running = false;
                }
            }

            if (_callbackError.HasFlag(PaStreamCallbackFlags.paCallbackException))
            {
                OnError?.Invoke(this, _callbackException);
            }
        }

        private unsafe void WriteToBuffer(double[] buffer, byte* outputBuffer, int actualWriteLength, PaSampleFormat sampleFormat)
        {
            for (var i = 0; i < actualWriteLength; i++)
            {
                if (sampleFormat == PaSampleFormat.PaFloat32)
                {
                    ((float*)outputBuffer)[i] = (float)buffer[i];
                }
                else if (sampleFormat == PaSampleFormat.PaInt16)
                {
                   ((short*)outputBuffer)[i] = (short)(buffer[i] * Int16.MaxValue);
                }
                else if (sampleFormat == PaSampleFormat.PaInt24)
                {
                    var value = (uint)(buffer[i] * int.MaxValue) >> 8;
                    outputBuffer[3 * i + 0] = (byte)(value & 0xff);
                    outputBuffer[3 * i + 1] = (byte)(value >> 8);
                    outputBuffer[3 * i + 2] = (byte)(value >> 16);
                }
            }
        }

        private unsafe void ReadFromBuffer(byte* inputBuffer, double[] buffer, int actualReadLength, PaSampleFormat sampleFormat)
        {
            for (var i = 0; i < actualReadLength; i++)
            {
                if (sampleFormat == PaSampleFormat.PaFloat32)
                {
                    buffer[i] = ((float*)inputBuffer)[i];
                }
                else if (sampleFormat == PaSampleFormat.PaInt16)
                {
                    buffer[i] = (double)((short*)inputBuffer)[i] / short.MaxValue;                      
                }
                else if (sampleFormat == PaSampleFormat.PaInt24)
                {
                    buffer[i] =
                    (double)(
                     (inputBuffer[i * 3 + 0] << 8) |
                     (inputBuffer[i * 3 + 1] << 16) |
                     (inputBuffer[i * 3 + 2] << 24)) / int.MaxValue;                                       
                }
            }
        }

        protected override bool ReleaseHandle()
        {
            return true;
        }

        private unsafe int NativeCallbackHandler(IntPtr input, IntPtr output, uint frameCount, IntPtr timeInfo, uint statusFlags, IntPtr userData)
        {
            var latencyMode = System.Runtime.GCSettings.LatencyMode;
            try
            {
                System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.LowLatency;

                if (!_running)
                {
                    return (int)PaStreamCallbackResult.PaComplete;
                }

                _callbackError = (PaStreamCallbackFlags)statusFlags;
                if (!_callbackReadProcessed)
                {
                    _callbackError |= PaStreamCallbackFlags.paCallbackReadNotProcessed;
                }
                if (!_callbackWriteProcessed)
                {
                    _callbackError |= PaStreamCallbackFlags.paCallbackWriteNotProcessed;
                }

                _lastFramesCount = (int)frameCount;
                //Marshal.Copy(input, _inputBuffer, 0, _inputChunkSizeBytes * _lastFramesCount);
                //Marshal.Copy(_outputBuffer, 0, output, _outputChunkSizeBytes * _lastFramesCount);
                ReadFromBuffer((byte*)input, _inputBuffer, _lastFramesCount * _inputChannelsCount, _inputSampleFormat);
                WriteToBuffer(_outputBuffer, (byte*)output, _lastFramesCount * _outputChannelsCount, _outputSampleFormat);

                _callbackReadProcessed = false;
                _callbackWriteProcessed = false;
                _callbackWaitHandle.Set();

                return (int)PaStreamCallbackResult.PaContinue;
            }
            catch (Exception e)
            {
                _callbackError |= PaStreamCallbackFlags.paCallbackException;
                _callbackException = e;
                _running = false;

                return (int)PaStreamCallbackResult.PaComplete;
            }
            finally
            {
                System.Runtime.GCSettings.LatencyMode = latencyMode;
            }
        }

        public bool IsActive()
        {
            var isActive = Pa_IsStreamActive(handle);
            return isActive > 0;
        }
    }
}
