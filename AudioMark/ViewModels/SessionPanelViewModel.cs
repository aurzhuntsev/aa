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
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using AudioMark.Core.Measurements.Common;

namespace AudioMark.ViewModels
{
    public class SessionPanelViewModel : ViewModelBase
    {
        public ObservableCollection<SessionItemViewModel> Items { get; } = new ObservableCollection<SessionItemViewModel>();

        private Subject<SessionItemViewModel> _whenSessionItemVisibilityChanged = new Subject<SessionItemViewModel>();
        public IObservable<SessionItemViewModel> WhenSessionItemVisibilityChanged
        {
            get => _whenSessionItemVisibilityChanged.AsObservable();
        }

        private Subject<SessionItemViewModel> _whenSessionItemAdded = new Subject<SessionItemViewModel>();
        public IObservable<SessionItemViewModel> WhenSessionItemAdded
        {
            get => _whenSessionItemAdded.AsObservable();
        }

        private Subject<SessionItemViewModel> _whenSessionItemRemoved = new Subject<SessionItemViewModel>();
        public IObservable<SessionItemViewModel> WhenSessionItemRemoved
        {
            get => _whenSessionItemRemoved.AsObservable();
        }

        private Subject<SessionItemViewModel> _whenSessionItemSelectionChanged = new Subject<SessionItemViewModel>();
        public IObservable<SessionItemViewModel> WhenSessionItemSelectionChanged
        {
            get => _whenSessionItemSelectionChanged.AsObservable();
        }

        public SessionPanelViewModel()
        {         
        }

        public SessionItemViewModel AddMeasurement(IMeasurement measurement, int index)
        {
            measurement.Update();
            var itemViewModel = new SessionItemViewModel(measurement) { SeriesIndex = index };

            itemViewModel.WhenRemoved.Subscribe(async item =>
            {
                var result = await Interactions.Confirm.Handle("Do you really want to delete this measurement?");
                if (result)
                {
                    Items.Remove(item);
                    _whenSessionItemRemoved.OnNext(item);
                }
            });

            itemViewModel.WhenVisibilityChanged.Subscribe((item) =>
            {
                _whenSessionItemVisibilityChanged.OnNext(item);
            });

            itemViewModel.WhenSelectionChanged.Subscribe((item) =>
            {
                foreach (var otherItem in Items)
                {
                    if (otherItem == item)
                    {
                        continue;
                    }

                    otherItem.Selected = false;
                }

                _whenSessionItemSelectionChanged.OnNext(item);
            });

            Items.Insert(0, itemViewModel);

            return itemViewModel;
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
                    var item = AddMeasurement(measurement, Items.Count);
                    _whenSessionItemAdded.OnNext(item);
                }
            }
            catch (Exception e)
            {
                await Interactions.Error.Handle(e);
            }

            return Unit.Default;
        }

        public void UpdateMeasurement(IMeasurement measurement)
        {
            var item = Items.First(x => x.Measurement == measurement);
            item.Measurement.Update();
            item.Update();
        }
    }
}
