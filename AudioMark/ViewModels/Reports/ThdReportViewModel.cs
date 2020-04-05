using AudioMark.Common;
using AudioMark.Core.Measurements;
using AudioMark.Core.Measurements.Analysis;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.ViewModels.Reports
{
    [DefaultViewModel(typeof(ThdAnalysisResult))]
    public class ThdReportViewModel : ReportViewModelBase
    {
        public ThdAnalysisResult Model { get; set; }

        public string ThdN
        {
            get => $"{Model.TotalThdPlusNoiseDb.ToString("F4")}dB ({Model.TotalThdPlusNoisePercentage.ToString("F8")}%)";
        }

        /* TODO: Some neat rounding */
        public string ThdF
        {
            get => $"{Model.ThdFDb.ToString("F4")}dB ({Model.ThdFPercentage.ToString("F8")}%)";
        }

        public string ThdR
        {
            get => $"{Model.ThdRDb.ToString("F4")}dB ({Model.ThdRPercentage.ToString("F8")}%)";
        }
        
        public ThdReportViewModel(ThdAnalysisResult model)
        {
            Model = model;
        }

        public override string GetText()
        {
            return Model.ToString();
        }

        public override void Update()
        {
            this.RaisePropertyChanged(nameof(ThdN));
            this.RaisePropertyChanged(nameof(ThdF));
            this.RaisePropertyChanged(nameof(ThdR));            
        }
    }
}
