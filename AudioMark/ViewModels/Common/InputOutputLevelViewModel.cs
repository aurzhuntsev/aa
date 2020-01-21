using AudioMark.Common;
using AudioMark.Core.Common;
using AudioMark.Core.Measurements;
using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.ViewModels.Common
{
    public class InputOutputLevelViewModel : ViewModelBase
    {
        private Tuner _tuner = new Tuner();

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

        private double _levelDbTp;
        public double LevelDbTp
        {
            get => _levelDbTp;
            set => this.RaiseAndSetIfChanged(ref _levelDbTp, value);
        }

        private double _levelDbFs;
        public double LevelDbFs
        {
            get => _levelDbFs;
            set => this.RaiseAndSetIfChanged(ref _levelDbFs, value);
        }

        public SignalLevelMode InputLevelMode
        {
            get => Model.InputLevelMode;
            set => this.RaiseAndSetIfPropertyChanged(() => Model.InputLevelMode, value);
        }

        private bool _isTunerActive;
        public bool IsTunerActive
        {
            get => _isTunerActive;
            set => this.RaiseAndSetIfChanged(ref _isTunerActive, value);
        }

        public InputOutputLevelViewModel(InputOutputLevel model)
        {
            Model = model;
        }

        public void Test()
        {
            _tuner = new Tuner() { SignalLevelMode = SignalLevelMode.dBFS };
            _tuner.OnReading += (s, r) =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    LevelDbFs = r.InputLevelDbFs;
                    LevelDbTp = r.InputLevelDbTp;
                });
            };

            IsTunerActive = true;
            _tuner.Test();
        }

        public void Stop()
        {
            IsTunerActive = false;
            _tuner.Stop();
        }
    }
}
