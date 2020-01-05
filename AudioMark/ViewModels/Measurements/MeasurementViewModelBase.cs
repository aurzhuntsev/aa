using AudioMark.Core.Measurements;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.ViewModels.Measurements
{
    public class MeasurementViewModelBase : ViewModelBase
    {
        private IMeasurement _measurement;
        public IMeasurement Measurement
        {
            get => _measurement;
            set => this.RaiseAndSetIfChanged(ref _measurement, value);
        }
    }
}
