using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.DataTypes
{
    /// <summary>
    /// exclude properties of outgoing calls from server to client
    /// exclude = ignore exchange type
    /// </summary>
    public class OutExcludeAttribute : CustomDataExchangerAttribute
    {
        public override LimitExchangeType LimitationMode { get; set; } = LimitExchangeType.OutgoingCall;
        public override CustomDataExchangerType ExchangeType { get; set; } = CustomDataExchangerType.Ignore;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties">properties to include</param>
        public OutExcludeAttribute(params string[] properties)
        {
            Properties = properties;
        }
    }
}
