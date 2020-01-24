using AudioMark.Common;
using AudioMark.Core.Measurements;
using AudioMark.Views.Common;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<Unit> LoadMeasurement()
        {
            try
            {
                var fileFilters = new List<Interactions.FileOptions.Filter>()
                {
                    new Interactions.FileOptions.Filter()
                    {
                        Extensions = new List<string> { FileExtensions.Measurement },
                        Name = "Measurement"
                    }
                };

                var fileNames = await Interactions.LoadFile.Handle(new Interactions.LoadFileOptions()
                {
                    AllowMultiple = false,
                    Title = "Choose measurement file to load...",
                    Filters = fileFilters
                });

                if (fileNames != null && fileNames.Any())
                {
                    var measurement = MeasurementsFactory.Load(fileNames[0]);
                    AddMeasurement(measurement, Items.Count);
                }
            }
            catch (Exception e)
            {
                await Interactions.Error.Handle(e);
            }

            return Unit.Default;
        }
    }
}
