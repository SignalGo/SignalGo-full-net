using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.DataTypes
{
    /// <summary>
    /// exclude properties of incoming and outgoing calls from client to server and server to client
    /// exclude = ignore exchange type
    /// </summary>
    public class ExcludeAttribute : CustomDataExchangerAttribute
    {
        public override LimitExchangeType LimitationMode { get; set; } = LimitExchangeType.Both;
        public override CustomDataExchangerType ExchangeType { get; set; } = CustomDataExchangerType.Ignore;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties">properties to include</param>
        public ExcludeAttribute(params string[] properties)
        {
            Properties = properties;
        }
    }
}
