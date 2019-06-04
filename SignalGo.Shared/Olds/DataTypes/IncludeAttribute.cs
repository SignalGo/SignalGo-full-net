using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.DataTypes
{
    /// <summary>
    /// include properties of incoming and outgoing calls from client to server and server to client
    /// include = take only exchange type
    /// </summary>
    public class IncludeAttribute : CustomDataExchangerAttribute
    {
        public override LimitExchangeType LimitationMode { get; set; } = LimitExchangeType.Both;
        public override CustomDataExchangerType ExchangeType { get; set; } = CustomDataExchangerType.TakeOnly;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties">properties to include</param>
        public IncludeAttribute(params string[] properties)
        {
            Properties = properties;
        }
    }
}
