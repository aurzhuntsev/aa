using AudioMark.Common;
using AudioMark.Core.Measurements;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.ViewModels.Reports
{
    [DefaultViewModel(typeof(ThdAnalysisResult))]
    public class ThdReportViewModel : ReportViewModelBase
    {
        public ThdAnalysisResult Model { get; set; }

        /* TODO: Some neat rounding */
        public string ThdF
        {
            get => $"{Model.ThdFDb}dB ({Model.ThdFPercentage}%)";
        }

        public string ThdR
        {
            get => $"{Model.ThdRDb}dB ({Model.ThdRPercentage}%)";
        }

        public string EvenToOdd
        {
            get => Model.EvenToOdd.ToString(); 
        }

        public ThdReportViewModel(ThdAnalysisResult model)
        {
            Model = model;
        }

        public override string GetText()
        {
            return Model.ToString();
        }
    }
}
