using AudioMark.Common;
using AudioMark.Core.Common;
using AudioMark.Core.Measurements;
using AudioMark.Core.Measurements.Settings.Common;
using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using AudioMark.Core.Measurements.Common;

namespace AudioMark.ViewModels.Common
{
    public class InputOutputLevelViewModel : ViewModelBase
    {
        private Tuner _tuner = new Tuner();

        public InputOutputLevel Model { get; set; } = new InputOutputLevel();

        public double OutputLevel
        {
            get => Model.OutputLevel;
            set
            {
                this.RaiseAndSetIfPropertyChanged(() => Model.OutputLevel, value);
                _tuner.OutputLevel = value;
            }
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

        
        private bool _isTunerActive;
        public bool IsTunerActive
        {
            get => _isTunerActive;
            set => this.RaiseAndSetIfChanged(ref _isTunerActive, value);
        }

        public InputOutputLevelViewModel(InputOutputLevel model)
        {
            Model = model;
            _tuner = new Tuner();
        }

        public void Test()
        {            
            _tuner.OnReading += (s, r) =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    LevelDbFs = r.InputLevelDbFs;
                    LevelDbTp = r.InputLevelDbTp;
                });
            };

            _tuner.OutputLevel = OutputLevel;
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
