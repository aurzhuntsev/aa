﻿using AudioMark.Core.Measurements;
using Avalonia;
using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Timers;

namespace AudioMark.ViewModels
{
    public class CurrentMeasurementViewModel : ViewModelBase
    {
        private readonly TimeSpan MonitorInterval = new TimeSpan(0, 0, 1);

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

        private MeasurementBase _measurement;
        private DispatcherTimer _timer;
        public void StartMonitoring(MeasurementBase measurement)
        {
            if (measurement != null)
            {
                _measurement = measurement;
                
                if (_timer != null)
                {
                    _timer.Stop();
                }

                _timer = new DispatcherTimer(MonitorInterval, DispatcherPriority.Normal, (s, e) =>
                {
                    UpdateViewModel();
                });                

                UpdateViewModel();
                _timer.Start();
            }
        }

        public void StopMonitoring()
        {
            if (_timer != null)
            {
                _timer.Stop();
                ResetViewModel();
            }
        }

        private void ResetViewModel()
        {
            Title = string.Empty;
            StepTitle = string.Empty;
            StepNumber = string.Empty;
            Remaining = string.Empty;
            Elapsed = string.Empty;
        }

        private void UpdateViewModel()
        {
            try
            {
                Title = _measurement.Title;

                var currentStepNumber = _measurement.Activities.IndexOf(_measurement.CurrentActivity) + 1;
                var totalSteps = _measurement.Activities.Count;

                StepTitle = _measurement.CurrentActivity.Description;
                StepNumber = $"[{currentStepNumber}/{totalSteps}]";

                Remaining = _measurement.CurrentActivity.Remaining.ToString(@"hh\:mm\:ss");
                Elapsed = _measurement.CurrentActivity.Elapsed.ToString(@"hh\:mm\:ss");
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }
        }
    }
}
