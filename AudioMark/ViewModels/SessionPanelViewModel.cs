using AudioMark.Core.Measurements;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace AudioMark.ViewModels
{
    public class SessionPanelViewModel : ViewModelBase
    {
        public ObservableCollection<SessionItemViewModel> Items { get; } = new ObservableCollection<SessionItemViewModel>();

        public SessionPanelViewModel()
        {
        }

        public void AddMeasurement(IMeasurement measurement)
        {
            Items.Add(new SessionItemViewModel(measurement));
        }
    }
}
