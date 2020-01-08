using AudioMark.Core.Measurements;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.ViewModels.Measurements
{
    public class MeasurementViewModelBase : ViewModelBase
    {
        private MeasurementBase _measurement;
        public MeasurementBase Measurement
        {
            get => _measurement;
            set => this.RaiseAndSetIfChanged(ref _measurement, value);
        }
    }
}
