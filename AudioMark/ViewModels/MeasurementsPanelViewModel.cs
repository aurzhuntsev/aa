﻿using AudioMark.Core.Common;
using AudioMark.Core.Measurements;
using AudioMark.ViewModels.Measurements;
using AudioMark.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
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

        private Action<SpectralData> _dataUpdate;

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

                    _content.Measurement.OnComplete += (m, e) => { Running = false; };
                    _content.Measurement.OnDataUpdate += (m, e) =>
                    {
                        var data = e as SpectralData;
                        if (data != null)
                        {
                            _dataUpdate(data);
                        }
                    };
                }

                return _content;
            }
        }

        private List<Tuple<Type, Type>> _meausrementViewModelAssociations = new List<Tuple<Type, Type>>();
        private void RegisterMeasurementViewModelAssociation<M, VM>() where M : IMeasurement where VM : MeasurementViewModelBase =>
            _meausrementViewModelAssociations.Add(new Tuple<Type, Type>(typeof(M), typeof(VM)));

        private void RegisterMeasurementViewModelAssociations()
        {
            RegisterMeasurementViewModelAssociation<ThdMeasurement, ThdMeasurementViewModel>();
        }

        public MeasurementsPanelViewModel(Action<SpectralData> dataUpdate)
        {
            _dataUpdate = dataUpdate;

            Items = new ObservableCollection<string>(MeasurementsFactory.List().Select(item => item.Name));

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
                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    desktop.Exit += OnExit;
                }

                Running = true;
                await Content.Measurement.Run();
            }
            else
            {
                Content.Measurement.Stop();

                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    desktop.Exit -= OnExit;
                }
            }
        }

        private void OnExit(object sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            if (Content != null && Content.Measurement != null && Content.Measurement.Running)
            {
                Content.Measurement.Stop();
            }
        }
    }
}
