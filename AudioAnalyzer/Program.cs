
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
            


            Console.ReadKey();
        }
    }
}
