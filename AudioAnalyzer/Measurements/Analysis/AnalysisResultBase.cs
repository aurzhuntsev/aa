using AudioMark.Core.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AudioMark.Core.Measurements.Analysis
{
    [Serializable]
    public abstract class AnalysisResultBase: IAnalysisResult
    {
        public Spectrum Data { get; set; }

        private string FormatValue(object value)
        {
            if (value is double d)
            {
                return d.ToString("F4");
            }

            return value.ToString();            
        }

        public Dictionary<string, string> ToDictionary()
        {
            var result = new Dictionary<string, string>();
            var props = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var prop in props)
            {
                var attributes = prop.GetCustomAttributes(true)
                                     .Where(attr => attr is StringAttribute)
                                     .ToList();
            
                foreach (var attr in attributes)
                {
                    if (attr is AnalysisResultFieldAttribute fieldAttr)
                    {
                        var value = prop.GetValue(this);
                        result.Add(fieldAttr.Value, FormatValue(value));
                    }
                    else if (attr is AnalysisResultDictionaryAttribute dictionaryAttr)
                    {
                        var dictionary = prop.GetValue(this) as IDictionary;
                        foreach (var key in dictionary.Keys)
                        {
                            result.Add(string.Format(dictionaryAttr.Value, key), FormatValue(dictionary[key]));
                        }
                    }
                }
            }

            return result;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var dictionary = ToDictionary();

            foreach (var key in dictionary.Keys)
            {
                sb.AppendLine($"{key}:\t{dictionary[key]}");
            }

            return sb.ToString();
        }
    }
}

