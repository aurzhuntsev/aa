using AudioMark.Common;
using AudioMark.Core.Measurements.Settings;
using AudioMark.Core.Measurements.Settings.Common;
using AudioMark.ViewModels.Common;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioMark.ViewModels.MeasurementSettings
{
    [DefaultViewModel(typeof(FrequencyResponseMeasurementSettings))]
    public class FrequencyResponseMeasurementSettingsViewModel : MeasurementSettingsViewModelBase
    {
        public FrequencyResponseMeasurementSettings Model
        {
            get => (FrequencyResponseMeasurementSettings)Settings;
        }

        public InputOutputLevelViewModel TestSignalInputOutputLevelOptions
        {
            get => new InputOutputLevelViewModel(Model.TestSignalOptions.InputOutputOptions);
        }

        public double FrequencyFrom
        {
            get => Model.LowFrequency;
            set => this.RaiseAndSetIfPropertyChanged(() => Model.LowFrequency, value, nameof(FrequencyFrom));
        }

        public double FrequencyTo
        {
            get => Model.HighFrequency;
            set => this.RaiseAndSetIfPropertyChanged(() => Model.HighFrequency, value, nameof(FrequencyTo));
        }

        public List<string> DistributionModesList { get; set; }

        public string DistributionMode
        {
            get => Model.DistributionMode.ToString();
            set
            {
                var val = (DistributionModes)Enum.Parse(typeof(DistributionModes), value);
                this.RaiseAndSetIfPropertyChanged(() => Model.DistributionMode, val, nameof(DistributionMode));

                this.RaisePropertyChanged(nameof(ShowNumberOfPoints));
                this.RaisePropertyChanged(nameof(ShowFrequenciesList));
                this.RaisePropertyChanged(nameof(ShowMinLogStep));
            }
        }

        public int NumberOfPoints
        {
            get => Model.Points;
            set => this.RaiseAndSetIfPropertyChanged(() => Model.Points, value, nameof(NumberOfPoints));
        }

        public bool ShowNumberOfPoints
        {
            get => DistributionMode == DistributionModes.Logarithmic.ToString() || DistributionMode == DistributionModes.Linear.ToString();
        }

        public bool ShowMinLogStep
        {
            get => DistributionMode == DistributionModes.Logarithmic.ToString();
        }

        public double MinLogStep
        {
            get => Model.MinLogStep;
            set => this.RaiseAndSetIfPropertyChanged(() => Model.MinLogStep, value);
        }

        public bool ShowFrequenciesList
        {
            get => DistributionMode == DistributionModes.List.ToString();
        }

        public string FrequenciesList
        {
            get
            {
                return Model.Frequencies.Select(f => f.ToString()).Aggregate((a, b) => a + ", " + b);
            }
            set
            {
                var f = value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(v => double.Parse(v.Trim()));
                this.RaiseAndSetIfPropertyChanged(() => Model.Frequencies, f.ToList(), nameof(FrequenciesList));
            }
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

        public FrequencyResponseMeasurementSettingsViewModel(FrequencyResponseMeasurementSettings model) : base(model)
        {
            DistributionModesList = Enum.GetNames(typeof(DistributionModes)).ToList();
        }
    }
}
