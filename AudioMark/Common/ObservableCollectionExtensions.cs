using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace AudioMark.Common
{
    public static class ObservableCollectionExtensions
    {
        public static void SetOrFirst<T>(this ObservableCollection<T> collection, Action<T> setter, T setting)
        {
            if (collection != null)
            {
                if (collection.Any(item => item.Equals(setting)))
                {
                    setter(setting);
                }
                else if (collection.Count > 0)
                {
                    setter(collection[0]);
                }
            }
        }
    }
}
