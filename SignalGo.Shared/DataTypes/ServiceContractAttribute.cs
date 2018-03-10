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
    /// type of your service class that supported
    /// </summary>
    public enum ServiceType
    {
        /// <summary>
        /// service class is implimeneted server methods
        /// </summary>
        SeverService,
        /// <summary>
        /// service class is implimeneted client methods
        /// </summary>
        ClientService,
        /// <summary>
        /// service class is implimeneted server methods that support httpCalls
        /// </summary>
        HttpService
    }

    /// <summary>
    /// service contract is communicate services between client and server
    /// </summary>
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
        /// <param name="instanceType"></param>
        /// <param name="serviceType"></param>
        public ServiceContractAttribute(string name, InstanceType instanceType, ServiceType serviceType)
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

    public static class ServiceContractExtensions
    {
        public static string GetServerServiceName(this Type type)
        {
            var serviceContract = type.GetCustomAttributes<ServiceContractAttribute>(true).FirstOrDefault();
            if (serviceContract == null)
                throw new Exception("your server class must have ServiceContract attribute that have ServiceType == ServiceType.SeverService parameter");
            return serviceContract.Name;
        }

        public static string GetClientServiceName(this Type type)
        {
            var serviceContract = type.GetCustomAttributes<ServiceContractAttribute>(true).FirstOrDefault();
            if (serviceContract == null)
                throw new Exception("your client class must have ServiceContract attribute that have ServiceType == ServiceType.ClientService parameter");
            return serviceContract.Name;
        }

        public static ServiceContractAttribute GetServerServiceAttribute(this Type type)
        {
            var serviceContract = type.GetCustomAttributes<ServiceContractAttribute>(true).FirstOrDefault();
            if (serviceContract == null)
                throw new Exception("your server class must have ServiceContract attribute that have ServiceType == ServiceType.SeverService parameter");
            return serviceContract;
        }

        public static ServiceContractAttribute GetClientServiceAttribute(this Type type)
        {
            var serviceContract = type.GetCustomAttributes<ServiceContractAttribute>(true).FirstOrDefault();
            if (serviceContract == null)
                throw new Exception("your client class must have ServiceContract attribute that have ServiceType == ServiceType.ClientService parameter");
            return serviceContract;
        }
    }
}
