using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

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
        /// default constructor for data exchanger
        /// </summary>
        /// <param name="type">type of your class to ignore or take properties for serialize</param>
        /// <param name="properties">list of types you want to take methods of that types</param>
        public CustomDataExchanger(Type type, params Type[] properties)
        {
            Type = type;
            Properties = GetMethods(properties).ToArray();
        }

        /// <summary>
        /// get list of methods of type
        /// </summary>
        /// <param name="types">your types</param>
        /// <returns>list of methods names</returns>
        public static List<string> GetMethods(Type[] types)
        {
            List<string> result = new List<string>();
            foreach (var serviceType in types)
            {
                foreach (var item in serviceType.GetInterfaces())
                {
                    result.AddRange(item.GetMethods().Select(x => x.Name));
                }
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                var parent = serviceType.GetTypeInfo().BaseType;
#else
            var parent = serviceType.BaseType;
#endif
                while (parent != null)
                {
                    result.AddRange(parent.GetMethods().Select(x => x.Name));
                    
                    foreach (var item in parent.GetInterfaces())
                    {
                        result.AddRange(item.GetMethods().Select(x => x.Name));
                    }
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                    parent = parent.GetTypeInfo().BaseType;
#else
                    parent = parent.BaseType;
#endif
                }
            }
            return result;
        }

        /// <summary>
        /// you can customize enable and disable ignorable
        /// </summary>
        /// <returns>if you return false system force skip to ignore property</returns>
        public virtual bool IsEnabled(object client, object server, string propertyName, Type type)
        {
            return true;
        }
    }
}
