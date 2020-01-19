using AudioMark.Common;
using AudioMark.Core.Measurements;
using AudioMark.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
