using AudioMark.Core.Measurements;
using AudioMark.Core.Measurements.Settings.Common;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using AudioMark.Core.Measurements.Common;

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

        public abstract IMeasurement Measurement { get; set; }
        public abstract bool IsCompleted { get; set; }
    }
}
