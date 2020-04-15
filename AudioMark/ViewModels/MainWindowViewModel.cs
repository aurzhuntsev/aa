using AudioMark.Core.Common;
using AudioMark.Core.Measurements;
using AudioMark.Views.GraphView;
using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioMark.Core.Measurements.Common;
using AudioMark.Core.Fft;

namespace AudioMark.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MeasurementsPanelViewModel Measurements { get; }
        public SessionPanelViewModel Session { get; set; }
        public TopPanelViewModel TopPanel { get; set; }
        public SettingsPanelViewModel Settings { get; set; }

        private bool _measurementsPanelVisible;
        public bool MeasurementsPanelVisible
        {
            get => _measurementsPanelVisible;
            set => this.RaiseAndSetIfChanged(ref _measurementsPanelVisible, value);
        }        

        private bool _settingsPanelVisible;
        public bool SettingsPanelVisible
        {
            get => _settingsPanelVisible;
            set => this.RaiseAndSetIfChanged(ref _settingsPanelVisible, value);
        }

        private bool _sessionPanelVisible;
        public bool SessionPanelVisible
        {
            get => _sessionPanelVisible;
            set => this.RaiseAndSetIfChanged(ref _sessionPanelVisible, value);
        }

        private List<Series> _series = new List<Series>();
        public List<Series> Series
        {
            get => _series;
            set => this.RaiseAndSetIfChanged(ref _series, value);
        }

        private Series _activeSeries = null;
        public Series ActiveSeries
        {
            get => _activeSeries;
            set => this.RaiseAndSetIfChanged(ref _activeSeries, value);
        }

        private bool _dynamicRender;
        public bool DynamicRender
        {
            get => _dynamicRender;
            set => this.RaiseAndSetIfChanged(ref _dynamicRender, value);
        }

        private bool _measurementRunning;
        public bool MeasurementRunning
        {
            get => _measurementRunning;
            set => this.RaiseAndSetIfChanged(ref _measurementRunning, value);
        }

        private List<IMeasurement> _storedMeasurements = new List<IMeasurement>();

        public MainWindowViewModel()
        {
            TopPanel = new TopPanelViewModel();
            Settings = new SettingsPanelViewModel();

            Measurements = new MeasurementsPanelViewModel();
            Measurements.WhenRunningStatusChanged.Subscribe(success =>
            {
                MeasurementRunning = Measurements.Running;
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
                        Series.Add(new Series()
                        {
                            Data = Measurements.Measurement.Result,
                            ColorIndex = _storedMeasurements.Count - 1,
                            Visible = true
                        });                        
                        Dispatcher.UIThread.Post(() =>
                        {
                            Session.AddMeasurement(Measurements.Measurement, _storedMeasurements.Count - 1);
                        });

                        Measurements.SetCompletedMeasurement(Measurements.Measurement);
                        SessionPanelVisible = true;
                    }

                    ActiveSeries = null;
                    UpdateGraphView(null);
                }
            });

            Measurements.WhenDataUpdated.Subscribe(data =>
            {
                UpdateGraphView(data);
                this.RaisePropertyChanged(nameof(Series));
            });

            Measurements.WhenSelectionCancelled.Subscribe(_ =>
            {
                foreach (var item in Session.Items)
                {
                    item.Selected = false;
                }
            });

            Measurements.WhenAnalysisOptionsChanged.Subscribe(_ =>
            {
                UpdateGraphView(null);
                Session.UpdateMeasurement(Measurements.Measurement);
            });

            Session = new SessionPanelViewModel();
            Session.WhenSessionItemAdded.Subscribe((item) =>
            {
                Series.Add(new Series()
                {
                    Data = item.Measurement.Result,
                    ColorIndex = _storedMeasurements.Count,
                    Visible = true
                });

                _storedMeasurements.Add(item.Measurement);
                UpdateGraphView(null);
            });

            Session.WhenSessionItemRemoved.Subscribe((item) =>
            {
                if (item.Selected)
                {
                    Measurements.SetCompletedMeasurement(null);
                }

                _storedMeasurements.Remove(item.Measurement);
                Series.Remove(Series.FirstOrDefault(s => s.ColorIndex == item.SeriesIndex));

                UpdateGraphView(null);
            });

            Session.WhenSessionItemVisibilityChanged.Subscribe((item) =>
            {
                var series = Series.FirstOrDefault(s => s.ColorIndex == item.SeriesIndex);
                if (series != null)
                {
                    series.Visible = item.Visible;
                    UpdateGraphView(null);
                }
            });

            Session.WhenSessionItemSelectionChanged.Subscribe((item) =>
            {
                if (item.Selected)
                {
                    Measurements.SetCompletedMeasurement(item.Measurement);

                    HideAllPanels();
                    MeasurementsPanelVisible = true;
                }
                else
                {
                    Measurements.SetCompletedMeasurement(null);
                }
            });
        }

        private void UpdateGraphView(Spectrum data)
        {
            var storedDatas = _storedMeasurements.Cast<IMeasurement>().Select(m => m.Result);
            Series.RemoveAll(s => !storedDatas.Contains(s.Data));

            if (data != null)
            {
                ActiveSeries =
                     new Series()
                     {
                         Data = data,
                         ColorIndex = storedDatas.Count(),
                         Visible = true
                     };
                this.RaisePropertyChanged(nameof(ActiveSeries));
            }
            else
            {
                this.RaisePropertyChanged(nameof(Series));
            }
        }

        private void HideAllPanels()
        {
            MeasurementsPanelVisible = false;
            Measurements.Reset();

            SettingsPanelVisible = false;
        }


        public void ToggleMeasurements()
        {
            var initialValue = MeasurementsPanelVisible;

            HideAllPanels();
            MeasurementsPanelVisible = !initialValue;
        }

        public void ToggleSettings()
        {
            var initialValue = SettingsPanelVisible;

            HideAllPanels();
            SettingsPanelVisible = !initialValue;
        }

        public void ToggleSession()
        {
            SessionPanelVisible = !SessionPanelVisible;
        }
    }
}
