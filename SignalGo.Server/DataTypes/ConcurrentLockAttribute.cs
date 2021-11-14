using System;

namespace SignalGo.Server.DataTypes
{
    /// <summary>
    /// type of lock system
    /// </summary>
    public enum ConcurrentLockType : byte
    {
        /// <summary>
        /// full lock for all clients call per server
        /// </summary>
        Full = 1,
        /// <summary>
        /// lock per client, one client cannot double call concurrent
        /// </summary>
        PerClient = 2,
        /// <summary>
        /// lock per ip address, one ip cannot double call concurrent
        /// </summary>
        PerIpAddress = 3,
        /// <summary>
        /// lock per method,multipe users cannot call one method concurrent
        /// </summary>
        PerMethod = 4,
        /// <summary>
        /// lock per signle instance service,users cannot call methods of service concurrent
        /// </summary>
        PerSingleInstanceService = 5,
        /// <summary>
        /// by a static string key
        /// </summary>
        Key = 6,
        /// <summary>
        /// per client session header
        /// </summary>
        PerSession = 7
    }
    /// <summary>
    /// lock method when multipe clients are calling
    /// </summary>
    public class ConcurrentLockAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public ConcurrentLockAttribute()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public ConcurrentLockAttribute(ConcurrentLockType type)
        {
            Type = type;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        public ConcurrentLockAttribute(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new Exception("key cannot be null as ConcurrentLockAttribute!");
            Type = ConcurrentLockType.Key;
            Key = key;
        }

        /// <summary>
        /// lock per value of parameter
        /// its just support for ConcurrentLockType.Full and parameter path
        /// </summary>
        /// <param name="type">Set type as PerParameterValue</param>
        /// <param name="parameterPath">Examples: "*/PropertyName" or "ParameterName/PropertyName"</param>
        public ConcurrentLockAttribute(ConcurrentLockType type, string parameterPath)
        {
            if (string.IsNullOrEmpty(parameterPath))
                throw new Exception("parameterPath cannot be null as ConcurrentLockAttribute!");
            Type = type;
            Key = parameterPath;
        }

        /// <summary>
        /// type of lock
        /// </summary>
        public ConcurrentLockType Type { get; set; }
        /// <summary>
        /// key of lock
        /// </summary>
        public string Key { get; set; }
    }
}
