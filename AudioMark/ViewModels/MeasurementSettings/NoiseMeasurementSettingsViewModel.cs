using AudioMark.Common;
using AudioMark.Core.Common;
using AudioMark.Core.Measurements;
using AudioMark.Core.Measurements.Settings;
using AudioMark.ViewModels.Common;
using AudioMark.ViewModels.MeasurementSettings.Common;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using AudioMark.Core.Measurements.Common;

namespace AudioMark.ViewModels.MeasurementSettings
{
    [DefaultViewModel(typeof(NoiseMeasurementSettings))]
    public class NoiseMeasurementSettingsViewModel : MeasurementSettingsViewModelBase
    {
        public NoiseMeasurementSettings Model
        {
            get => (NoiseMeasurementSettings)Settings;
        }

        private bool _isCompleted;
        public override bool IsCompleted
        {
            get => _isCompleted;
            set => this.RaiseAndSetIfChanged(ref _isCompleted, value);
        }

        private IMeasurement _measurement;
        public override IMeasurement Measurement
        {
            get => _measurement;
            set
            {
                _measurement = value;
                CorrectionProfile.Target = _measurement.Result;
            }
        }

        public bool GenerateDummySignal
        {
            get => Model.GenerateDummySignal;
            set => this.RaiseAndSetIfPropertyChanged(() => Model.GenerateDummySignal, value);
        }

        public double DummySignalFrequency
        {
            get => Model.DummySignalOptions.Frequency;
            set => this.RaiseAndSetIfPropertyChanged(() => Model.DummySignalOptions.Frequency, value, nameof(DummySignalFrequency));
        }

        public double DummySignalLevel
        {
            get => Model.DummySignalOptions.InputOutputOptions.OutputLevel;
            set => this.RaiseAndSetIfPropertyChanged(() => Model.DummySignalOptions.InputOutputOptions.OutputLevel, value, nameof(DummySignalLevel));
        }

        public bool LimitHighFrequency
        {
            get => Model.LimitHighFrequency;
            set
            {
                this.RaiseAndSetIfPropertyChanged(() => Model.LimitHighFrequency, value);
                _whenAnalysisOptionsChanged.OnNext(this);
            }
        }

        public double HighFrequency
        {
            get => Model.HighFrequency;
            set
            {
                this.RaiseAndSetIfPropertyChanged(() => Model.HighFrequency, value);
                _whenAnalysisOptionsChanged.OnNext(this);
            }
        }

        public CorrectionProfileViewModel CorrectionProfile { get; }
        public StopConditionsViewModel StopConditions { get; }

        public NoiseMeasurementSettingsViewModel(NoiseMeasurementSettings settings) : base()
        {
            Settings = settings;

            CorrectionProfile = new CorrectionProfileViewModel(Model);
            CorrectionProfile.WhenChanged.Subscribe(_ => _whenAnalysisOptionsChanged.OnNext(this));

            StopConditions = new StopConditionsViewModel(Model);
        }
    }
}
