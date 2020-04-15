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
    [DefaultViewModel(typeof(ImdModMeasurementSettings))]
    public class ImdModMeasurementSettingsViewModel : MeasurementSettingsViewModelBase
    {
        public ImdModMeasurementSettings Model
        {
            get => (ImdModMeasurementSettings)Settings;
        }

        public double TestSignalFrequency
        {
            get => Model.TestSignalOptions.Frequency;
            set => this.RaiseAndSetIfPropertyChanged(() => Model.TestSignalOptions.Frequency, value, nameof(TestSignalFrequency));
        }

        public double SecondarySignalFrequency
        {
            get => Model.SecondarySignalFrequency;
            set => this.RaiseAndSetIfPropertyChanged(() => Model.SecondarySignalFrequency, value, nameof(SecondarySignalFrequency));
        }

        public double SignalsRate
        {
            get => Model.SignalsRate;
            set => this.RaiseAndSetIfPropertyChanged(() => Model.SignalsRate, value, nameof(SignalsRate));
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
            }
        }

        public int WarmUpDurationSeconds
        {
            get => Model.WarmUpDurationSeconds;
            set => this.RaiseAndSetIfPropertyChanged(() => Model.WarmUpDurationSeconds, value);
        }
       
        public int HarmonicDetectionWindow
        {
            get => Model.WindowHalfSize;
            set
            {
                this.RaiseAndSetIfPropertyChanged(() => Model.WindowHalfSize, value, nameof(HarmonicDetectionWindow));
                _whenAnalysisOptionsChanged.OnNext(this);
            }
        }

        public int MaxOrder
        {
            get => Model.MaxOrder;
            set
            {
                this.RaiseAndSetIfPropertyChanged(() => Model.MaxOrder, value, nameof(MaxOrder));
                _whenAnalysisOptionsChanged.OnNext(this);
            }
        }

        public bool LimitMaxFrequency
        {
            get => Model.LimitMaxFrequency;
            set
            {
                this.RaiseAndSetIfPropertyChanged(() => Model.LimitMaxFrequency, value, nameof(LimitMaxFrequency));
                _whenAnalysisOptionsChanged.OnNext(this);
            }
        }

        public double MaxFrequency
        {
            get => Model.MaxFrequency;
            set
            {
                this.RaiseAndSetIfPropertyChanged(() => Model.MaxFrequency, value, nameof(MaxFrequency));
                _whenAnalysisOptionsChanged.OnNext(this);
            }
        }

        public ImdModMeasurementSettingsViewModel(ImdModMeasurementSettings settings) : base(settings)
        {            
        }
    }
}
