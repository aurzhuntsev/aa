using AudioMark.Common;
using AudioMark.Core.Measurements.Settings.Common;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.ViewModels.MeasurementSettings.Common
{
    public class StopConditionsViewModel : ViewModelBase
    {
        private IGlobalOptions _model;

        public bool OverrideStopConditionsSettings
        {
            get => _model.StopConditions.Overriden;
            set => this.RaiseAndSetIfPropertyChanged(() => _model.StopConditions.Overriden, value, nameof(OverrideStopConditionsSettings));
        }

        public bool StopOnTimeoutEnabled
        {
            get => _model.StopConditions.Value.TimeoutEnabled;
            set => this.RaiseAndSetIfPropertyChanged(() => _model.StopConditions.Value.TimeoutEnabled, value, nameof(StopOnTimeoutEnabled));

        }

        public int StopOnTimeout
        {
            get => _model.StopConditions.Value.Timeout;
            set => this.RaiseAndSetIfPropertyChanged(() => _model.StopConditions.Value.Timeout, value, nameof(StopOnTimeout));
        }

        public bool StopOnToleranceEnabled
        {
            get => _model.StopConditions.Value.ToleranceMatchingEnabled;
            set => this.RaiseAndSetIfPropertyChanged(() => _model.StopConditions.Value.ToleranceMatchingEnabled, value, nameof(StopOnToleranceEnabled));
        }

        public double StopOnTolerance
        {
            get => _model.StopConditions.Value.Tolerance * 100.0;
            set
            {
                _model.StopConditions.Value.Tolerance = value / 100.0;
                this.RaisePropertyChanged(nameof(StopOnTolerance));
            }
        }

        public double StopOnConfidence
        {
            get => _model.StopConditions.Value.Confidence * 100.0;
            set
            {
                _model.StopConditions.Value.Confidence = value / 100.0;
                this.RaisePropertyChanged(nameof(StopOnConfidence));
            }
        }

        public StopConditionsViewModel(IGlobalOptions settings)
        {
            _model = settings;
        }

    }
}
