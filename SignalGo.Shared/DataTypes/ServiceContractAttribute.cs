using SignalGo.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SignalGo.Shared.DataTypes
{

    /// <summary>
    /// service contract is communicate services between client and server
    /// for register services with attributes
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    public class ServiceContractAttribute : Attribute
    {
        /// <summary>
        /// type of service
        /// </summary>
        public ServiceType ServiceType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="serviceType"></param>
        public ServiceContractAttribute(string name, ServiceType serviceType)
        {
            Name = name;
            ServiceType = serviceType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="serviceType"></param>
        /// <param name="instanceType"></param>
        public ServiceContractAttribute(string name, ServiceType serviceType, InstanceType instanceType)
        {
            Name = name;
            InstanceType = instanceType;
            ServiceType = serviceType;
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
