using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SignalGo.Server.Helpers
{
    public static class ConcurrentObjects
    {
        static ConcurrentDictionary<string, SemaphoreSlim> IPAddressLocks { get; set; } = new ConcurrentDictionary<string, SemaphoreSlim>();
        static ConcurrentDictionary<MethodInfo, SemaphoreSlim> MethodsLocks { get; set; } = new ConcurrentDictionary<MethodInfo, SemaphoreSlim>();
        static ConcurrentDictionary<object, SemaphoreSlim> InstancesLocks { get; set; } = new ConcurrentDictionary<object, SemaphoreSlim>();

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
    }
}
