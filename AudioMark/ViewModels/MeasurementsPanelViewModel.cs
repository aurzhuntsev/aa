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
using AudioMark.Core.Measurements.Common;

namespace AudioMark.ViewModels
{
    public class MeasurementsPanelViewModel : ViewModelBase
    {
        private IMeasurement _measurement;
        public IMeasurement Measurement
        {
            get => _measurement;
            set => this.RaiseAndSetIfChanged(ref _measurement, value);
        }

        private bool _isCompleted;
        public bool IsCompleted
        {
            get => _isCompleted;
            set => this.RaiseAndSetIfChanged(ref _isCompleted, value);
        }

        private Subject<bool> _whenRunningStatusChanged = new Subject<bool>();
        public IObservable<bool> WhenRunningStatusChanged
        {
            get => _whenRunningStatusChanged.AsObservable();
        }

        private Subject<Spectrum> _whenDataUpdated = new Subject<Spectrum>();
        public IObservable<Spectrum> WhenDataUpdated
        {
            get => _whenDataUpdated.AsObservable();
        }

        private Subject<Unit> _whenSelectionCancelled = new Subject<Unit>();
        public IObservable<Unit> WhenSelectionCancelled
        {
            get => _whenSelectionCancelled.AsObservable();
        }

        private Subject<Unit> _whenAnalysisOptionsChanged = new Subject<Unit>();
        public IObservable<Unit> WhenAnalysisOptionsChanged
        {
            get => _whenAnalysisOptionsChanged.AsObservable();
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
        IDisposable _contentSubscription = null;

        public MeasurementsPanelViewModel()
        {
            Items = new ObservableCollection<string>(MeasurementsFactory.List().Select(item => item.Name));

            this.WhenAnyValue(x => x.SelectedIndex).Subscribe(x =>
            {
                SetSelectedMeasurement();
            });
        }

        private void SetSelectedMeasurement()
        {
            var settings = MeasurementsFactory.CreateSettings(Items[SelectedIndex]);
            var viewModel = DefaultForModel(settings);
            Content = (MeasurementSettingsViewModelBase)viewModel;

            SubscribeToContentEvents();
        }

        private void OnMeasurementDataUpdate(object sender, object e)
        {
        }

        private void SubscribeToContentEvents()
        {
            if (_contentSubscription != null)
            {
                _contentSubscription.Dispose();
            }

            _contentSubscription = _content.WhenAnalysisOptionsChanged.Subscribe((model) =>
            {
                if (IsCompleted)
                {
                    Measurement.Update();
                }

                _whenAnalysisOptionsChanged.OnNext(Unit.Default);
            });
        }

        public void Reset()
        {
            if (!IsCompleted)
            {
                SetSelectedMeasurement();
            }
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
                    _whenDataUpdated.OnNext(data as Spectrum);
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

        public void SetCompletedMeasurement(IMeasurement measurement)
        {
            Measurement = measurement;
            if (measurement != null)
            {
                _content = (MeasurementSettingsViewModelBase)DefaultForModel(measurement.Settings);
                _content.IsCompleted = true;
                IsCompleted = true;

                var selectedItemName = MeasurementsFactory.List().Where(item => item.Type == measurement.GetType()).FirstOrDefault().Name;
                SelectedIndex = Items.IndexOf(selectedItemName);

                SubscribeToContentEvents();
            }
            else
            {
                SetSelectedMeasurement();
                IsCompleted = false;
            }

            this.RaisePropertyChanged(nameof(Content));
        }

        public void CancelSelection()
        {
            SetCompletedMeasurement(null);
            _whenSelectionCancelled.OnNext(Unit.Default);
        }
    }
}
