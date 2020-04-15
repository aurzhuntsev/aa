using AudioMark.Common;
using AudioMark.Core.Measurements.Settings;
using AudioMark.Core.Measurements.Settings.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.ViewModels.MeasurementSettings
{
    [DefaultViewModel(typeof(FrequencyResponseMeasurementSettings))]
    public class FrequencyResponseMeasurementSettingsViewModel : MeasurementSettingsViewModelBase
    {
        public FrequencyResponseMeasurementSettingsViewModel(FrequencyResponseMeasurementSettings model) : base(model)
        {
        }
    }
}
