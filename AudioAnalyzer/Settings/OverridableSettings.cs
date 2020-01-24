using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace AudioMark.Core.Settings
{
    [Serializable]
    public class OverridableSettings<T> where T : new()
    {
        public bool Overriden { get; set; }

        private T _globalSettings;
        private T _overridenValue;

        public T Value
        {
            get
            {
                if (Overriden)
                {
                    if (_overridenValue == null)
                    {
                        _overridenValue = CloneGlobalSettings();
                    }
                    return _overridenValue;
                }

                return CloneGlobalSettings();
            }
        }

        public OverridableSettings(T globalSettings)
        {
            _globalSettings = globalSettings;
        }

        private T CloneGlobalSettings()
        {
            /* TODO: Implement something better at some point */
            var jsonValue = JsonSerializer.Serialize<T>(_globalSettings);
            return JsonSerializer.Deserialize<T>(jsonValue);
        }
    }
}
