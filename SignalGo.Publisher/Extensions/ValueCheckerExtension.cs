using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SignalGo.Publisher.Extensions
{
    /// <summary>
    /// check variable have any value and not null,empty,zero, ...
    /// </summary>
    public static class ValueCheckerExtension
    {
        public static bool HasValue(this string str)
        {
            if (string.IsNullOrEmpty(str.ToString()))
                return false;
            else
                return true;
        }
        public static bool HasValue(this int number)
        {
            if (number <= 0)
                return false;
            else
                return true;
        }
        public static bool HasCompleted<T>(this Task<T> task)
        {
            if (task.IsCompleted)
                return true;
            else
                return false;
        }
        public static bool HasValue<T>(this Task<T> obj)
        {
            if (obj != null && obj.Result != null)
                return true;
            return false;
        }
        public static bool HasValue(this object obj)
        {
            if (obj == null)
                return false;
            else
                return true;
        }
        public static bool HasValue<T>(this List<T> list)
        {
            if (list.Any() || list.Count > 0)
                return true;
            return false;
        }
        public static bool HasValue<T>(this ObservableCollection<T> collection)
        {
            if (collection.Any() || collection.Count > 0)
                return true;
            return false;
        }
        public static bool HasValue<T>(this List<T> collection, Func<T, bool> condition)
        {
            if (collection == null)
                return false;
            if (collection.Any())
                return true;
            else
                return false;
        }

    }
}
