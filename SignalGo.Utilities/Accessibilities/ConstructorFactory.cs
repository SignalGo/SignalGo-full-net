using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SignalGo.Accessibilities
{
    public static class ConstructorFactory
    {
        internal static ConcurrentDictionary<Thread, ConcurrentDictionary<Type, object[]>> SingleToneByThreadItems = new ConcurrentDictionary<Thread, ConcurrentDictionary<Type, object[]>>();

        public static bool SetSingleToneByThread<T>(object[] parameters)
        {
            var type = typeof(T);
            if (SingleToneByThreadItems.TryGetValue(Thread.CurrentThread, out ConcurrentDictionary<Type, object[]> keyItems))
            {
                if (keyItems.TryGetValue(type, out object[] obj))
                {
                    return false;
                }
                keyItems[type] = parameters;
            }
            else
            {
                var values = new ConcurrentDictionary<Type, object[]>();
                values.TryAdd(type, parameters);
                SingleToneByThreadItems[Thread.CurrentThread] = values;
            }
            return true;
        }
    
        public static object[] GetSingleToneByThread<T>()
        {
            if (SingleToneByThreadItems.TryGetValue(Thread.CurrentThread, out ConcurrentDictionary<Type, object[]> keyItems))
            {
                if (keyItems.TryGetValue(typeof(T), out object[] obj))
                {
                    return obj;
                }
            }
            return null;
        }
    }
}
