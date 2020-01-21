using AudioMark.Common;
using AudioMark.Core.Measurements;
using AudioMark.Views.Common;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Text;

namespace AudioMark.ViewModels
{
    public class SessionPanelViewModel : ViewModelBase
    {
        public ObservableCollection<SessionItemViewModel> Items { get; } = new ObservableCollection<SessionItemViewModel>();

        public SessionPanelViewModel()
        {
            AddMeasurement(new ThdMeasurement(null) { Name = "Text test test" }, 0);
        }

        public void AddMeasurement(IMeasurement measurement, int index)
        {
            var itemViewModel = new SessionItemViewModel(measurement) { SeriesIndex = index };
            itemViewModel.WhenRemoved.Subscribe(async item =>
            {
                var result = await Interactions.Confirm.Handle("Do you really want to delete this measurement?");
                if (result)
                {
                    Items.Remove(item);
                }
            });

            itemViewModel.WhenVisibilityChanged.Subscribe((item) =>
            {

            });

            Items.Insert(0, itemViewModel);
        }
    }
}
