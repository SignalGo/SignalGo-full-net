using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Server.Helpers
{
    public interface ICustomAsyncDisposable
    {
        /// <summary>
        /// is your object disposed
        /// </summary>
        bool IsDisposed { get; set; }
        /// <summary>
        /// when your object is not disposed automaticaly it will thro exception
        /// 
        /// </summary>
        bool IsThrowWhenNotDisposed { get; set; }
        /// <summary>
        /// custom dispose method
        /// </summary>
        /// <returns></returns>
        Task CustomDisposeAsync();
    }
}
