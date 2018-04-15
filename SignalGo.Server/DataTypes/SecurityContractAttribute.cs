using SignalGo.Server.Models;
using SignalGo.Shared.Http;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SignalGo.Server.DataTypes
{
    /// <summary>
    /// Security contract attribute used to check permission before call methods
    /// </summary>
    public abstract class SecurityContractAttribute : Attribute
    {
        /// <summary>
        /// Call your security method for http calls
        /// </summary>
        /// <param name="client">client that called method</param>
        /// <param name="controller">controller if available</param>
        /// <param name="serviceName">service name</param>
        /// <param name="methodName">method name</param>
        /// <param name="address">address</param>
        /// <param name="parameters">parameters</param>
        /// <returns></returns>
        public abstract bool CheckHttpPermission(ClientInfo client, IHttpClientInfo controller, string serviceName, string methodName, string address, List<object> parameters);
        /// <summary>
        /// The result of your security method for http calls when access is denied
        /// </summary>
        /// <param name="client">client that called method</param>
        /// <param name="controller">controller if available</param>
        /// <param name="serviceName">service name</param>
        /// <param name="methodName">method name</param>
        /// <param name="address">address</param>
        /// <param name="parameters">parameters</param>
        /// <returns></returns>
        public abstract object GetHttpValueWhenDenyPermission(ClientInfo client, IHttpClientInfo controller, string serviceName, string methodName, string address, List<object> parameters);

        /// <summary>
        /// Call your check security method
        /// </summary>
        /// <param name="client">client that is calling your method</param>
        /// <param name="service">the service instance class that used this attribute</param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns>if client has permission return true, else false</returns>
        public abstract bool CheckPermission(ClientInfo client, object service, MethodInfo method, List<object> parameters);
        /// <summary>
        /// If CheckPermission returns false, the server calls this method to send data to client
        /// </summary>
        /// <param name="client">client that is calling your method</param>
        /// <param name="service">the service instance class that used this attribute</param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns>if your method is not void, you can return a value to client</returns>
        public abstract object GetValueWhenDenyPermission(ClientInfo client, object service, MethodInfo method, List<object> parameters);
    }
}
