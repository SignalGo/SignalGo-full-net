using SignalGo.Server.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace SignalGo.Server.DataTypes
{
    /// <summary>
    /// Security contract attribute for using check permission before call methods
    /// </summary>
    public interface ISecurityContract
    {
        /// <summary>
        /// call your check security method
        /// </summary>
        /// <param name="client">who client is calling your method</param>
        /// <param name="service">your service instance class that used this attribute</param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns>if client have permission return true else false</returns>
        bool CheckPermission(ClientInfo client, object service, MethodInfo method, List<object> parameters);
        /// <summary>
        /// after return false when calling CheckPermission server call this method for send data to client
        /// </summary>
        /// <param name="client">who client is calling your method</param>
        /// <param name="service">your service instance class that used this attribute</param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns>if your method is not void you can return a value to client</returns>
        object GetValueWhenDenyPermission(ClientInfo client, object service, MethodInfo method, List<object> parameters);
    }

    /// <summary>
    /// Security contract attribute for using check permission before call methods
    /// </summary>
    public interface ISecurityContractAsync
    {
        /// <summary>
        /// call your check security method
        /// </summary>
        /// <param name="client">who client is calling your method</param>
        /// <param name="service">your service instance class that used this attribute</param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns>if client have permission return true else false</returns>
        Task<bool> CheckPermissionAsync(int taskId, ClientInfo client, object service, MethodInfo method, List<object> parameters);
        /// <summary>
        /// after return false when calling CheckPermission server call this method for send data to client
        /// </summary>
        /// <param name="client">who client is calling your method</param>
        /// <param name="service">your service instance class that used this attribute</param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns>if your method is not void you can return a value to client</returns>
        Task<object> GetValueWhenDenyPermissionAsync(int taskId, ClientInfo client, object service, MethodInfo method, List<object> parameters);
    }

    /// <summary>
    /// Security contract attribute for using check permission before call methods
    /// </summary>
    public abstract class SecurityContractAttribute : Attribute, ISecurityContract
    {
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

    /// <summary>
    /// Security contract attribute for using check permission before call methods
    /// </summary>
    public abstract class SecurityContractAsyncAttribute : Attribute, ISecurityContractAsync
    {
        /// <summary>
        /// call your check security method
        /// </summary>
        /// <param name="client">who client is calling your method</param>
        /// <param name="service">your service instance class that used this attribute</param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns>if client have permission return true else false</returns>
        public abstract Task<bool> CheckPermissionAsync(int taskId, ClientInfo client, object service, MethodInfo method, List<object> parameters);
        /// <summary>
        /// after return false when calling CheckPermission server call this method for send data to client
        /// </summary>
        /// <param name="client">who client is calling your method</param>
        /// <param name="service">your service instance class that used this attribute</param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns>if your method is not void you can return a value to client</returns>
        public abstract Task<object> GetValueWhenDenyPermissionAsync(int taskId, ClientInfo client, object service, MethodInfo method, List<object> parameters);
    }
}
