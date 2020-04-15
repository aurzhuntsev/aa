using AudioMark.Common;
using AudioMark.Core.Common;
using AudioMark.Core.Settings;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AudioMark.ViewModels.Settings
{
    public class FftSettingsViewModel : ViewModelBase
    {
        const double LowWindowSizePowMultiplier = (1.0 / 8.0);
        const double HighWindowSizePowMultiplier = 4.0;

        public List<string> WindowFunctionsList { get; private set; }
        public string WindowFunction
        {
            get
            {
                return typeof(WindowFunctions).GetFields(BindingFlags.Public | BindingFlags.Static)
                    .First(f => f.Name == AppSettings.Current.Fft.WindowFunction.ToString())
                    .GetCustomAttributes(typeof(StringAttribute)).Cast<StringAttribute>().First().Value;
            }
            set
            {
                var field = typeof(WindowFunctions).GetFields(BindingFlags.Public | BindingFlags.Static)
                .First(f => (f.GetCustomAttributes(typeof(StringAttribute), false).First() as StringAttribute).Value == value);

                AppSettings.Current.Fft.WindowFunction = (WindowFunctions)field.GetValue(null);
            }
        }
       
        public List<int> WindowSizesList { get; private set; }
        public int WindowSize
        {
            get => AppSettings.Current.Fft.WindowSize;
            set
            {
                this.RaiseAndSetIfPropertyChanged(() => AppSettings.Current.Fft.WindowSize, value, nameof(WindowSize));
                this.RaisePropertyChanged(nameof(FrequencyResolution));
            }
        }

        public int WindowOverlapFactorPercentage
        {
            get => (int)(AppSettings.Current.Fft.WindowOverlapFactor * 100.0);
            set
            {
                var val = (double)value / 100.0;
                if (val < 0.0)
                {
                    val = 0.0;
                }
                else if (val > 1.0)
                {
                    val = 1.0;
                }

                this.RaiseAndSetIfPropertyChanged(() => AppSettings.Current.Fft.WindowOverlapFactor, val, nameof(WindowOverlapFactorPercentage));
            }
        }

        public double FrequencyResolution
        {
            get => AppSettings.Current.Device.SampleRate / (2.0 * WindowSize);
        }

        public FftSettingsViewModel()
        {
            UpdateWindowSizesList();
            
            WindowFunctionsList = typeof(WindowFunctions).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Select(f => (f.GetCustomAttributes(typeof(StringAttribute), false).First() as StringAttribute).Value)
                .ToList();

        }

        public void UpdateWindowSizesList()
        {
            WindowSizesList = new List<int>();
            for (var k = LowWindowSizePowMultiplier; k <= HighWindowSizePowMultiplier; k *= 2)
            {
                WindowSizesList.Add((int)(AppSettings.Current.Device.SampleRate * k));
            }
            
            this.RaisePropertyChanged(nameof(WindowSizesList));

            if (!WindowSizesList.Contains(AppSettings.Current.Fft.WindowSize))
            {
                WindowSize = (int)(AppSettings.Current.Device.SampleRate * 0.5);
            }
            else
            {
                this.RaisePropertyChanged(nameof(FrequencyResolution));
            }
        }

        public void Reset()
        {
            this.RaisePropertyChanged(nameof(WindowSize));
            this.RaisePropertyChanged(nameof(WindowOverlapFactorPercentage));
        }
    }
}
