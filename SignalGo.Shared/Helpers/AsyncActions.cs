// Licensed to the ali.visual.studio@gmail.com under one or more agreements.
// The license this file to you under the GNU license.
// See the LICENSE file in the project root for more information.
//https://github.com/Ali-YousefiTelori
//https://github.com/SignalGo/SignalGo-full-net

using SignalGo.Shared.Logs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace SignalGo.Shared
{
    /// <summary>
    /// run Action on same thread
    /// </summary>
    public static class AsyncActions
    {
        /// <summary>
        /// log somethings to files
        /// </summary>
        public static AutoLogger AutoLogger { get; set; } = new AutoLogger() { FileName = "AsyncActions Logs.log" };
#if (!PORTABLE)
        /// <summary>
        /// user interface thread
        /// </summary>
        private static SynchronizationContext UIThread { get; set; }
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
                    AutoLogger.LogError(ex, "AsyncActions RunOnUI");
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
        public static void Run(Action action, Action<Exception> onException = null)
        {
#if (NET35 || NET40)
            ThreadPool.QueueUserWorkItem(RunAction, null);
            void RunAction(object state)
#else
            System.Threading.Tasks.Task.Run(new Action(RunAction));
            void RunAction()
#endif
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    try
                    {
                        onException?.Invoke(ex);
                        AutoLogger.LogError(ex, "AsyncActions Run");
                        OnActionException?.Invoke(ex);
                    }
                    catch (Exception ex2)
                    {
                        AutoLogger.LogError(ex2, "AsyncActions Run 2");

                    }
                }
            }
        }
        /// <summary>
        /// start new thread and run your action on new thread
        /// </summary>
        /// <param name="action"></param>
        /// <param name="onException"></param>
        /// <returns></returns>
        public static Thread StartNew(Action action, Action<Exception> onException = null)
        {
            Thread thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    try
                    {
                        onException?.Invoke(ex);
                        AutoLogger.LogError(ex, "AsyncActions Run");
                        OnActionException?.Invoke(ex);
                    }
                    catch (Exception ex2)
                    {
                        AutoLogger.LogError(ex2, "AsyncActions Run 2");

                    }
                }
            });
            thread.IsBackground = true;
            thread.Start();
            return thread;
        }
    }
}
