using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SignalGo.ServiceManager.Models
{
    public abstract class ServerProcessBaseInfo : IDisposable
    {
        public static Func<ServerProcessBaseInfo> Instance { get; set; }
        public Process BaseProcess { get; set; }

        public abstract void Dispose();

        public abstract void Start(string command, string assemblyPath, string shell = "/bin/bash");
    }
}
