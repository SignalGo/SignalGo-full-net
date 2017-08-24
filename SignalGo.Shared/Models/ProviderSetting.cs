using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.Models
{
    /// <summary>
    /// server or client connector provider setting
    /// </summary>
    public class ProviderSetting
    {
        /// <summary>
        /// maximum value of timeout to wait for send data
        /// </summary>
        public TimeSpan SendDataTimeout { get; set; } = new TimeSpan(0, 0, 30);
        /// <summary>
        /// maximum value of timeout to wait for receive callbackinfo data
        /// </summary>
        public TimeSpan ReceiveDataTimeout { get; set; } = new TimeSpan(0, 0, 30);
        /// <summary>
        /// maximum send data block
        /// </summary>
        public uint MaximumSendDataBlock { get; set; } = uint.MaxValue;
        /// <summary>
        /// maximum receive data block
        /// </summary>
        public uint MaximumReceiveDataBlock { get; set; } = uint.MaxValue;
    }
}
