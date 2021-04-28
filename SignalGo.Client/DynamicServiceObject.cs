#if (!NET35)
using SignalGo.Client;
using SignalGo.Client.ClientManager;
using SignalGo.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SignalGo.Client
{
    /// <summary>
    /// helper of dynamic calls
    /// </summary>
    internal class DynamicServiceObject : DynamicObject, OperationCalls
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
            Type type = ReturnTypes[binder.Name];
            if (type == typeof(void))
            {
                this.SendDataNoParam(binder.Name, ServiceName, binder.MethodToParameters(x => ClientSerializationHelper.SerializeObject(x), args).ToArray());
                result = null;
            }
            else
            {
                string data = this.SendDataNoParam(binder.Name, ServiceName, binder.MethodToParameters(x => ClientSerializationHelper.SerializeObject(x), args).ToArray()).ToString();
                result = Newtonsoft.Json.JsonConvert.DeserializeObject(data, type, JsonSettingHelper.GlobalJsonSetting);
            }
            return true;
        }

        /// <summary>
        /// initialize type to returnTypes
        /// </summary>
        /// <param name="type"></param>
        public void InitializeInterface(Type type)
        {
            IEnumerable<MethodInfo> items = type.GetListOfMethods();
            foreach (MethodInfo item in items)
            {
                ReturnTypes.Add(item.Name, item.ReturnType);
            }
        }
    }


    /// <summary>
    /// helper of dynamic calls
    /// </summary>
    internal class DynamicServiceObjectWitoutInterface : DynamicObject, OperationCalls
    {
        /// <summary>
        /// service name
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// connector to call methods
        /// </summary>
        public ConnectorBase Connector { get; set; }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = this.SendDataNoParam(binder.Name, ServiceName, binder.MethodToParameters(x => ClientSerializationHelper.SerializeObject(x), args).ToArray());
            return true;
        }
    }
}
#endif
