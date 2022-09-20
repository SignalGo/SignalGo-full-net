using SignalGo.Publisher.Shared.Models;
using SignalGo.Shared.Log;
using System;
using System.Collections.Generic;
using System.Text;

namespace SignalGo.Publisher.Shared.Helpers
{
    public static class AutoLoggerHelper
    {
        public static AutoLogger ServiceUpdateLogger { get; set; } = new AutoLogger() { DirectoryName = "", FileName = "Hosted service update logs.log" };
    }
}
