using AudioMark.Common;
using AudioMark.Core.AudioData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace AudioMark.ViewModels.Settings
{
    public class DeviceSettingsViewModel : ViewModelBase
    {
        private List<DeviceInfo> _devices;
        private DeviceInfo _deviceSetting;
        private int _sampleRate;

        private Subject<DeviceSettingsViewModel> _whenChanged = new Subject<DeviceSettingsViewModel>();
        public IObservable<DeviceSettingsViewModel> WhenChanged
        {
            get => _whenChanged.AsObservable();
        }

        public string DeviceTitle { get; }

        public ObservableCollection<string> DevicesList { get; set; }
        private string _device;
        public string Device
        {
            get => _device;
            set
            {
                this.RaiseAndSetIfChanged(ref _device, value);

                FormatsList = new ObservableCollection<string>(
                    _devices.Where(device => device.Name == value)
                    .Select(device => device.SampleFormat.ToString())
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList()
                );

                this.RaisePropertyChanged(nameof(FormatsList));
                FormatsList.SetOrFirst(x => Format = x, _deviceSetting.SampleFormat.ToString());
                
                _whenChanged.OnNext(this);
            }
        }

        public ObservableCollection<string> FormatsList { get; set; }
        private string _format;
        public string Format
        {
            get => _format;
            set
            {
                this.RaiseAndSetIfChanged(ref _format, value);                
                _whenChanged.OnNext(this);
            }
        }

        private int _minLatency;
        public int MinLatency
        {
            get => _minLatency;
            set => this.RaiseAndSetIfChanged(ref _minLatency, value);
        }

        private int _latency;
        public int Latency
        {
            get => _latency;
            set
            {                
                this.RaiseAndSetIfChanged(ref _latency, value);
            }
        }

        public DeviceSettingsViewModel(string title, IEnumerable<DeviceInfo> devices, DeviceInfo deviceSetting)
        {
            DeviceTitle = title;

            _devices = devices.ToList();
            _deviceSetting = deviceSetting;

            DevicesList = new ObservableCollection<string>(devices.Select(device => device.Name).Distinct());
            DevicesList.SetOrFirst(v => Device = v, _deviceSetting.Name);
        }

        public void SetSampleRate(int sampleRate)
        {
            _sampleRate = sampleRate;

            var device = _devices
                .Where(d => d.Name == Device && d.SampleFormat.ToString() == Format)
                .FirstOrDefault();
            MinLatency = device.LatencyMilliseconds;
            Latency = MinLatency;
        }
    }
}
