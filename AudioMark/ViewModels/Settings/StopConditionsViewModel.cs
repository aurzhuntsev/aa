using AudioMark.Common;
using AudioMark.Core.Measurements.Settings.Common;
using AudioMark.Core.Settings;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.ViewModels.Settings
{
    public class StopConditionsViewModel : ViewModelBase
    {
        private StopConditions _model;

        public bool StopOnTimeoutEnabled
        {
            get => _model.TimeoutEnabled;
            set => this.RaiseAndSetIfPropertyChanged(() => _model.TimeoutEnabled, value, nameof(StopOnTimeoutEnabled));
        }

        public int StopOnTimeout
        {
            get => _model.Timeout;
            set => this.RaiseAndSetIfPropertyChanged(() => _model.Timeout, value, nameof(StopOnTimeout));
        }

        public bool StopOnToleranceEnabled
        {
            get => _model.ToleranceMatchingEnabled;
            set => this.RaiseAndSetIfPropertyChanged(() => _model.ToleranceMatchingEnabled, value, nameof(StopOnToleranceEnabled));
        }

        public double StopOnTolerance
        {
            get => _model.Tolerance * 100.0;
            set
            {
                _model.Tolerance = value / 100.0;
                this.RaisePropertyChanged(nameof(StopOnTolerance));
            }
        }

        public double StopOnConfidence
        {
            get => _model.Confidence * 100.0;
            set
            {
                _model.Confidence = value / 100.0;
                this.RaisePropertyChanged(nameof(StopOnConfidence));
            }
        }

        public StopConditionsViewModel(StopConditions model)
        {
            _model = model;
        }

    }
}
