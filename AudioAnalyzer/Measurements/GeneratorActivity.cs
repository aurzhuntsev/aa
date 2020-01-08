using AudioMark.Core.AudioData;
using AudioMark.Core.Common;
using AudioMark.Core.Generators;
using AudioMark.Core.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements
{
    public class GeneratorActivity : ActivityBase
    {        
        private IAudioDataAdapter _adapter;

        public Dictionary<int, IGenerator> Generators { get; }

        public delegate void ReadEvent(double[] buffer);
        public event ReadEvent OnRead;                

        public GeneratorActivity(string description)
            : base(description)
        {
            _adapter = AudioDataAdapterProvider.Get();
            Generators = new Dictionary<int, IGenerator>();            

            _adapter.OnWrite = new IAudioDataAdapter.DataWriteEventHandler((sender, buffer) =>
            {
                foreach (var channel in Generators.Keys)
                {
                    buffer[channel - 1] = Generators[channel].Next();
                }

                return buffer.Length;
            });

            _adapter.OnRead = new IAudioDataAdapter.DataReadEventHandler((sender, buffer, length) =>
            {
                CheckStopConditions();
                OnRead?.Invoke(buffer);
            });
        }        
        
        public void AddGenerator(int channel, IGenerator generator)
        {
            if (channel < 1 || channel > AppSettings.Current.Device.OutputDevice.ChannelsCount)
            {
                throw new InvalidOperationException("Invalid channel.");
            }

            if (Generators.ContainsKey(channel))
            {
                throw new InvalidOperationException($"Generator is already registered for channel {channel}.");
            }

            Generators.Add(channel, generator);
        }

        public override void Start()
        {
            base.Start();

            _adapter.Start();
        }

        public override void Stop()
        {
            base.Stop();

            _adapter.Stop();
        }
    }
}
