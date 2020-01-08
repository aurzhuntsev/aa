using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioMark.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MeasurementsPanelViewModel Measurements { get; }
        public CurrentMeasurementViewModel CurrentMeasurement { get; }
        public GraphViewModel Graph { get; }

        public double[] Data { get; set; }

        public MainWindowViewModel()
        {
            Graph = new GraphViewModel();
            Measurements = new MeasurementsPanelViewModel(Graph);
            CurrentMeasurement = new CurrentMeasurementViewModel();            

            this.WhenAnyValue(_ => _.Measurements.Running)
                .Subscribe(running =>
                {
                    if (Measurements.Content != null)
                    {
                        if (running)
                        {
                            CurrentMeasurement.StartMonitoring(Measurements.Content.Measurement);
                        }
                        else
                        {
                            CurrentMeasurement.StopMonitoring();
                        }
                    }
                });                       
        }
    }
}
