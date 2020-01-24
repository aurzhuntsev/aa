using AudioMark.Common;
using AudioMark.Core.Measurements;
using AudioMark.ViewModels.Reports;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace AudioMark.ViewModels
{
    public class SessionItemViewModel : ViewModelBase
    {
        public IMeasurement Measurement { get; }

        public string Name
        {
            get => Measurement.Name;
            set => this.RaiseAndSetIfPropertyChanged(() => Measurement.Name, value);
        }

        private int _seriesIndex;
        public int SeriesIndex
        {
            get => _seriesIndex;
            set => this.RaiseAndSetIfChanged(ref _seriesIndex, value);
        }

        private bool _visible = true;
        public bool Visible
        {
            get => _visible;
            set => this.RaiseAndSetIfChanged(ref _visible, value);
        }

        private bool _selected = false;
        public bool Selected
        {
            get => _selected;
            set => this.RaiseAndSetIfChanged(ref _selected, value);
        }

        private Subject<SessionItemViewModel> _whenRemoved = new Subject<SessionItemViewModel>();
        public IObservable<SessionItemViewModel> WhenRemoved
        {
            get => _whenRemoved.AsObservable();
        }

        private Subject<SessionItemViewModel> _whenVisibilityChanged = new Subject<SessionItemViewModel>();
        public IObservable<SessionItemViewModel> WhenVisibilityChanged
        {
            get => _whenVisibilityChanged.AsObservable();
        }

        private Subject<SessionItemViewModel> _whenNameEdited = new Subject<SessionItemViewModel>();
        public IObservable<SessionItemViewModel> WhenNameEdited
        {
            get => _whenNameEdited.AsObservable();
        }

        private Subject<SessionItemViewModel> _whenSelectionChanged  = new Subject<SessionItemViewModel>();
        public IObservable<SessionItemViewModel> WhenSelectionChanged
        {
            get => _whenSelectionChanged.AsObservable();
        }


        public ReportViewModelBase Report
        {
            get => (ReportViewModelBase)DefaultForModel(Measurement.AnalysisResult);
        }

        public SessionItemViewModel(IMeasurement measurement)
        {
            Measurement = measurement;
        }

        public void CopyToClipboard()
        {
            TextCopy.Clipboard.SetText(Report.GetText());
        }

        public void ToggleVisibility()
        {
            Visible = !Visible;
            _whenVisibilityChanged.OnNext(this);
        }

        public void Remove()
        {
            _whenRemoved.OnNext(this);
        }

        public async Task<Unit> Save()
        {
            try
            {
                var saveFileFilters = new[] {
                new Interactions.SaveFileOptions.Filter() {
                    Name = "Measurement",
                    Extensions = new List<string>() { FileExtensions.Measurement}
                }
            };

                var fileName = await Interactions.SaveFile.Handle(new Interactions.SaveFileOptions()
                {
                    Filters = saveFileFilters.ToList(),
                    DefaultExtension = FileExtensions.Measurement,
                    Directory = ".",
                    InitialFileName = Name + "." + FileExtensions.Measurement,
                    Title = "Save measurement..."
                }); ;

                if (!string.IsNullOrEmpty(fileName))
                {
                    Measurement.SaveToFile(fileName);
                }
            }
            catch (Exception e)
            {
                await Interactions.Error.Handle(e);
            }

            return Unit.Default;
        }

        public async void EditName()
        {
            var result = await Interactions.Input.Handle(new Interactions.InputOptions() { Text = "Enter measurement name:", Value = Name });
            if (!result.Canceled)
            {
                Name = result.Value;
                _whenNameEdited.OnNext(this);
            }
        }        

        public void SelectItem()
        {
            Selected = !Selected;
            _whenSelectionChanged.OnNext(this);
        }
    }
}
