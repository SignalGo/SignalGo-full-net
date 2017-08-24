#if (!NET35)
using System;
using System.Collections.Generic;
using System.Dynamic;
using SignalGo.Client;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SignalGo.Client
{
    /// <summary>
    /// helper of dynamic calls
    /// </summary>
    public class DynamicServiceObject : DynamicObject, OperationCalls
    {
        /// <summary>
        /// service name
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// connector to call methods
        /// </summary>
        public ConnectorBase Connector { get; set; }

        /// <summary>
        /// types can return of method names
        /// </summary>
        public Dictionary<string, Type> ReturnTypes = new Dictionary<string, Type>();

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var type = ReturnTypes[binder.Name];
            if (type == typeof(void))
            {
                this.SendDataNoParam(binder.Name, ServiceName, args);
                result = null;
            }
            else
            {
                result = Newtonsoft.Json.JsonConvert.DeserializeObject(this.SendDataNoParam(binder.Name, ServiceName, args).ToString(), type);
            }
            return true;
        }

        /// <summary>
        /// initialize type to returnTypes
        /// </summary>
        /// <param name="type"></param>
        public void InitializeInterface(Type type)
        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            foreach (var item in type.GetTypeInfo().GetMethods())
#else
            foreach (var item in type.GetMethods())
#endif
            {
                ReturnTypes.Add(item.Name, item.ReturnType);
            }
        }
    }
}
#endif
