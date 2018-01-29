using SignalGo.Server.Models;
using SignalGo.Shared.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SignalGo.Server.DataTypes
{
    /// <summary>
    /// Security contract attribute for using check permission before call methods
    /// </summary>
    public abstract class SecurityContractAttribute : Attribute
    {
        public abstract bool CheckHttpPermission(ClientInfo client, HttpRequestController controller, string serviceName, string methodName, string address, List<object> parameters);
        public abstract ActionResult GetHttpValueWhenDenyPermission(ClientInfo client, HttpRequestController controller, string serviceName, string methodName, string address, List<object> parameters);

        /// <summary>
        /// call your check security method
        /// </summary>
        /// <param name="client">who client is calling your method</param>
        /// <param name="service">your service instance class that used this attribute</param>
        /// <returns>if client have permission return true else false</returns>
        public abstract bool CheckPermission(ClientInfo client, object service, MethodInfo method, List<object> parameters);
        /// <summary>
        /// after return false when calling CheckPermission server call this method for send data to client
        /// </summary>
        /// <param name="client">who client is calling your method</param>
        /// <param name="service">your service instance class that used this attribute</param>
        /// <returns>if your method is not void you can return a value to client</returns>
        public abstract object GetValueWhenDenyPermission(ClientInfo client, object service, MethodInfo method, List<object> parameters);
    }
}
