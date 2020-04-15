using AudioMark.Core.Measurements;
using AudioMark.Core.Measurements.Settings.Common;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using AudioMark.Core.Measurements.Common;
using AudioMark.ViewModels.MeasurementSettings.Common;
using AudioMark.Core.Measurements.Analysis;
using AudioMark.ViewModels.Settings;
using AudioMark.Common;

namespace AudioMark.ViewModels.MeasurementSettings
{
    public abstract class MeasurementSettingsViewModelBase : ViewModelBase
    {
        private IMeasurementSettings _settings;
        public IMeasurementSettings Settings
        {
            get => _settings;
            set => this.RaiseAndSetIfChanged(ref _settings, value);
        }

        protected Subject<MeasurementSettingsViewModelBase> _whenAnalysisOptionsChanged = new Subject<MeasurementSettingsViewModelBase>();
        public IObservable<MeasurementSettingsViewModelBase> WhenAnalysisOptionsChanged
        {
            get => _whenAnalysisOptionsChanged.AsObservable();
        }


        private bool _isCompleted;
        public bool IsCompleted
        {
            get => _isCompleted;
            set => this.RaiseAndSetIfChanged(ref _isCompleted, value);
        }

        private IMeasurement _measurement;
        public IMeasurement Measurement
        {
            get => _measurement;
            set
            {
                _measurement = value;
                CorrectionProfile.Target = _measurement.Result;
            }
        }

        public CorrectionProfileViewModel CorrectionProfile { get; }

        public bool OverrideStopConditions
        {
            get => ((IGlobalOptions)Settings).StopConditions.Overriden;
            set
            {
                this.RaiseAndSetIfPropertyChanged(() => ((IGlobalOptions)Settings).StopConditions.Overriden, value, nameof(OverrideStopConditions));
                this.RaisePropertyChanged(nameof(StopConditions));
            }
        }

        public StopConditionsViewModel StopConditions
        {
            get => new StopConditionsViewModel(((IGlobalOptions)Settings).StopConditions.Value);
        }

        public MeasurementSettingsViewModelBase(IMeasurementSettings model)
        {
            Settings = model;
            if (model is ICorrectionProfile correctable)
            {
                CorrectionProfile = new CorrectionProfileViewModel(correctable);
                CorrectionProfile.WhenChanged.Subscribe(_ => _whenAnalysisOptionsChanged.OnNext(this));
            }
        }

    }
}
