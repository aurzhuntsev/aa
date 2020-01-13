using AudioMark.Core.AudioData;
using AudioMark.Core.Common;
using AudioMark.Core.Generators;
using AudioMark.Core.Settings;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AudioMark.Core.Measurements
{
    public class GeneratorActivity : ActivityBase
    {
        private IAudioDataAdapter _adapter;

        public Dictionary<int, IGenerator> Generators { get; }

        public delegate void ReadEvent(double[] buffer, bool discard);
        public event ReadEvent OnRead;

        private DateTime _lastStopConditionsChecked;
        private object _stopConditionCheckSync = new object();

        public GeneratorActivity(string description)
            : base(description)
        {
            _adapter = AudioDataAdapterProvider.Get();
            Generators = new Dictionary<int, IGenerator>();

            _adapter.OnWrite = new IAudioDataAdapter.DataWriteEventHandler((sender, buffer, discard) =>
            {
                foreach (var channel in Generators.Keys)
                {
                    buffer[channel - 1] = Generators[channel].Next();
                }

                return buffer.Length;
            });

            _adapter.OnRead = new IAudioDataAdapter.DataReadEventHandler((sender, buffer, length, discard) =>
            {
                if (DateTime.Now.Subtract(_lastStopConditionsChecked).Duration().TotalMilliseconds >= AppSettings.Current.StopConditions.CheckIntervalMilliseconds)
                {
                    _lastStopConditionsChecked = DateTime.Now;
                    Task.Run(() =>
                    {
                        lock (_stopConditionCheckSync)
                        {
                            CheckStopConditions();                    
                        }
                    });
                }                
                OnRead?.Invoke(buffer, discard);
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
            
            _adapter.FillOutputBuffer();

            _lastStopConditionsChecked = DateTime.Now;
            _adapter.Start();
        }

        public override void Stop()
        {
            base.Stop();

            _adapter.Stop();
        }
    }
}
