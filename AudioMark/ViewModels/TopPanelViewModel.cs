using AudioMark.Core.Measurements;
using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using AudioMark.Core.Measurements.Common;

namespace AudioMark.ViewModels
{
    public class TopPanelViewModel : ViewModelBase
    {
        private const int TimerIntervalMilliseconds = 250;

        private DispatcherTimer _timer;

        private IMeasurement _activeMeasurement;
        public IMeasurement ActiveMeasurement
        {
            get => _activeMeasurement;
            private set
            {
                this.RaiseAndSetIfChanged(ref _activeMeasurement, value);
                this.RaisePropertyChanged(nameof(HasActiveMeasurement));
            }
        }

        public bool HasActiveMeasurement
        {
            get => _activeMeasurement != null;
        }
        
        public int SeriesIndex
        {
            get; private set;
        }

        public string Name
        {
            get => _activeMeasurement?.Name;
        }

        public string CurrentActivity
        {
            get => $"{_activeMeasurement?.CurrentActivityDescription} [{_activeMeasurement?.CurrentActivityIndex + 1}/{_activeMeasurement.ActivitiesCount}]";
        }

        public string Remaining
        {
            get => _activeMeasurement != null && _activeMeasurement.Remaining.HasValue ? _activeMeasurement.Remaining.Value.ToString(@"hh\:mm\:ss") : "<not estimated>";
        }

        public string Elapsed
        {
            get => _activeMeasurement != null ? _activeMeasurement.Elapsed.ToString(@"hh\:mm\:ss") : string.Empty;
        }

        private static readonly string[] _tickerChars = new[] { "/", "–", @"\", "|" };
        private int _runningTickerIndex = 1;
        private string _runningTicker = _tickerChars[0];
        public string RunningTicker
        {
            get => _runningTicker;
            set => this.RaiseAndSetIfChanged(ref _runningTicker, value);
        }

        public TopPanelViewModel()
        {
        }

        public void SetActiveMeasurement(IMeasurement measurement, int seriesIndex)
        {
            if (measurement != null)
            {
                ActiveMeasurement = measurement;
                SeriesIndex = seriesIndex;

                if (_timer != null && _timer.IsEnabled)
                {
                    _timer.Stop();
                }

                RaisePropertiesChanged();
                _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(TimerIntervalMilliseconds), DispatcherPriority.Background, (s, e) =>
                {
                    RunningTicker = _tickerChars[_runningTickerIndex];
                    _runningTickerIndex++;
                    if (_runningTickerIndex == _tickerChars.Length)
                    {
                        _runningTickerIndex = 0;                        
                        RaisePropertiesChanged();
                    }
                });

                _timer.Start();
            }
            else
            {
                ActiveMeasurement = null;
                _timer.Stop();
                RaisePropertiesChanged();
            }
        }

        private void RaisePropertiesChanged()
        {
            this.RaisePropertyChanged(nameof(SeriesIndex));
            this.RaisePropertyChanged(nameof(Name));            
            this.RaisePropertyChanged(nameof(CurrentActivity));
            this.RaisePropertyChanged(nameof(Remaining));
            this.RaisePropertyChanged(nameof(Elapsed));
        }
    }
}
