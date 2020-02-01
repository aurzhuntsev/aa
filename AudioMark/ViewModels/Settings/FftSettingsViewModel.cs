using AudioMark.Common;
using AudioMark.Core.Settings;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace AudioMark.ViewModels.Settings
{
    public class FftSettingsViewModel : ViewModelBase
    {
        const double LowWindowSizePowMultiplier = (1.0 / 8.0);
        const double HighWindowSizePowMultiplier = 2.0;

        public ObservableCollection<int> WindowSizesList { get; private set; }
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
        }

        public void UpdateWindowSizesList()
        {
            WindowSizesList = new ObservableCollection<int>();
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
