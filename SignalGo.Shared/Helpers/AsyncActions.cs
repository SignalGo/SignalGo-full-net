using SignalGo.Shared.Log;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SignalGo.Shared
{
    /// <summary>
    /// ConcurrentDictionary extension helper
    /// </summary>
    public static class ConcurrentDictionaryEx
    {
        /// <summary>
        /// remove a dictionary key
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="self"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> self, TKey key)
        {
            return ((IDictionary<TKey, TValue>)self).Remove(key);
        }
    }

    /// <summary>
    /// run Action on same thread
    /// </summary>
    public static class AsyncActions
    {
        /// <summary>
        /// if actions return exceptions
        /// </summary>
        public static Action<Exception> OnActionException { get; set; }
        /// <summary>
        /// Run action on thread
        /// </summary>
        /// <param name="action">your action</param>
        public static void Run(Action action, Action<Exception> onException = null)
        {
            Thread thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    onException?.Invoke(ex);
                    AutoLogger.LogError(ex, "AsyncActions Run");
                    OnActionException?.Invoke(ex);
                }
            })
            {
                IsBackground = false
            };
            thread.Start();
        }
    }
}
