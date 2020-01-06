using AudioMark.Core.Measurements;
using Avalonia;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace AudioMark.ViewModels
{
    public class CurrentMeasurementViewModel : ViewModelBase
    {
        private string _title;
        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        private string _stepTitle;
        public string StepTitle
        {
            get => _stepTitle;
            set => this.RaiseAndSetIfChanged(ref _stepTitle, value);
        }

        private string _stepNumber;
        public string StepNumber
        {
            get => _stepNumber;
            set => this.RaiseAndSetIfChanged(ref _stepNumber, value);
        }

        private string _remaining;
        public string Remaining
        {
            get => _remaining;
            set => this.RaiseAndSetIfChanged(ref _remaining, value);
        }

        private string _elapsed;
        public string Elapsed
        {
            get => _elapsed;
            set => this.RaiseAndSetIfChanged(ref _elapsed, value);
        }

        private Rect _viewBounds;
        public Rect ViewBounds
        {
            get => _viewBounds;
            set => this.RaiseAndSetIfChanged(ref _viewBounds, value);
        }

        private int _progressWidth;
        public int ProgressWidth
        {
            get => _progressWidth;
            set => this.RaiseAndSetIfChanged(ref _progressWidth, value);
        }

        private IMeasurement _measurement;
        public CurrentMeasurementViewModel(IMeasurement measurement)
        {
            _measurement = measurement;            
        }
    }
}
