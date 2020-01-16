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

        public string Result
        {
            get
            {
                var result = new StringBuilder();
                result.AppendLine($"THDf:       -123.33dB (0.000005%)");
                result.AppendLine($"THDr:       -121.44dB (0.0000056%)");
                result.AppendLine($"Even/odd:   0.1005");
                return result.ToString();
            }
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
