using System;
using System.Diagnostics;

namespace SignalGo.ServiceManager.Core.Models
{
    public abstract class ServerProcessBaseInfo : IDisposable
    {
        public static Func<ServerProcessBaseInfo> Instance { get; set; }
        public Process BaseProcess { get; set; }

        public abstract void Dispose();

        public abstract void Start(string command, string assemblyPath, string shell = "/bin/bash");
    }
}
