using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;

namespace SignalGo.Server.Helpers
{
    public static class ConcurrentObjects
    {
        static ConcurrentDictionary<string, SemaphoreSlim> IPAddressLocks { get; set; } = new ConcurrentDictionary<string, SemaphoreSlim>();
        static ConcurrentDictionary<MethodInfo, SemaphoreSlim> MethodsLocks { get; set; } = new ConcurrentDictionary<MethodInfo, SemaphoreSlim>();
        static ConcurrentDictionary<object, SemaphoreSlim> InstancesLocks { get; set; } = new ConcurrentDictionary<object, SemaphoreSlim>();
        static ConcurrentDictionary<string, SemaphoreSlim> StringValuesLocks { get; set; } = new ConcurrentDictionary<string, SemaphoreSlim>();
        static ConcurrentDictionary<object, SemaphoreSlim> ParameterValuesLocks { get; set; } = new ConcurrentDictionary<object, SemaphoreSlim>();
        static ConcurrentDictionary<string, ConcurrentDictionary<object, SemaphoreSlim>> CustomParameterValuesLocks { get; set; } = new ConcurrentDictionary<string, ConcurrentDictionary<object, SemaphoreSlim>>();

        public static SemaphoreSlim GetIpObject(string key)
        {
            if (IPAddressLocks.TryGetValue(key, out SemaphoreSlim semaphoreSlim))
            {
                return semaphoreSlim;
            }
            else
            {
                semaphoreSlim = new SemaphoreSlim(1);
                if (!IPAddressLocks.TryAdd(key, semaphoreSlim))
                    return GetIpObject(key);
                return semaphoreSlim;
            }
        }

        public static SemaphoreSlim GetMethodObject(MethodInfo key)
        {
            if (MethodsLocks.TryGetValue(key, out SemaphoreSlim semaphoreSlim))
            {
                return semaphoreSlim;
            }
            else
            {
                semaphoreSlim = new SemaphoreSlim(1);
                if (!MethodsLocks.TryAdd(key, semaphoreSlim))
                    return GetMethodObject(key);
                return semaphoreSlim;
            }
        }

        public static SemaphoreSlim GetInstanceObject(object key)
        {
            if (InstancesLocks.TryGetValue(key, out SemaphoreSlim semaphoreSlim))
            {
                return semaphoreSlim;
            }
            else
            {
                semaphoreSlim = new SemaphoreSlim(1);
                if (!InstancesLocks.TryAdd(key, semaphoreSlim))
                    return GetInstanceObject(key);
                return semaphoreSlim;
            }
        }

        public static SemaphoreSlim GetParameterValue(object key)
        {
            if (ParameterValuesLocks.TryGetValue(key, out SemaphoreSlim semaphoreSlim))
            {
                return semaphoreSlim;
            }
            else
            {
                semaphoreSlim = new SemaphoreSlim(1);
                if (!ParameterValuesLocks.TryAdd(key, semaphoreSlim))
                    return GetParameterValue(key);
                return semaphoreSlim;
            }
        }

        public static SemaphoreSlim GetCustomParameterValue(string firstKey, object secondKey)
        {
            if (CustomParameterValuesLocks.TryGetValue(firstKey, out ConcurrentDictionary<object, SemaphoreSlim> items))
            {
                if (items.TryGetValue(secondKey, out SemaphoreSlim semaphoreSlim))
                {
                    return semaphoreSlim;
                }
                else
                {
                    semaphoreSlim = new SemaphoreSlim(1);
                    if (!items.TryAdd(secondKey, semaphoreSlim))
                        return GetCustomParameterValue(firstKey, secondKey);
                    return semaphoreSlim;
                }
            }
            else
            {
                items = new ConcurrentDictionary<object, SemaphoreSlim>();
                CustomParameterValuesLocks.TryAdd(firstKey, items);
                return GetCustomParameterValue(firstKey, secondKey);
            }
        }

        public static void RemoveParameterValue(object key)
        {
            ParameterValuesLocks.TryRemove(key, out _);
        }

        public static SemaphoreSlim GetStringValuesLocks(string parameterValue)
        {
            if (StringValuesLocks.TryGetValue(parameterValue, out SemaphoreSlim semaphoreSlim))
            {
                return semaphoreSlim;
            }
            else
            {
                semaphoreSlim = new SemaphoreSlim(1);
                if (!StringValuesLocks.TryAdd(parameterValue, semaphoreSlim))
                    return GetStringValuesLocks(parameterValue);
                return semaphoreSlim;
            }
        }
    }
}
