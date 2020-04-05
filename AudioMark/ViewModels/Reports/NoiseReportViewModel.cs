using AudioMark.Common;
using AudioMark.Core.Measurements.Analysis;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.ViewModels.Reports
{
    [DefaultViewModel(typeof(NoiseAnalysisResult))]
    public class NoiseReportViewModel : ReportViewModelBase
    {
        public NoiseAnalysisResult Model { get; set; }

        public string NoisePowerDbFs
        {
            get => $"{Model.NoisePowerDbFs.ToString("F4")}dB";
        }

        public string AverageLevelDbTp
        {
            get => $"{Model.AverageLevelDbTp.ToString("F4")}dB";
        }

        public NoiseReportViewModel(NoiseAnalysisResult model)
        {
            Model = model;
        }

        public override string GetText()
        {
            return Model.ToString();   
        }

        public override void Update()
        {
            this.RaisePropertyChanged(nameof(NoisePowerDbFs));
            this.RaisePropertyChanged(nameof(AverageLevelDbTp));
        }
    }
}
