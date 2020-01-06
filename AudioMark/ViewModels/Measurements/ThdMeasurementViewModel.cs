using AudioMark.Common;
using AudioMark.Core.Measurements;
using AudioMark.ViewModels.Common;
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

        public InputOutputLevelViewModel TestSignalInputOutputLevelOptions
        {
            get => new InputOutputLevelViewModel(Model.TestSignalOptions.InputOutputOptions);
        }

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

    }
}
