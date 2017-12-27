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
#if (!PORTABLE)
        static SynchronizationContext UIThread { get; set; }
        /// <summary>
        /// initialize ui thread
        /// </summary>
        public static void InitializeUIThread()
        {
            if (SynchronizationContext.Current == null)
                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            UIThread = SynchronizationContext.Current;
        }

        /// <summary>
        /// run your code on ui thread
        /// </summary>
        /// <param name="action"></param>
        public static void RunOnUI(Action action)
        {
            if (UIThread == null)
                throw new Exception("UI thread not initialized please call AsyncActions.InitializeUIThread in your ui thread to initialize");
            UIThread.Post((state) =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {

                }
            }, null);
        }
#endif
        /// <summary>
        /// if actions return exceptions
        /// </summary>
        public static Action<Exception> OnActionException { get; set; }
        /// <summary>
        /// Run action on thread
        /// </summary>
        /// <param name="action">your action</param>
        /// <param name="onException"></param>
#if (PORTABLE)
        public static System.Threading.Tasks.Task Run(Action action, Action<Exception> onException = null)
#else
        public static Thread Run(Action action, Action<Exception> onException = null)
#endif
        {
#if (PORTABLE)
            var thread = System.Threading.Tasks.Task.Factory.StartNew(() =>
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
              });
#else
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
#endif
            return thread;
        }

        /// <summary>
        /// run your code with async await
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static System.Threading.Tasks.Task RunAsync(Action action)
        {
            return System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                action();
            });
        }

        /// <summary>
        /// run your code with async await
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public static System.Threading.Tasks.Task<T> RunAsync<T>(Func<T> func)
        {
            return System.Threading.Tasks.Task<T>.Factory.StartNew(() =>
            {
                return func();
            });
        }
    }
}
