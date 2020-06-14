using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Publisher.Engine.Models
{
    public enum ClientPermissionMode
    {
        //فقط کلاینت بتواند آپلود کند
        Upload = 1,
        //فقط کلاینت بتواند دانلود کند 
        Download = 2,
        //هر دو
        Both = 3
    }

    public class AppSettings
    {
        public string Name { get; set; }
        public string ServerAddress { get; set; }
        public string CurrentPassword { get; set; }
        public string CurrentSyncFolderPath { get; set; }
        public ClientPermissionMode CurrentClientPermissionMode { get; set; }
        public bool IsSubtree { get; set; }
        public string RunBeforUpdate { get; set; }
        public string RunAfterUpdate { get; set; }
        public string KillBeforUpdate { get; set; }
        public string KillAfterUpdate { get; set; }
        public string IgnoreFileNames { get; set; }
        public bool IsSkipRemovedItems { get; set; }
    }
}
