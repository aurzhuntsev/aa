using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;

namespace AudioMark.ViewModels.Reports
{
    public abstract class ReportViewModelBase : ViewModelBase
    {
        public abstract string GetText();
    }
}
