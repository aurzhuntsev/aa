using AudioMark.Common;
using AudioMark.Core.Measurements;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.ViewModels.Measurements
{
    public class ThdMeasurementViewModel : MeasurementViewModelBase
    {
        private ThdMeasurement Model
        {
            get => (ThdMeasurement)Measurement;
        }

        public double TestSignalFrequency
        {
            get => Model.TestSignalOptions.Frequency;
            set => this.RaiseAndSetIfPropertyChanged(() => Model.TestSignalOptions.Frequency, value, nameof(TestSignalFrequency));
        }
    }
}
