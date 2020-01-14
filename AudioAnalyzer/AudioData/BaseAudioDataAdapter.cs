using AudioMark.Core.Common;
using AudioMark.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AudioMark.Core.AudioData
{
    public abstract class BaseAudioDataAdapter : IAudioDataAdapter
    {        
        private volatile bool running = false;
        public bool Running => running;

        private int ringBufferSize = 2;
        protected double[] xb = null;

        protected RingBuffer InputBuffer { get; set; }
        protected EventWaitHandle InputWaitHandle { get; set; } = new EventWaitHandle(false, EventResetMode.ManualReset);        
        protected bool DiscardInput { get; set; }

        protected RingBuffer OutputBuffer { get; set; }
        protected EventWaitHandle OutputWaitHandle { get; set; } = new EventWaitHandle(false, EventResetMode.ManualReset);                
        protected bool DiscardOutput { get; set; }

        private Thread inputProcessingThread = null;                
        private Thread outputProcessingThread = null;        

        public IAudioDataAdapter.DataReadEventHandler OnRead { get; set; }
        public IAudioDataAdapter.DataWriteEventHandler OnWrite { get; set; }

        public abstract IEnumerable<DeviceInfo> EnumerateInputDevices();
        public abstract IEnumerable<DeviceInfo> EnumerateOutputDevices();

        public abstract DeviceInfo GetDefaultInputDevice();
        public abstract DeviceInfo GetDefaultOutputDevice();

        protected BaseAudioDataAdapter()
        {         
        }

        public virtual void Initialize()
        {            
            InputBuffer = new RingBuffer(ringBufferSize * AppSettings.Current.Device.BufferSize, AppSettings.Current.Device.InputDevice.ChannelsCount);
            OutputBuffer = new RingBuffer(ringBufferSize * AppSettings.Current.Device.BufferSize, AppSettings.Current.Device.OutputDevice.ChannelsCount);
        }

        public void Start()
        {            
            if (running)
            {
                throw new InvalidOperationException("Already running.");
            }

            running = true;

            if (OnRead != null)
            {
                inputProcessingThread = new Thread(() =>
                {
                    Thread.CurrentThread.Priority = ThreadPriority.Highest;
                    while (running)
                    {
                        InputWaitHandle.WaitOne();
                        InputWaitHandle.Reset();

                        while (InputBuffer.Read((buffer, length) =>
                        {
                            OnRead(this, buffer, length, DiscardInput);
                        })) { };

                        DiscardInput = false;
                    }
                });

                inputProcessingThread.Start();
            }

            if (OnWrite != null)
            {                
                outputProcessingThread = new Thread(() =>
                {
                    Thread.CurrentThread.Priority = ThreadPriority.Highest;
                    while (running)
                    {
                        OutputWaitHandle.WaitOne();
                        OutputWaitHandle.Reset();

                        while (OutputBuffer.Write((buffer) =>
                        {
                            return OnWrite(this, buffer, DiscardOutput);
                        })) { };

                        DiscardOutput = false;
                    }
                });

                outputProcessingThread.Start();
            }

            StartDevices();
        }

        public void Stop()
        {
            running = false;            
            StopDevices();

            InputWaitHandle.Set();
            OutputWaitHandle.Set();
        }

        protected abstract void StartDevices();
        protected abstract void StopDevices();
       
        public void ValidateDeviceSettings()
        {
            if (AppSettings.Current.Device.InputDevice == null)
            {
                AppSettings.Current.Device.InputDevice = GetDefaultInputDevice();
                AppSettings.Current.Save();
            }

            if (AppSettings.Current.Device.OutputDevice == null)
            {
                AppSettings.Current.Device.OutputDevice = GetDefaultOutputDevice();
                AppSettings.Current.Save();
            }                
            
            /* TODO: Implement actual validation (e.g. missing device) */
        }

        public void FillOutputBuffer()
        {
            while (OutputBuffer.Write((buffer) =>
            {
                return OnWrite(this, buffer, false);
            })) { };
        }
    }
}
