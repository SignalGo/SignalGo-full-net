using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Client
{
    /// <summary>
    /// setting of client
    /// </summary>
    public class SettingInfo
    {
        /// <summary>
        /// if true, when client get timeout error when calling server method client force disposed from signalgo
        /// </summary>
        public bool IsDisposeClientWhenTimeout { get; set; }
    }
}
