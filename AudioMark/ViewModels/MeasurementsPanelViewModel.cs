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
        
        public MeasurementViewModelBase Content
        {
            get
            {
                var type =
                    _meausrementViewModelAssociations.First(item => 
                        item.Item1 == MeasurementsFactory.List().First(item => 
                            item.Name == Items[SelectedIndex]).Type).Item2;

                var measurementViewModel = (MeasurementViewModelBase)Activator.CreateInstance(type);
                measurementViewModel.Measurement = MeasurementsFactory.Create(Items[SelectedIndex]);

                return measurementViewModel;
            }
        }

        private List<Tuple<Type, Type>> _meausrementViewModelAssociations = new List<Tuple<Type, Type>>();
        private void RegisterMeasurementViewModelAssociation<M, VM>() where M : IMeasurement where VM : MeasurementViewModelBase =>
            _meausrementViewModelAssociations.Add(new Tuple<Type, Type>(typeof(M), typeof(VM)));

        private void RegisterMeasurementViewModelAssociations()
        {
            RegisterMeasurementViewModelAssociation<NoiseMeasurement, NoiseMeasurementViewModel>();
            RegisterMeasurementViewModelAssociation<ThdMeasurement, ThdMeasurementViewModel>();
        }

        public MeasurementsPanelViewModel()
        {
            Items = new ObservableCollection<string>(MeasurementsFactory.List().Select(item => item.Name));

            RegisterMeasurementViewModelAssociations();

            this.WhenAnyValue(x => x.SelectedIndex).Subscribe(x =>
            {
                this.RaisePropertyChanged("Content");
            });
        }

        public void Run()
        {            
            var measurement = MeasurementsFactory.Create(Items[SelectedIndex]);
            measurement.Run();
        }
    }
}
