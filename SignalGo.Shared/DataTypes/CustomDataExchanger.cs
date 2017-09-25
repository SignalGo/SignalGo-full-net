using System;
using System.Collections.Generic;
using System.Text;

namespace SignalGo.Shared.DataTypes
{
    /// <summary>
    /// system custom data exchanger help you to ignore or take custom properties to serialize data
    /// </summary>
    public class CustomDataExchanger : Attribute
    {
        /// <summary>
        /// type of data exchanger you need
        /// </summary>
        public CustomDataExchangerType CustomDataExchangerType { get; set; } = CustomDataExchangerType.Take;
        /// <summary>
        /// limitation mode in incoming call or outgoingCall
        /// </summary>
        public LimitExchangeType LimitationMode { get; set; } = LimitExchangeType.Both;
        /// <summary>
        /// type of your class to ignore or take properties for serialize
        /// </summary>
        public Type Type { get; set; }
        /// <summary>
        /// property names that you need to ignore or take for serialize
        /// </summary>
        public string[] Properties { get; set; }
        /// <summary>
        /// default constructor for data exchanger
        /// </summary>
        /// <param name="type">type of your class to ignore or take properties for serialize</param>
        /// <param name="properties">property names that you need to ignore or take for serialize</param>
        public CustomDataExchanger(Type type, params string[] properties)
        {
            Type = type;
            Properties = properties;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual bool IsEnabled(object client, object server, string propertyName, Type type)
        {
            return true;
        }
    }
}
