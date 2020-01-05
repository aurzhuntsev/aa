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
    public enum AutoTuneMode
    {
        Amplitude, Power
    }

    [Measurement("Total Harmonic Distortion")]
    public class ThdMeasurement : SpectrumMeasurement
    {
        public class ThdSignalOptions
        {
            public double Frequency { get; set; } = 100500;
            public double Amplitude { get; set; } = -3.0;

            public AutoTuneMode TargetMode { get; set; }
            public double TargetValue { get; set; }
        }

        public ThdSignalOptions SignalOptions { get; set; } = new ThdSignalOptions();
        public ThdSignalOptions WarmUpSignalOptions { get; set; } = new ThdSignalOptions();

        public bool OverrideWarmUpOptions { get; set; }

        public int HarmonicWindowSize { get; set; }

        protected override IGenerator GetGenerator()
        {
            return null;
        }
    }
}
