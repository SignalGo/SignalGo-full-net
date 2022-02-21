using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.DataTypes
{
    /// <summary>
    /// exclude properties of incoming calls from client to server
    /// exclude = ignore exchange type
    /// </summary>
    public class InExcludeAttribute : CustomDataExchangerAttribute
    {
        public override LimitExchangeType LimitationMode { get; set; } = LimitExchangeType.IncomingCall;
        public override CustomDataExchangerType ExchangeType { get; set; } = CustomDataExchangerType.Ignore;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties">properties to include</param>
        public InExcludeAttribute(params string[] properties)
        {
            Properties = properties;
        }
    }
}
