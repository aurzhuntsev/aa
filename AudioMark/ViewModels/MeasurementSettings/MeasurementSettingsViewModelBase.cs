using AudioMark.Core.Measurements;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

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

        public abstract bool IsCompleted { get; set; }
    }
}
