
using AudioMark.Core.AudioData;
using AudioMark.Core.Common;
using AudioMark.Core.Generators;
using AudioMark.Core.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Core.Measurements
{
    public class Tuner
    {
        public const double TunerFrequency = 440.0;
        public const int UpdatesPerSecond = 10;

        public class Reading
        {
            public double InputLevelDbTp { get; set; }
            public double InputLevelDbFs { get; set; }
        }

        public event EventHandler<Reading> OnReading;
        public event EventHandler<Exception> OnError;

        private volatile bool _running;
        public bool Running
        {
            get => _running;
            private set => _running = value;
        }

        private IAudioDataAdapter _adapter;
        private SineGenerator _generator;
        private int _counter = 0;

        private double _outputLevel;
        public double OutputLevel
        {
            get => _outputLevel;
            set
            {
                _outputLevel = value;             
            }
        }

        public int InputChannel { get; set; } = AppSettings.Current.Device.PrimaryInputChannel;
        public int OutputChannel { get; set; } = AppSettings.Current.Device.PrimaryOutputChannel;

        public Tuner()
        {
            _adapter = AudioDataAdapterProvider.Get();         
        }

        public void Test()
        {
            /* TODO: Change to support different channels */
            _generator = new SineGenerator(AppSettings.Current.Device.SampleRate, TunerFrequency, Math.Pow(10.0, _outputLevel / 20.0));

            double maxValue = double.MinValue;
            double power = 0.0;
            double previousValue = double.NaN;
            double powerNorm = (double)UpdatesPerSecond / AppSettings.Current.Device.SampleRate;

            _adapter.SetReadHandler((sender, buffer, length, discard) =>
            {
                var value = Math.Abs(buffer[InputChannel - 1]);
                if (value > maxValue)
                {
                    maxValue = value;
                }

                if (!double.IsNaN(previousValue))
                {
                    power += (Math.Min(previousValue, value) + 0.5 * Math.Abs(value - previousValue));
                }
                previousValue = value;

                _counter++;
                if (_counter == AppSettings.Current.Device.SampleRate / UpdatesPerSecond)
                {
                    OnReading?.Invoke(this, new Reading()
                    {
                        InputLevelDbFs = power == 1.0 ? 0 : 10.0 * Math.Log10(1.0 / (powerNorm * power)),
                        InputLevelDbTp = maxValue == 1.0 ? 0 : 20.0 * Math.Log10(1.0 / maxValue)
                    });

                    _counter = 0;
                    maxValue = double.MinValue;
                    power = 0.0;
                }
            });

            _adapter.SetWriteHandler((sender, buffer, discard) =>
            {
                buffer[OutputChannel - 1] = _generator.Next();
                return buffer.Length;
            });

            _adapter.SetErrorHandler((sender, e) =>
            {
                Stop();
                OnError?.Invoke(sender, e);
            });

            Running = true;
            _adapter.Start();
        }

        /* TODO: Implement at some point */
        public void Tune()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            if (Running)
            {
                Running = false;
                _adapter.Stop();
            }
        }
    }
}
