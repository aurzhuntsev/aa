﻿using AudioMark.Core.AudioData;
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
        private IGenerator _generator;
        private int _counter = 0;


        public double InputLevel { get; set; }
        public double OutputLevel { get; set; }
        public SignalLevelMode SignalLevelMode { get; set; }

        public Tuner()
        {
            _adapter = AudioDataAdapterProvider.Get();
            _generator = new SineGenerator(AppSettings.Current.Device.SampleRate, TunerFrequency);

            /* TODO: Change to support different channels */

            double maxValue = double.MinValue;
            double power = 0.0;
            double previousValue = double.NaN;
            double powerNorm = (double)UpdatesPerSecond / AppSettings.Current.Device.SampleRate;

            _adapter.SetReadHandler((sender, buffer, length, discard) =>
            {
                var value = Math.Abs(buffer[AppSettings.Current.Device.PrimaryInputChannel - 1]);
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
                buffer[AppSettings.Current.Device.PrimaryOutputChannel - 1] = _generator.Next();
                return buffer.Length;
            });

        }

        public void Test()
        {
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
