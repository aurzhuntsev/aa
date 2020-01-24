using AudioMark.Common;
using AudioMark.Core.Measurements;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioMark.ViewModels.MeasurementSettings.Common
{
    public class CorrectionProfileViewModel : ViewModelBase
    {
        public IMeasurement Measurement { get; private set; }

        private bool _applyCorrectionProfile;
        public bool ApplyCorrectionProfile
        {
            get => _applyCorrectionProfile;
            set => this.RaiseAndSetIfChanged(ref _applyCorrectionProfile, value);
        }

        private string _profileFileName;
        public string ProfileFileName
        {
            get => _profileFileName;
            set => this.RaiseAndSetIfChanged(ref _profileFileName, value);
        } 

        public CorrectionProfileViewModel()
        {
            ProfileFileName = "(no profile loaded)";
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
                    Measurement = MeasurementsFactory.Load(fileNames[0]);
                    ProfileFileName = Measurement.Name;

                    ApplyCorrectionProfile = true;
                    /* TODO: Finish */
                }
            }
            catch (Exception e)
            {
                await Interactions.Error.Handle(e);
            }

            return Unit.Default;
        }
    }
}
