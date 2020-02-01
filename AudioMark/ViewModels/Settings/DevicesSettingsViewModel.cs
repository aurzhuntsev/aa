using AudioMark.Common;
using AudioMark.Core.AudioData;
using AudioMark.Core.Measurements.Common;
using AudioMark.Core.Settings;
using Avalonia.Threading;
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
    public class DevicesSettingsViewModel : ViewModelBase
    {
        private const double TimeToTestSeconds = 3.0;

        private IAudioDataAdapter _adapter;
        private IEnumerable<DeviceInfo> _inputDevices;
        private IEnumerable<DeviceInfo> _outputDevices;

        private Tuner _tuner;
        private DispatcherTimer _stopTimer;

        private Subject<DevicesSettingsViewModel> _whenSettingsUpdated = new Subject<DevicesSettingsViewModel>();
        public IObservable<DevicesSettingsViewModel> WhenSettingsUpdated
        {
            get => _whenSettingsUpdated.AsObservable();
        }

        public ObservableCollection<string> ApisList { get; }
        private string _api;
        public string Api
        {
            get => _api;
            set
            {
                this.RaiseAndSetIfChanged(ref _api, value);
                AppSettings.Current.Device.Api = value;


                InputDeviceSettings = new DeviceSettingsViewModel("Input", _inputDevices.Where(device => device.ApiName == _api), AppSettings.Current.Device.InputDevice);
                InputDeviceSettings.WhenChanged.Subscribe(_ => UpdateSampleRatesList());
                this.RaisePropertyChanged(nameof(InputDeviceSettings));
                if (!string.IsNullOrEmpty(SampleRate))
                {
                    InputDeviceSettings.SetSampleRate(int.Parse(SampleRate));
                }

                OutputDeviceSettings = new DeviceSettingsViewModel("Output", _outputDevices.Where(device => device.ApiName == _api), AppSettings.Current.Device.OutputDevice);
                OutputDeviceSettings.WhenChanged.Subscribe(_ => UpdateSampleRatesList());
                this.RaisePropertyChanged(nameof(OutputDeviceSettings));
                if (!string.IsNullOrEmpty(SampleRate))
                {
                    OutputDeviceSettings.SetSampleRate(int.Parse(SampleRate));
                }

                UpdateSampleRatesList();
            }
        }

        public DeviceSettingsViewModel InputDeviceSettings { get; private set; }
        public DeviceSettingsViewModel OutputDeviceSettings { get; private set; }

        private int _inputChannel;
        public int InputChannel
        {
            get => _inputChannel;
            set
            {
                this.RaiseAndSetIfChanged(ref _inputChannel, value);
                AppSettings.Current.Device.PrimaryInputChannel = value + 1;
            }
        }


        private int _outputChannel;
        public int OutputChannel
        {
            get => _outputChannel;
            set
            {
                this.RaiseAndSetIfChanged(ref _outputChannel, value);
                AppSettings.Current.Device.PrimaryOutputChannel = value + 1;
            }
        }

        public ObservableCollection<string> InputChannelsList { get; set; }
        public ObservableCollection<string> OutputChannelsList { get; set; }

        public ObservableCollection<string> SampleRatesList { get; set; } = new ObservableCollection<string>();
        private string _sampleRate;
        public string SampleRate
        {
            get => _sampleRate;
            set
            {
                this.RaiseAndSetIfChanged(ref _sampleRate, value);
                UpdateSettings();
            }
        }

        private bool _hasSupportedSampleRates;
        public bool HasSupportedSampleRates
        {
            get => _hasSupportedSampleRates;
            set => this.RaiseAndSetIfChanged(ref _hasSupportedSampleRates, value);
        }

        private double _levelDbTp;
        public double LevelDbTp
        {
            get => _levelDbTp;
            set => this.RaiseAndSetIfChanged(ref _levelDbTp, value);
        }

        private double _levelDbFs;
        public double LevelDbFs
        {
            get => _levelDbFs;
            set => this.RaiseAndSetIfChanged(ref _levelDbFs, value);
        }

        private bool _isTestActive;
        public bool IsTestActive
        {
            get => _isTestActive;
            set => this.RaiseAndSetIfChanged(ref _isTestActive, value);
        }

        public DevicesSettingsViewModel()
        {
            _adapter = AudioDataAdapterProvider.Get();
            _tuner = new Tuner();

            _inputDevices = _adapter.EnumerateInputDevices();
            _outputDevices = _adapter.EnumerateOutputDevices();

            ApisList = new ObservableCollection<string>(
                _adapter.EnumerateSystemApis()
            );

            ApisList.SetOrFirst(v => Api = v, AppSettings.Current.Device.Api);

            _tuner.OutputLevel = -AppSettings.Current.Device.ClippingLevel;
            _tuner.OnReading += (s, r) =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    LevelDbFs = r.InputLevelDbFs;
                    LevelDbTp = r.InputLevelDbTp;
                });
            };

            _tuner.OnError += async (s, e) =>
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await Interactions.Error.Handle(e);
                });
            };
        }

        private void UpdateSampleRatesList()
        {
            var inputs = _inputDevices.Where(device => device.ApiName == Api && device.Name == InputDeviceSettings.Device
                         && device.SampleFormat == (SampleFormat)Enum.Parse(typeof(SampleFormat), InputDeviceSettings.Format))
                        .ToList();
            var outputs = _outputDevices.Where(device => device.ApiName == Api && device.Name == OutputDeviceSettings.Device
                         && device.SampleFormat == (SampleFormat)Enum.Parse(typeof(SampleFormat), OutputDeviceSettings.Format))
                        .ToList();

            var sampleRates = inputs.Select(device => device.SampleRate)
                                    .Where(rate => outputs.Any(device => device.SampleRate == rate))
                                    .OrderBy(x => x)
                                    .Select(rate => rate.ToString())
                                    .Distinct()
                                    .ToList();
            if (sampleRates.Any())
            {
                SampleRatesList = new ObservableCollection<string>(sampleRates);
                HasSupportedSampleRates = true;
            }
            else
            {
                SampleRatesList = null;
                HasSupportedSampleRates = false;
            }

            this.RaisePropertyChanged(nameof(SampleRatesList));
            SampleRatesList.SetOrFirst(v => SampleRate = v, AppSettings.Current.Device.SampleRate.ToString());

            UpdateSettings();
        }

        private void UpdateSettings()
        {
            if (!HasSupportedSampleRates)
            {
                return;
            }


            var inputDevice = _inputDevices
                .Where(d => d.ApiName == Api && d.Name == InputDeviceSettings.Device && d.SampleFormat.ToString() == InputDeviceSettings.Format)
                .FirstOrDefault();

            AppSettings.Current.Device.InputDevice.Index = inputDevice.Index;
            AppSettings.Current.Device.InputDevice.ChannelsCount = inputDevice.ChannelsCount;
            AppSettings.Current.Device.InputDevice.SampleFormat = inputDevice.SampleFormat;
            AppSettings.Current.Device.InputDevice.Name = inputDevice.Name;
            AppSettings.Current.Device.InputDevice.LatencyMilliseconds = InputDeviceSettings.Latency;
            AppSettings.Current.Device.InputDevice.SampleRate = int.Parse(SampleRate);

            InputDeviceSettings.SetSampleRate(int.Parse(SampleRate));

            InputChannelsList = new ObservableCollection<string>();
            for (var i = 0; i < inputDevice.ChannelsCount; i++)
            {
                InputChannelsList.Add($"Channel {i + 1}");
            }
            this.RaisePropertyChanged(nameof(InputChannelsList));

            if (AppSettings.Current.Device.PrimaryInputChannel - 1 < inputDevice.ChannelsCount)
            {
                InputChannel = AppSettings.Current.Device.PrimaryInputChannel - 1;
            }
            else
            {
                InputChannel = 0;
            }

            var outputDevice = _outputDevices
                .Where(d => d.ApiName == Api && d.Name == OutputDeviceSettings.Device && d.SampleFormat.ToString() == OutputDeviceSettings.Format)
                .FirstOrDefault();

            AppSettings.Current.Device.OutputDevice.Index = outputDevice.Index;
            AppSettings.Current.Device.OutputDevice.ChannelsCount = outputDevice.ChannelsCount;
            AppSettings.Current.Device.OutputDevice.SampleFormat = outputDevice.SampleFormat;
            AppSettings.Current.Device.OutputDevice.Name = outputDevice.Name;
            AppSettings.Current.Device.OutputDevice.LatencyMilliseconds = OutputDeviceSettings.Latency;
            AppSettings.Current.Device.OutputDevice.SampleRate = int.Parse(SampleRate);

            OutputDeviceSettings.SetSampleRate(int.Parse(SampleRate));

            OutputChannelsList = new ObservableCollection<string>();
            for (var i = 0; i < outputDevice.ChannelsCount; i++)
            {
                OutputChannelsList.Add($"Channel {i + 1}");
            }
            this.RaisePropertyChanged(nameof(OutputChannelsList));

            if (AppSettings.Current.Device.PrimaryOutputChannel - 1 < outputDevice.ChannelsCount)
            {
                OutputChannel = AppSettings.Current.Device.PrimaryOutputChannel - 1;
            }
            else
            {
                OutputChannel = 0;
            }

            AppSettings.Current.Device.SampleRate = int.Parse(SampleRate);

            _whenSettingsUpdated.OnNext(null);
        }

        public async void Test()
        {
            try
            {
                _tuner.Test();
                _stopTimer = new DispatcherTimer(TimeSpan.FromSeconds(TimeToTestSeconds), DispatcherPriority.Background, (s, e) =>
                {
                    Stop();
                });
                IsTestActive = true;

                _stopTimer.Start();
            }
            catch (Exception e)
            {
                await Interactions.Error.Handle(e);
            }
        }

        public void Stop()
        {
            IsTestActive = false;
            _stopTimer.Stop();
            _tuner.Stop();
        }

        public void Reset()
        {
            Api = AppSettings.Current.Device.Api;
            this.RaisePropertyChanged(nameof(Api));
        }
    }
}
