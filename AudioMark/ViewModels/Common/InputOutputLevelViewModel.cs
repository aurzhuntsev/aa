using AudioMark.Common;
using AudioMark.Core.Measurements;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.ViewModels.Common
{
    public class InputOutputLevelViewModel : ViewModelBase
    {
        public InputOutputLevel Model { get; set; } = new InputOutputLevel();

        public double OutputLevel
        {
            get => Model.OutputLevel;
            set => this.RaiseAndSetIfPropertyChanged(() => Model.OutputLevel, value);
        }

        public double InputLevel
        {
            get => Model.InputLevel;
            set => this.RaiseAndSetIfPropertyChanged(() => Model.InputLevel, value);
        }

        public bool MatchInputLevel
        {
            get => Model.MatchInputLevel;
            set => this.RaiseAndSetIfPropertyChanged(() => Model.MatchInputLevel, value);
        }

        public SignalLevelMode InputLevelMode
        {
            get => Model.InputLevelMode;
            set => this.RaiseAndSetIfPropertyChanged(() => Model.InputLevelMode, value);
        }

        public InputOutputLevelViewModel(InputOutputLevel model)
        {
            Model = model;
        }
    }
}
