using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.DataTypes
{
    /// <summary>
    /// include properties of outgoing calls from server to client
    /// include = take only exchange type
    /// </summary>
    public class OutIncludeAttribute : CustomDataExchangerAttribute
    {
        public override LimitExchangeType LimitationMode { get; set; } = LimitExchangeType.OutgoingCall;
        public override CustomDataExchangerType ExchangeType { get; set; } = CustomDataExchangerType.TakeOnly;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties">properties to include</param>
        public OutIncludeAttribute(params string[] properties)
        {
            Properties = properties;
        }
    }
}
