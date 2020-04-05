using AudioMark.Common;
using AudioMark.Core.Common;
using AudioMark.Core.Measurements;
using AudioMark.Core.Measurements.Settings.Common;
using AudioMark.Core.Settings;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using AudioMark.Core.Measurements.Common;

namespace AudioMark.ViewModels.MeasurementSettings.Common
{
    public class CorrectionProfileViewModel : ViewModelBase
    {
        private ICorrectionProfile _model;
        public Spectrum Target { get; set; }

        private Subject<Unit> _whenChanged = new Subject<Unit>();
        public IObservable<Unit> WhenChanged
        {
            get => _whenChanged.AsObservable();
        }

        public bool ApplyCorrectionProfile
        {
            get => _model.ApplyCorrectionProfile;
            set
            {
                this.RaiseAndSetIfPropertyChanged(() => _model.ApplyCorrectionProfile, value, nameof(ApplyCorrectionProfile));
                _whenChanged.OnNext(Unit.Default);
            }
        }

        public Spectrum CorrectionProfile
        {
            get => _model.CorrectionProfile;
            set => this.RaiseAndSetIfPropertyChanged(() => _model.CorrectionProfile, value, nameof(CorrectionProfile));
        }

        public string ProfileName
        {
            get => _model.CorrectionProfileName;
            set => this.RaiseAndSetIfPropertyChanged(() => _model.CorrectionProfileName, value, nameof(ProfileName));
        }

        public CorrectionProfileViewModel(ICorrectionProfile model)
        {
            _model = model;
        }

        public async Task<Unit> Load()
        {
            try
            {
                var fileFilters = new List<Interactions.FileOptions.Filter>()
                {
                    new Interactions.FileOptions.Filter()
                    {
                        Extensions = new List<string> { FileExtensions.Measurement },
                        Name = "Measurement"
                    }
                };

                var fileNames = await Interactions.LoadFile.Handle(new Interactions.LoadFileOptions()
                {
                    AllowMultiple = false,
                    Title = "Choose measurement file to load...",
                    Filters = fileFilters
                });

                if (fileNames != null && fileNames.Any())
                {
                    var profile = MeasurementsFactory.Load(fileNames[0]);
                    CorrectionProfile = profile.AnalysisResult.Data;
                    ProfileName = profile.Name;

                    ValidateCorrectionProfile();

                    _model.ApplyCorrectionProfile = true;                    
                    this.RaisePropertyChanged(nameof(ApplyCorrectionProfile));
                    
                    _whenChanged.OnNext(Unit.Default);                    
                }
            }
            catch (Exception e)
            {
                await Interactions.Error.Handle(e);
            }

            return Unit.Default;
        }

        private void ValidateCorrectionProfile()
        {
            int targetSampleRate = (Target == null) ? AppSettings.Current.Device.SampleRate : Target.MaxFrequency * 2;
            int targetWindowSize = (Target == null) ? AppSettings.Current.Fft.WindowSize : Target.Size;
            var errorText = new StringBuilder();

            if (targetSampleRate != CorrectionProfile.MaxFrequency * 2)
            {
                errorText.AppendLine($"Profile sample rate ({CorrectionProfile.MaxFrequency * 2}Hz) does not match the one of the selected measurement ({targetSampleRate}Hz)");
            }
            if (targetWindowSize != CorrectionProfile.Size)
            {
                errorText.AppendLine($"Profile FFT window size ({CorrectionProfile.Size}) does not match the one of the selected measurement ({targetWindowSize})");
            }

            if (errorText.Length != 0)
            {
                CorrectionProfile = null;
                throw new Exception(errorText.ToString());
            }
        }
    }
}
