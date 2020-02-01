using AudioMark.Common;
using AudioMark.Core.AudioData;
using AudioMark.Core.Measurements;
using AudioMark.Core.Settings;
using AudioMark.ViewModels.Settings;
using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using AudioMark.Core.Measurements.Common;

namespace AudioMark.ViewModels
{
    public class SettingsPanelViewModel : ViewModelBase
    {
        public DevicesSettingsViewModel Devices { get; }
        public FftSettingsViewModel Fft { get; }

        public SettingsPanelViewModel()
        {
            Devices = new DevicesSettingsViewModel();
            Fft = new FftSettingsViewModel();

            Devices.WhenSettingsUpdated.Subscribe(_ => Fft.UpdateWindowSizesList());
        }

        public void Save()
        {
            AppSettings.Current.Save();
        }

        public void Reset()
        {         
            AppSettings.Current.Reset();
            Devices.Reset();
            Fft.Reset();

            this.RaisePropertyChanged(nameof(Devices));
            this.RaisePropertyChanged(nameof(Fft));
        }

    }
}
