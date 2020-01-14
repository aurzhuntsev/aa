using AudioMark.Core.Common;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioMark.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MeasurementsPanelViewModel Measurements { get; }
        public CurrentMeasurementViewModel CurrentMeasurement { get; }

        private bool _measurementsPanelVisible;
        public bool MeasurementsPanelVisible 
        {
            get => _measurementsPanelVisible;
            set => this.RaiseAndSetIfChanged(ref _measurementsPanelVisible, value);
        }

        private SpectralData _data;
        public SpectralData Data
        {
            get => _data;
            set => this.RaiseAndSetIfChanged(ref _data, value);
        }

        private bool _dynamicRender;
        public bool DynamicRender
        {
            get => _dynamicRender;
            set => this.RaiseAndSetIfChanged(ref _dynamicRender, value);
        }

        public MainWindowViewModel()
        {
            Measurements = new MeasurementsPanelViewModel((data) =>
            {
                Data = data;
                this.RaisePropertyChanged(nameof(Data));
            });

            CurrentMeasurement = new CurrentMeasurementViewModel();            

            this.WhenAnyValue(_ => _.Measurements.Running)
                .Subscribe(running =>
                {
                    if (Measurements.Content != null)
                    {
                        if (running)
                        {
                            CurrentMeasurement.StartMonitoring(Measurements.Content.Measurement);
                            DynamicRender = true;
                        }
                        else
                        {
                            CurrentMeasurement.StopMonitoring();
                            DynamicRender = false;
                        }
                    }
                });
        }

        public void ToggleMeasurements() => MeasurementsPanelVisible = !MeasurementsPanelVisible;
    }
}
