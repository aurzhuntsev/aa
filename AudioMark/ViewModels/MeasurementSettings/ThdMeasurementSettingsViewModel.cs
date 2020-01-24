using AudioMark.Common;
using AudioMark.Core.Measurements;
using AudioMark.ViewModels.Common;
using AudioMark.ViewModels.MeasurementSettings.Common;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.ViewModels.MeasurementSettings
{
    [DefaultViewModel(typeof(ThdMeasurementSettings))]
    public class ThdMeasurementSettingsViewModel : MeasurementSettingsViewModelBase
    {
        public ThdMeasurementSettings Model
        {
            get => (ThdMeasurementSettings)Settings;
        }

        private bool _isCompleted;
        public override bool IsCompleted
        {
            get => _isCompleted;
            set => this.RaiseAndSetIfChanged(ref _isCompleted, value);
        }

        #region Test signal options
        public double TestSignalFrequency
        {
            get => Model.TestSignalOptions.Frequency;
            set => this.RaiseAndSetIfPropertyChanged(() => Model.TestSignalOptions.Frequency, value, nameof(TestSignalFrequency));
        }

        public InputOutputLevelViewModel TestSignalInputOutputLevelOptions
        {
            get => new InputOutputLevelViewModel(Model.TestSignalOptions.InputOutputOptions);
        }
        #endregion

        #region Warmup signal options
        public bool WarmUpEnabled
        {
            get => Model.WarmUpEnabled;
            set
            {
                this.RaiseAndSetIfPropertyChanged(() => Model.WarmUpEnabled, value);
                this.RaisePropertyChanged(nameof(WarmupSignalOptionsEnabled));
            }
        }

        public int WarmUpDurationSeconds
        {
            get => Model.WarmUpDurationSeconds;
            set => this.RaiseAndSetIfPropertyChanged(() => Model.WarmUpDurationSeconds, value);
        }

        public bool OverrideWarmUpSignalOptions
        {
            get => Model.OverrideWarmUpSignalOptions;
            set
            {
                this.RaiseAndSetIfPropertyChanged(() => Model.OverrideWarmUpSignalOptions, value);
                this.RaisePropertyChanged(nameof(WarmupSignalOptionsEnabled));
            }
        }

        public bool WarmupSignalOptionsEnabled
        {
            get => OverrideWarmUpSignalOptions && WarmUpEnabled;
        }

        public double WarmUpSignalFrequency
        {
            get => Model.WarmUpSignalOptions.Frequency;
            set => this.RaiseAndSetIfPropertyChanged(() => Model.WarmUpSignalOptions.Frequency, value, nameof(WarmUpSignalFrequency));
        }

        public InputOutputLevelViewModel WarmUpSignalInputOutputLevelOptions
        {
            get => new InputOutputLevelViewModel(Model.WarmUpSignalOptions.InputOutputOptions);
        }
        #endregion

        #region Stop conditions options

        public bool OverrideStopConditionsSettings
        {
            get => Model.StopConditions.Overriden;
            set => this.RaiseAndSetIfPropertyChanged(() => Model.StopConditions.Overriden, value, nameof(OverrideStopConditionsSettings));
        }

        public bool StopOnTimeoutEnabled
        {
            get => Model.StopConditions.Value.TimeoutEnabled;
            set => this.RaiseAndSetIfPropertyChanged(() => Model.StopConditions.Value.TimeoutEnabled, value, nameof(StopOnTimeoutEnabled));

        }

        public int StopOnTimeout
        {
            get => Model.StopConditions.Value.Timeout;
            set => this.RaiseAndSetIfPropertyChanged(() => Model.StopConditions.Value.Timeout, value, nameof(StopOnTimeout));
        }

        public bool StopOnToleranceEnabled
        {
            get => Model.StopConditions.Value.ToleranceMatchingEnabled;
            set => this.RaiseAndSetIfPropertyChanged(() => Model.StopConditions.Value.ToleranceMatchingEnabled, value, nameof(StopOnToleranceEnabled));
        }

        public double StopOnTolerance
        {
            get => Model.StopConditions.Value.Tolerance * 100.0;
            set
            {
                Model.StopConditions.Value.Tolerance = value / 100.0;
                this.RaisePropertyChanged(nameof(StopOnTolerance));
            }
        }

        public double StopOnConfidence
        {
            get => Model.StopConditions.Value.Confidence * 100.0;
            set
            {
                Model.StopConditions.Value.Confidence = value / 100.0;
                this.RaisePropertyChanged(nameof(StopOnConfidence));
            }
        }

        public CorrectionProfileViewModel CorrectionProfile { get; } = new CorrectionProfileViewModel();

        #endregion

        public ThdMeasurementSettingsViewModel()
        {

        }

        public ThdMeasurementSettingsViewModel(ThdMeasurementSettings settings) : base()
        {
            Settings = settings;
        }
    }
}
