using AudioMark.Core.Common;
using AudioMark.Core.Measurements;
using AudioMark.ViewModels.Measurements;
using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace AudioMark.ViewModels
{
    public class MeasurementsPanelViewModel : ViewModelBase
    {
        public ObservableCollection<string> Items { get; }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedIndex, value);
        }

        private bool _running;
        public bool Running
        {
            get => _running;
            set => this.RaiseAndSetIfChanged(ref _running, value);
        }

        private GraphViewModel _graph;

        private MeasurementViewModelBase _content = null;
        public MeasurementViewModelBase Content
        {
            get
            {
                var type =
                    _meausrementViewModelAssociations.First(item =>
                        item.Item1 == MeasurementsFactory.List().First(item =>
                            item.Name == Items[SelectedIndex]).Type).Item2;

                if (_content == null || _content.GetType() != type)
                {
                    _content = (MeasurementViewModelBase)Activator.CreateInstance(type);
                    _content.Measurement = MeasurementsFactory.Create(Items[SelectedIndex]);

                    _content.Measurement.OnComplete += (m) => { Running = false; };
                    _content.Measurement.OnDataUpdate += (m, e) =>
                    {
                        var data = e as double[];
                        if (data != null)
                        {
                            if (_graph.Data == null)
                            {
                                _graph.Data = new double[data.Length];
                            };

                            for (var i = 0; i < _graph.Data.Length; i++)
                            {
                                _graph.Data[i] = data[i];
                            }

                            _graph.Render();
                        }
                    };
                }

                return _content;
            }
        }

        private List<Tuple<Type, Type>> _meausrementViewModelAssociations = new List<Tuple<Type, Type>>();
        private void RegisterMeasurementViewModelAssociation<M, VM>() where M : MeasurementBase where VM : MeasurementViewModelBase =>
            _meausrementViewModelAssociations.Add(new Tuple<Type, Type>(typeof(M), typeof(VM)));

        private void RegisterMeasurementViewModelAssociations()
        {
            RegisterMeasurementViewModelAssociation<NoiseMeasurement, NoiseMeasurementViewModel>();
            RegisterMeasurementViewModelAssociation<ThdMeasurement, ThdMeasurementViewModel>();
        }

        public MeasurementsPanelViewModel(GraphViewModel graph)
        {
            Items = new ObservableCollection<string>(MeasurementsFactory.List().Select(item => item.Name));
            _graph = graph;

            RegisterMeasurementViewModelAssociations();

            this.WhenAnyValue(x => x.SelectedIndex).Subscribe(x =>
            {
                this.RaisePropertyChanged("Content");
            });
        }

        public async void Run(Button sender)
        {
            if (!Running)
            {
                Running = true;
                await Content.Measurement.Run();
            }
            else
            {
                Content.Measurement.Stop();
            }
        }
    }
}
