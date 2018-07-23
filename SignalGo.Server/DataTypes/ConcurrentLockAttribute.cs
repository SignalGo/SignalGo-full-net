using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        PerMethod = 4
    }
    /// <summary>
    /// lock method when multipe clients are calling
    /// </summary>
    public class ConcurrentLockAttribute : Attribute
    {
        public ConcurrentLockType Type { get; set; }
    }
}
