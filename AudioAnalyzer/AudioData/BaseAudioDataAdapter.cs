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

        public EventHandler<AudioDataEventArgs> OnRead { get; private set; }
        public EventHandler<AudioDataEventArgs> OnWrite { get; private set; }

        public EventHandler<Exception> OnError { get; private set; }

        public abstract IEnumerable<string> EnumerateSystemApis();
        public abstract IEnumerable<DeviceInfo> EnumerateInputDevices();
        public abstract IEnumerable<DeviceInfo> EnumerateOutputDevices();

        public abstract DeviceInfo GetDefaultInputDevice();
        public abstract DeviceInfo GetDefaultOutputDevice();

        protected BaseAudioDataAdapter()
        {         
        }

        public virtual void Initialize()
        {                        
        }

        public void Start()
        {
            try
            {
                if (running)
                {
                    throw new InvalidOperationException("Already running.");
                }

                running = true;
              
                StartDevices();
            }
            catch (Exception e)
            {
                running = false;
                throw e;
            }
        }

        public void Stop()
        {
            running = false;            
            StopDevices();

            ResetBuffers();
        }
        
        protected abstract void StartDevices();
        protected abstract void StopDevices();
       
        public void ValidateDeviceSettings()
        {
            if (AppSettings.Current.Device.InputDevice == null || !ValidateInputDevice(AppSettings.Current.Device.InputDevice))
            {
                AppSettings.Current.Device.InputDevice = GetDefaultInputDevice();
                AppSettings.Current.Save();
            }            


            if (AppSettings.Current.Device.OutputDevice == null || !ValidateOutputDevice(AppSettings.Current.Device.OutputDevice))
            {
                AppSettings.Current.Device.OutputDevice = GetDefaultOutputDevice();
                AppSettings.Current.Save();
            }                
            
            /* TODO: Implement actual validation (e.g. missing device) */
        }

        public void SetReadHandler(EventHandler<AudioDataEventArgs> readHandler)
        {
            if (Running)
            {
                throw new InvalidOperationException("Cannot change handler while running");
            }

            OnRead = readHandler;
        }

        public void SetWriteHandler(EventHandler<AudioDataEventArgs> writeHandler)
        {
            if (Running)
            {
                throw new InvalidOperationException("Cannot change handler while running");
            }

            OnWrite = writeHandler;
        }
        
        public void SetErrorHandler(EventHandler<Exception> errorHandler)
        {
            if (Running)
            {
                throw new InvalidOperationException("Cannot change handler while running");
            }

            OnError = errorHandler;
        }
        
        public abstract bool ValidateInputDevice(DeviceInfo device);
        public abstract bool ValidateOutputDevice(DeviceInfo device);

        public void ResetBuffers()
        {
        }
    }
}
