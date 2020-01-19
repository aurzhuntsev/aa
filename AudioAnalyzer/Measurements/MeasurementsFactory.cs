using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioMark.Core.Common;

namespace AudioMark.Core.Measurements
{
    public class MeasurementsFactory
    {
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

        static MeasurementsFactory()
        {
            Register<ThdMeasurement, ThdMeasurementSettings, ThdAnalysisResult, SpectralData>();
        }
    }
}
