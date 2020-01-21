using AudioMark.Core.Common;
using AudioMark.Core.Measurements;
using Avalonia.Threading;
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
        public SessionPanelViewModel Session { get; set; }
        public TopPanelViewModel TopPanel { get; set; }

        private bool _measurementsPanelVisible;
        public bool MeasurementsPanelVisible
        {
            get => _measurementsPanelVisible;
            set => this.RaiseAndSetIfChanged(ref _measurementsPanelVisible, value);
        }

        /* TODO: Make it better? */
        private int _sucessfulSeriesCount = 0;
        private List<SpectralData> _series = new List<SpectralData>();
        public List<SpectralData> Series
        {
            get => _series;
            set => this.RaiseAndSetIfChanged(ref _series, value);
        }

        private bool _dynamicRender;
        public bool DynamicRender
        {
            get => _dynamicRender;
            set => this.RaiseAndSetIfChanged(ref _dynamicRender, value);
        }

        private List<IMeasurement> _storedMeasurements = new List<IMeasurement>();

        public MainWindowViewModel()
        {
            TopPanel = new TopPanelViewModel();
            Measurements = new MeasurementsPanelViewModel();
            Measurements.WhenRunningStatusChanged.Subscribe(success =>
            {
                if (Measurements.Running)
                {
                    TopPanel.SetActiveMeasurement(Measurements.Measurement, Session.Items.Count);
                    DynamicRender = true;
                }
                else
                {
                    TopPanel.SetActiveMeasurement(null, 0);
                    DynamicRender = false;

                    if (success)
                    {
                        _storedMeasurements.Add(Measurements.Measurement);
                        Dispatcher.UIThread.Post(() =>
                        {
                            Session.AddMeasurement(Measurements.Measurement, _storedMeasurements.Count - 1);
                        });
                    }
                    
                    UpdateGraphView(null);
                }
            });

            Measurements.WhenDataUpdated.Subscribe(data =>
            {
                /* TODO: Optimize */
                UpdateGraphView(data);                
                
                this.RaisePropertyChanged(nameof(Series));
            });

            Session = new SessionPanelViewModel();
        }

        private void UpdateGraphView(SpectralData data)
        {
            var storedDatas = _storedMeasurements.Cast<IMeasurement<SpectralData>>().Select(m => m.Data);
            Series.RemoveAll(s => !storedDatas.Contains(s));

            if (data != null)
            {
                Series.Add(data);
            }

            this.RaisePropertyChanged(nameof(Series));
        }

        public void ToggleMeasurements() => MeasurementsPanelVisible = !MeasurementsPanelVisible;
    }
}
