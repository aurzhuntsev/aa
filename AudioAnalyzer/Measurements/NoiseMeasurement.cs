using AudioMark.Core.Common;
using AudioMark.Core.Generators;
using PortAudioWrapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AudioMark.Core.Settings;
using AudioMark.Core.AudioData;
using System.Diagnostics;

namespace AudioMark.Core.Measurements
{    
    [Measurement("Noise")]
    public class NoiseMeasurement : SpectrumMeasurement
    {
        public class ThdSignalOptions
        {
            public double Frequency { get; set; } = 100500;
            public double Amplitude { get; set; } = -3.0;

            public double TargetValue { get; set; }
        }

        public ThdSignalOptions SignalOptions { get; set; } = new ThdSignalOptions();
        public ThdSignalOptions WarmUpSignalOptions { get; set; } = new ThdSignalOptions();

        public bool OverrideWarmUpOptions { get; set; }

        public int HarmonicWindowSize { get; set; }

        protected override void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}
