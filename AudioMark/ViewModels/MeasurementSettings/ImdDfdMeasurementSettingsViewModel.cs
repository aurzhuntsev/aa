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
    [DefaultViewModel(typeof(ImdDfdMeasurementSettings))]
    public class ImdDfdMeasurementSettingsViewModel : MeasurementSettingsViewModelBase
    {
        public ImdDfdMeasurementSettings Model
        {
            get => (ImdDfdMeasurementSettings)Settings;
        }

        #region Test signal options
        public double TestSignalFrequency
        {
            get => Model.TestSignalOptions.Frequency;
            set => this.RaiseAndSetIfPropertyChanged(() => Model.TestSignalOptions.Frequency, value, nameof(TestSignalFrequency));
        }

        public double FrequencyDifference
        {
            get => Model.FrequencyDifference;
            set => this.RaiseAndSetIfPropertyChanged(() => Model.FrequencyDifference, value, nameof(FrequencyDifference));
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

        #endregion
        public ImdDfdMeasurementSettingsViewModel(ImdDfdMeasurementSettings settings) : base(settings)
        {            
        }
    }
}
