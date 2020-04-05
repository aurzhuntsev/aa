using AudioMark.Common;
using AudioMark.Core.Measurements.Analysis;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioMark.ViewModels.Reports
{
    [DefaultViewModel(typeof(ImdAnalysisResult))]
    public class ImdReportViewModel : ReportViewModelBase
    {
        public class ImdItem 
        { 
            public string Order { get; set; }
            public string Value { get; set; }
        }

        public ImdAnalysisResult Model { get; set; }

        public string TotalImdPlusNoise
        {
            get => $"{Model.TotalImdPlusNoiseDb.ToString("F4")}dB ({Model.TotalImdPlusNoisePercentage.ToString("F4")}%)";
        }

        public string ImdF2
        {
            get => $"{Model.ImdF2ForGivenOrderDb.ToString("F4")}dB ({Model.ImdF2ForGivenOrderPercentage.ToString("F4")}%)";
        }

        public string ImdF1F2
        {
            get => $"{Model.ImdF1F2ForGivenOrderDb.ToString("F4")}dB ({Model.ImdF1F2ForGivenOrderPercentage.ToString("F4")}%)";
        }

        public string F1
        {
            get => $"{Model.F1Db.ToString("F4")}dB";
        }

        public string F2
        {
            get => $"{Model.F2Db.ToString("F4")}dB";
        }

        public List<ImdItem> Imd
        {
            get => Model.OrderedImd.OrderBy(x => x.Key)
                .Select(x => new ImdItem()
                {
                    Order = $"{x.Key} order:",
                    Value = $"{x.Value.ToString("F4")}dB"
                })
                .ToList();
        }

        public ImdReportViewModel(ImdAnalysisResult model)
        {
            Model = model;
        }

        public override string GetText()
        {
            return Model.ToString();
        }

        public override void Update()
        {
            this.RaisePropertyChanged(nameof(TotalImdPlusNoise));
            this.RaisePropertyChanged(nameof(F1));
            this.RaisePropertyChanged(nameof(F2));
            this.RaisePropertyChanged(nameof(ImdF2));
            this.RaisePropertyChanged(nameof(ImdF1F2));
            this.RaisePropertyChanged(nameof(Imd));
        }
    }
}
