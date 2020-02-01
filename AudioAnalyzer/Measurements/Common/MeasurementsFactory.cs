using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using AudioMark.Core.Common;
using AudioMark.Core.Measurements.Analysis;
using AudioMark.Core.Measurements.Settings;
using AudioMark.Core.Measurements.Settings.Common;
/* TODO: Customize scroll looknfeel */
namespace AudioMark.Core.Measurements.Common
{
    public class MeasurementsFactory
    {
        const string MeasurementFileMagicString = "AMME";
        const int VersionLength = 16;

        public class MeasurementListItem
        {
            public Type Type { get; set; }
            public Type SettingsType { get; set; }
            public Type ReportType { get; set; }
            public Type ResultType { get; set; }

            public string Name { get; set; }
        }

        private static List<MeasurementListItem> measurements = new List<MeasurementListItem>();

        public static void Register<T, TSettings, TReport, TResult>() where T : MeasurementBase<TResult> where TSettings : IMeasurementSettings where TReport : IAnalysisResult
        {
            var type = typeof(T);

            var name = type.GetStringAttributeValue<MeasurementAttribute>();
            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidOperationException(type.Name);
            }

            var item = new MeasurementListItem()
            {
                Type = type,
                SettingsType = typeof(TSettings),
                ReportType = typeof(TReport),
                ResultType = typeof(TResult),
                Name = name
            };
            measurements.Add(item);
        }

        public static IEnumerable<MeasurementListItem> List() => measurements;

        public static IMeasurement Create(string name, IMeasurementSettings settings)
        {
            var item = measurements.FirstOrDefault(m => m.Name == name);
            if (item == null)
            {
                throw new KeyNotFoundException(name);
            }

            return (IMeasurement)Activator.CreateInstance(item.Type, new[] { settings });
        }

        public static IMeasurementSettings CreateSettings(string name)
        {
            var item = measurements.FirstOrDefault(m => m.Name == name);
            if (item == null)
            {
                throw new KeyNotFoundException(name);
            }

            return (IMeasurementSettings)Activator.CreateInstance(item.SettingsType);
        }

        public static IMeasurement Load(string fileName)
        {
            var formatter = new BinaryFormatter();
            using (var streamReader = new StreamReader(fileName))
            {
                var buffer = new byte[VersionLength];

                streamReader.BaseStream.Read(buffer, 0, MeasurementFileMagicString.Length);
                if (MeasurementFileMagicString != Encoding.ASCII.GetString(buffer, 0, MeasurementFileMagicString.Length))
                {
                    throw new Exception("Unexpected beginning of the file.");
                }

                streamReader.BaseStream.Read(buffer, 0, VersionLength);
                var version = Encoding.ASCII.GetString(buffer);
                var currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                bool matchesVersion = version.Trim() == currentVersion;

                try
                {
                    var container = (MeasurementSerializationContainer)formatter.Deserialize(streamReader.BaseStream);
                    var item = measurements.FirstOrDefault(m => m.Type.Name == container.TypeName);
                    if (item == null)
                    {
                        throw new KeyNotFoundException(container.TypeName);
                    }

                    var result = (IMeasurement)Activator.CreateInstance(item.Type, new object[] { container.Settings, container.Result });
                    result.Name = container.Name;

                    return result;
                }
                catch (Exception e)
                {
                    if (matchesVersion)
                    {
                        throw e;
                    }
                    else
                    {
                        throw new Exception($"Unable to open file it's version of {version} is incompatible with the current software version is {currentVersion}", e);
                    }
                }
            }
        }

        /* TODO: Maybe find better spot */
        public static void Save(string fileName, IMeasurement measurement)
        {
            var formatter = new BinaryFormatter();
            using (var streamWriter = new StreamWriter(fileName, false))
            {
                var amme = Encoding.ASCII.GetBytes(MeasurementFileMagicString);
                streamWriter.BaseStream.Write(amme, 0, MeasurementFileMagicString.Length);

                var version = Encoding.ASCII.GetBytes(Assembly.GetExecutingAssembly().GetName().Version.ToString().PadRight(16));
                streamWriter.BaseStream.Write(version, 0, version.Length);

                var container = new MeasurementSerializationContainer()
                {
                    TypeName = measurement.GetType().Name,
                    Name = measurement.Name,
                    Settings = measurement.Settings,
                    Result = measurement.AnalysisResult
                };

                formatter.Serialize(streamWriter.BaseStream, container);
            }
        }

        static MeasurementsFactory()
        {
            Register<ThdMeasurement, ThdMeasurementSettings, ThdAnalysisResult, SpectralData>();
            Register<NoiseMeasurement, NoiseMeasurementSettings, NoiseAnalysisResult, SpectralData>();
        }
    }
}
