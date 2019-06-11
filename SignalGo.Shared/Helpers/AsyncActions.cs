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
        /// <param name="onException"></param>
        public static void RunOnUI(Action action, Action<Exception> onException = null)
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
                    onException?.Invoke(ex);
                }
            }, null);
        }
    }
}
