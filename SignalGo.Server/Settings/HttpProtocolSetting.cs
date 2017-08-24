using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Server.Settings
{
    /// <summary>
    /// Http protocol settings 
    /// </summary>
    public class HttpProtocolSetting
    {
        /// <summary>
        /// handle cross origin access from browser origin header
        /// </summary>
        public bool HandleCrossOriginAccess { get; set; }
    }
}
