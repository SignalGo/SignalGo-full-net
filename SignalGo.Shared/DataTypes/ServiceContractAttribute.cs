using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.DataTypes
{
    /// <summary>
    /// type of instance
    /// when cllient connect to servevr and registering service, service class get new instance
    /// </summary>
    public enum InstanceType
    {
        /// <summary>
        /// single instance for all of user
        /// </summary>
        SingleInstance = 1,
        /// <summary>
        /// create new instance per user connection
        /// </summary>
        MultipeInstance = 2,
    }

    /// <summary>
    /// service contract is communicate services between client and server
    /// </summary>
    public class ServiceContractAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public ServiceContractAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="instanceType"></param>
        public ServiceContractAttribute(string name, InstanceType instanceType)
        {
            Name = name;
            InstanceType = instanceType;
        }
        /// <summary>
        /// name of service
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// when cllient connect to server and registering a service, service class get new instance
        /// you can change the instance plane with this parameter
        /// </summary>
        public InstanceType InstanceType { get; set; } = InstanceType.MultipeInstance;
    }
}
