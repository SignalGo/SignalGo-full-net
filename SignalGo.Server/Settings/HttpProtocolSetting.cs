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
        /// <summary>
        /// if http protocolsetting is https
        /// </summary>
        public bool IsHttps { get; set; }
        /// <summary>
        /// X509Certificate
        /// </summary>
        public System.Security.Cryptography.X509Certificates.X509Certificate X509Certificate { get; set; }
    }
}
