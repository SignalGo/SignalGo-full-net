using SignalGo.Server.Models;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SignalGo.Server.DataTypes
{
    /// <summary>
    /// Security contract attribute for using check permission before call methods
    /// </summary>
    public abstract class SecurityContractAttribute : Attribute
    {
        ///// <summary>
        ///// call your security method for http calls
        ///// </summary>
        ///// <param name="client">client if that called method</param>
        ///// <param name="controller">controller if available</param>
        ///// <param name="serviceName">service name</param>
        ///// <param name="methodName">method name</param>
        ///// <param name="address">address</param>
        ///// <param name="parameters">parameters</param>
        ///// <returns></returns>
        //public abstract bool CheckHttpPermission(ClientInfo client, IHttpClientInfo controller, string serviceName, string methodName, string address, List<object> parameters);
        ///// <summary>
        ///// result of your security method for http calls when access dined
        ///// </summary>
        ///// <param name="client">client if that called method</param>
        ///// <param name="controller">controller if available</param>
        ///// <param name="serviceName">service name</param>
        ///// <param name="methodName">method name</param>
        ///// <param name="address">address</param>
        ///// <param name="parameters">parameters</param>
        ///// <returns></returns>
        //public abstract object GetHttpValueWhenDenyPermission(ClientInfo client, IHttpClientInfo controller, string serviceName, string methodName, string address, List<object> parameters);

        /// <summary>
        /// call your check security method
        /// </summary>
        /// <param name="client">who client is calling your method</param>
        /// <param name="service">your service instance class that used this attribute</param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns>if client have permission return true else false</returns>
        public abstract bool CheckPermission(ClientInfo client, object service, MethodInfo method, List<object> parameters);
        /// <summary>
        /// after return false when calling CheckPermission server call this method for send data to client
        /// </summary>
        /// <param name="client">who client is calling your method</param>
        /// <param name="service">your service instance class that used this attribute</param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns>if your method is not void you can return a value to client</returns>
        public abstract object GetValueWhenDenyPermission(ClientInfo client, object service, MethodInfo method, List<object> parameters);
    }
}
