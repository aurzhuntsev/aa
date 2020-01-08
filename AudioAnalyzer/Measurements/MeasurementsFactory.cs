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
            public string Name { get; set; }
        }

        private static List<MeasurementListItem> measurements = new List<MeasurementListItem>();

        public static void Register<T>() where T: MeasurementBase
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
                Name = name
            };
            measurements.Add(item);
        }

        public static IEnumerable<MeasurementListItem> List() => measurements;

        public static MeasurementBase Create(string name)
        {
            var item = measurements.FirstOrDefault(m => m.Name == name);
            if (item == null)
            {
                throw new KeyNotFoundException(name);
            }

            return (MeasurementBase)Activator.CreateInstance(item.Type);
        }

        static MeasurementsFactory()
        {
            Register<NoiseMeasurement>();
            Register<ThdMeasurement>();
        }
    }
}
