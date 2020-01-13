
using AudioMark.Core.AudioData;
using AudioMark.Core.Measurements;
using AudioMark.Core.Settings;
using Microsoft.Extensions.Configuration;
using PortAudioWrapper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AudioMark.Core
{
    class Program
    {
        static void Main(string[] args)
        {

            PortAudio.Initialize();
            //(new SpectrumMeasurement()).Run();
            //AppSettings.Current.Save();
            //return;

            AppSettings.Current.Save();
            Console.ReadKey();
            return;

            var istreamParameters = new PaStreamParameters()
            {
                ChannelCount = 2,
                Device = PortAudio.Instance.GetDefaultInputDeviceIndex(),
                SampleFormat = PaSampleFormat.PaFloat32,
                SuggestedLatency = 0.1
            };


            var ostreamParameters = new PaStreamParameters()
            {
                ChannelCount = 2,
                Device = PortAudio.Instance.GetDefaultOutputDeviceIndex(),
                SampleFormat = PaSampleFormat.PaFloat32,
                SuggestedLatency = 0.1
            };

            var r = new Random();


            var table = new float[96000];
            var rec = new List<float>();
            var x = 0;
            for (x = 0; x < 96000; x++)
            {
                table[x] = (float)(Math.Sin(x * (2 * Math.PI / 96)));
            }



            x = 0;
            Process.GetCurrentProcess().PriorityBoostEnabled = true;
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;

            int runs = 0;
            using (var stream = new PortAudioStream(istreamParameters, ostreamParameters, 96000, 0, 0,
                (i, o, time, status) =>
                {
                    
                    for (var c = 0; c < o.Length; c += 2)
                    {
                        var v = table[x];
                        o[c] = v;
                        o[c + 1] = v;

                        x++;
                        if (x == 96000)
                        {
                            x = 0;
                        }
                    }

                    if (runs > 10)
                    {
                        for (var c = 0; c < i.Length; c += 2)
                        {                            
                            //rec.Add(i[c+1]);
                        }
                    }

                    runs++;
                    
                    if (rec.Count >= 96000)
                    {
                        var arr = rec.ToArray();
                        MathNet.Numerics.IntegralTransforms.Fourier.ForwardReal(arr, arr.Length - 2, MathNet.Numerics.IntegralTransforms.FourierOptions.Default);

                        return PaStreamCallbackResult.PaAbort;
                    };

                    return PaStreamCallbackResult.PaContinue;
                }))
            {
                stream.Start();
            }



            Console.ReadKey();
        }
    }
}
