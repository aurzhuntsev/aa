using AudioMark.Core.Common;
using AudioMark.Core.Measurements;
using AudioMark.ViewModels.MeasurementSettings;
using AudioMark.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace AudioMark.ViewModels
{
    public class MeasurementsPanelViewModel : ViewModelBase
    {
        public IMeasurement Measurement { get; private set; }

        private Subject<bool> _whenRunningStatusChanged = new Subject<bool>();
        public IObservable<bool> WhenRunningStatusChanged
        {
            get => _whenRunningStatusChanged.AsObservable();
        }

        private Subject<SpectralData> _whenDataUpdated = new Subject<SpectralData>();
        public IObservable<SpectralData> WhenDataUpdated
        {
            get => _whenDataUpdated.AsObservable();
        }

        /* TODO: See how it will work content that needs scrolling */
        public ObservableCollection<string> Items { get; }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedIndex, value);
        }

        private bool _running;
        public bool Running
        {
            get => _running;
            set => this.RaiseAndSetIfChanged(ref _running, value);
        }

        private MeasurementSettingsViewModelBase _content = null;
        public MeasurementSettingsViewModelBase Content
        {
            get => _content;
            set => this.RaiseAndSetIfChanged(ref _content, value);
        }
  
        public MeasurementsPanelViewModel()
        {
            Items = new ObservableCollection<string>(MeasurementsFactory.List().Select(item => item.Name));
            
            this.WhenAnyValue(x => x.SelectedIndex).Subscribe(x =>
            {               
                var settings = MeasurementsFactory.CreateSettings(Items[SelectedIndex]);
                var viewModel = DefaultForModel(settings);
                _content = (MeasurementSettingsViewModelBase)viewModel;                
            });
        }

        private void OnMeasurementDataUpdate(object sender, object e)
        {         
        }

        public async void Run(Button sender)
        {
            if (!Running)
            {
                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    desktop.Exit += OnExit;
                }

                Measurement = MeasurementsFactory.Create(Items[SelectedIndex], Content.Settings);                
                Measurement.OnComplete += (sender, success) =>
                {
                    Running = false;
                    _whenRunningStatusChanged.OnNext(success);
                };

                Measurement.OnError += (sender, e) =>
                {
                    Running = false;
                    _whenRunningStatusChanged.OnNext(false);
                };

                Measurement.OnDataUpdate += (sender, data) =>
                {
                    _whenDataUpdated.OnNext(data as SpectralData);
                };

                Running = true;
                _whenRunningStatusChanged.OnNext(false);

                await Measurement.Run();
            }
            else
            {
                Measurement.Stop();

                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    desktop.Exit -= OnExit;
                }
            }
        }

        private void OnExit(object sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            if (Measurement != null && Measurement.Running)
            {
                Measurement.Stop();
            }
        }
    }
}
