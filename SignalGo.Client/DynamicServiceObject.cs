#if (!NET35)
using System;
using System.Collections.Generic;
using System.Dynamic;
using SignalGo.Client;
using System.Linq;
using System.Text;
using System.Reflection;
using SignalGo.Shared.Helpers;

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
                var data = this.SendDataNoParam(binder.Name, ServiceName, args).ToString();
                result = Newtonsoft.Json.JsonConvert.DeserializeObject(data, type);
            }
            return true;
        }

        /// <summary>
        /// initialize type to returnTypes
        /// </summary>
        /// <param name="type"></param>
        public void InitializeInterface(Type type)
        {
            var items = type.GetListOfMethods();
            foreach (var item in items)
            {
                ReturnTypes.Add(item.Name, item.ReturnType);
            }
        }
    }
}
#endif
