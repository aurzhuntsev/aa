using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MeasurementsPanelViewModel Measurements { get; }

        public MainWindowViewModel()
        {
            Measurements = new MeasurementsPanelViewModel();
        }
    }
}
