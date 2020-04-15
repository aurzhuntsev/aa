using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioMark.Core.Common
{
    public static class TypeExtensions
    {
        public static string GetStringAttributeValue<T>(this Type type, bool inherit = false) where T : StringAttribute
        {
            var attribute = type.GetCustomAttributes(typeof(T), inherit).FirstOrDefault();
            if (attribute == null)
            {
                return null;
            }

            return ((StringAttribute)attribute).Value;
        }        
    }
}
