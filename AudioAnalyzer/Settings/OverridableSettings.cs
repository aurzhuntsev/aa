using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace AudioMark.Core.Settings
{
    public class OverridableSettings<T> where T: new()
    {
        public bool Overriden { get; set; }

        private T _globalSettings;
        private Lazy<T> _overridenValue;

        public T Value
        {
            get
            {
                if (Overriden)
                {
                    return _overridenValue.Value;
                }

                return CloneGlobalSettings();
            }
        }

        public OverridableSettings(T globalSettings)
        {
            _globalSettings = globalSettings;
             _overridenValue = new Lazy<T>(CloneGlobalSettings);
        }               
        
        private T CloneGlobalSettings()
        {
            /* TODO: Implement something better at some point */
            var jsonValue = JsonSerializer.Serialize<T>(_globalSettings);
            return JsonSerializer.Deserialize<T>(jsonValue);
        }
    }
}
