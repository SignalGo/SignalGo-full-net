using SignalGo.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SignalGo.Accessibilities
{
    public static class DataFactory
    {
        internal static ConcurrentDictionary<Thread, ConcurrentDictionary<Type, object>> SingleToneByThreadItems = new ConcurrentDictionary<Thread, ConcurrentDictionary<Type, object>>();

        public static bool SetSingleToneByThread(object instance)
        {
            if (instance == null)
                throw new Exception("instance cannot be null!");
            if (SingleToneByThreadItems.TryGetValue(Thread.CurrentThread, out ConcurrentDictionary<Type, object> keyItems))
            {
                if (keyItems.TryGetValue(instance.GetType(), out object obj))
                {
                    return false;
                }
                keyItems[instance.GetType()] = instance;
            }
            else
            {
                var values = new ConcurrentDictionary<Type, object>();
                values.TryAdd(instance.GetType(), instance);
                SingleToneByThreadItems[Thread.CurrentThread] = values;
            }
            return true;
        }

        public static bool SetSingleToneByThread<T>()
        {
            var parameters = ConstructorFactory.GetSingleToneByThread<T>();
            object instance = parameters == null ? Activator.CreateInstance(typeof(T)): Activator.CreateInstance(typeof(T), parameters);
            return SetSingleToneByThread(instance);
        }

        public static T GetSingleToneByThread<T>()
            where T : class
        {
            if (SingleToneByThreadItems.TryGetValue(Thread.CurrentThread, out ConcurrentDictionary<Type, object> keyItems))
            {
                if (keyItems.TryGetValue(typeof(T), out object obj))
                {
                    return (T)obj;
                }
            }
            return null;
        }
    }
}
