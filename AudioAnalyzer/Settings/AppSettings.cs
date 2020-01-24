using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AudioMark.Core.Settings
{
    [Serializable]
    public class AppSettings
    {
        public Device Device { get; set; }
        public Fft Fft { get; set; }
        public StopConditions StopConditions { get; set; }

        public static AppSettings Current
        {
            get
            {
                return _current.Value;
            }
        }

        private static Lazy<AppSettings> _current = CreateLazy();

        private AppSettings()
        {            
        }

        private static string GetSettingsFilePath() => Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

        private static AppSettings Load()
        {
            using (var streamReader = new StreamReader(GetSettingsFilePath()))
            {
                return JsonSerializer.Deserialize<AppSettings>(streamReader.ReadToEnd());
            }
        }

        private static Lazy<AppSettings> CreateLazy()
        {
            return new Lazy<AppSettings>(() =>
            {
                var appSettings = Load();
                return appSettings;
            });
        }

        public void Save()
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };

            var data = JsonSerializer.Serialize<AppSettings>(this, options);
            using (var streamWriter = new StreamWriter(GetSettingsFilePath(), false))
            {
                streamWriter.Write(data);
            }
        }

        public void Reset()
        {
            _current = CreateLazy();
        }
    }
}
