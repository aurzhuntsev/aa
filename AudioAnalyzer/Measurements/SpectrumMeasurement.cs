using AudioMark.Core.AudioData;
using AudioMark.Core.Common;
using AudioMark.Core.Generators;
using AudioMark.Core.Settings;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AudioMark.Core.Measurements
{
    public abstract class SpectrumMeasurement : MeasurementBase
    {
        public delegate void DataUpdate(SpectralData data);
        public DataUpdate OnDataUpdate { get; set; }

        public SpectralData Data { get; private set; }

        public Fft FftSettings { get; set; }
        public bool OverrideFftSetings { get; set; }

        public StopConditions StopConditions { get; set; }
        public bool OverrideStopConditions { get; set; }

        public IEnumerable<ActivityBase> Activities => throw new NotImplementedException();

        public ActivityBase CurrentActivity => throw new NotImplementedException();

        public string Title => throw new NotImplementedException();

        public void Run()
        {
            Initialize();

            Data = new SpectralData(AppSettings.Current.Fft.WindowSize, AppSettings.Current.Device.SampleRate);

            var adapter = AudioDataAdapterProvider.Get();

            adapter.OnWrite = new IAudioDataAdapter.DataWriteEventHandler((sender, buffer) =>
            {
                buffer[AppSettings.Current.Device.PrimaryOutputChannel - 1] = 0;// GetGenerator().Next();
                return buffer.Length;
            });

            var proc = new SpectralDataProcessor(0,0)
            {
                OnItemProcessed = (data) =>
                {
                    lock (Data)
                    {
                        Data.Add(data);                        
                    }

                    OnDataUpdate(Data);
                }
            };
            
            adapter.OnRead = new IAudioDataAdapter.DataReadEventHandler((sender, buffer, length) =>
            {
                proc.Add(buffer[AppSettings.Current.Device.PrimaryInputChannel - 1]);
            });

            adapter.Start();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
