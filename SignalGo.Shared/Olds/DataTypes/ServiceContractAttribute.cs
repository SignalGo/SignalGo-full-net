using System;
using System.Linq;

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
        /// service class is implemented server methods
        /// </summary>
        ServerService,
        /// <summary>
        /// service class is implemented client methods
        /// </summary>
        ClientService,
        /// <summary>
        /// service class is implemented server methods that support httpCalls
        /// </summary>
        HttpService,
        /// <summary>
        ///  service class is implemented server stream methods
        /// </summary>
        StreamService,
        /// <summary>
        /// one way signal go service that client will call then close
        /// </summary>
        OneWayService,
    }

    /// <summary>
    /// service contract is communicate services between client and server
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

    /// <summary>
    /// extension of service contract attribute
    /// </summary>
    public static class ServiceContractExtensions
    {
        /// <summary>
        /// get service name from ServiceContractAttribute
        /// </summary>
        /// <param name="serviceContract"></param>
        /// <returns></returns>
        public static string GetServiceName(this ServiceContractAttribute serviceContract, bool isClient)
        {
            if (isClient)
                return serviceContract.Name.ToLower();
            if (serviceContract.ServiceType == ServiceType.HttpService)
                return serviceContract.Name.ToLower();
            return (serviceContract.Name + serviceContract.ServiceType.ToString()).ToLower();
        }

        /// <summary>
        /// get server service name from ServiceContractAttribute
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetServerServiceName(this Type type, bool isClient)
        {
            ServiceContractAttribute serviceContract = type.GetServerServiceAttribute();
            if (isClient)
                return serviceContract.Name.ToLower();
            if (serviceContract.ServiceType == ServiceType.HttpService)
                return serviceContract.Name.ToLower();
            return (serviceContract.Name + serviceContract.ServiceType.ToString()).ToLower();
        }

        /// <summary>
        /// get client service name from ServiceContractAttribute
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetClientServiceName(this Type type, bool isClient)
        {
            ServiceContractAttribute serviceContract = type.GetClientServiceAttribute();
            if (isClient)
                return serviceContract.Name.ToLower();
            if (serviceContract.ServiceType == ServiceType.HttpService)
                return serviceContract.Name.ToLower();
            return (serviceContract.Name + serviceContract.ServiceType.ToString()).ToLower();
        }

        /// <summary>
        /// check if type is server service
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsServerService(this Type type)
        {
            return type.GetCustomAttributes<ServiceContractAttribute>(true).Any(x => x.ServiceType == ServiceType.ServerService);
        }

        public static bool HasServiceAttribute(this Type type)
        {
            return type.GetCustomAttributes<ServiceContractAttribute>(true).Count() > 0;
        }

        /// <summary>
        /// check if type is client service
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsClientService(this Type type)
        {
            return type.GetCustomAttributes<ServiceContractAttribute>(true).Any(x => x.ServiceType == ServiceType.ClientService);
        }

        /// <summary>
        /// check if type is client service
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsHttpService(this Type type)
        {
            var attributes = type.GetCustomAttributes<ServiceContractAttribute>(true);
            //if attributes.length == 0 that is rest api service
            return attributes.Any(x => x.ServiceType == ServiceType.HttpService) || attributes.Length == 0;
        }

        /// <summary>
        /// get all server service attribute from ServiceContractAttribute
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ServiceContractAttribute[] GetServiceContractAttributes(this Type type)
        {
            ServiceContractAttribute[] serviceContract = type.GetCustomAttributes<ServiceContractAttribute>(true);
            if (serviceContract.Length == 0)
                throw new Exception("your server class must have ServiceContract attribute that have ServiceType == ServiceType.SeverService parameter");
            return serviceContract;
        }

        /// <summary>
        /// get server service attribute from ServiceContractAttribute
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ServiceContractAttribute GetServerServiceAttribute(this Type type)
        {
            ServiceContractAttribute serviceContract = type.GetCustomAttributes<ServiceContractAttribute>(true).Where(x => x.ServiceType == ServiceType.ServerService || x.ServiceType == ServiceType.HttpService || x.ServiceType == ServiceType.StreamService || x.ServiceType == ServiceType.OneWayService).FirstOrDefault();
            if (serviceContract == null)
                throw new Exception("your server class must have ServiceContract attribute that have ServiceType == ServiceType.SeverService parameter");
            return serviceContract;
        }

        /// <summary>
        /// get server service attribute from ServiceContractAttribute
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ServiceContractAttribute GetServerServiceAttribute(this Type type, string serviceName, bool isClient)
        {
            ServiceContractAttribute serviceContract = type.GetCustomAttributes<ServiceContractAttribute>(true).Where(x => x.ServiceType == ServiceType.ServerService || x.ServiceType == ServiceType.HttpService || x.ServiceType == ServiceType.StreamService || x.ServiceType == ServiceType.OneWayService).Where(x => x.GetServiceName(isClient).Equals(serviceName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (serviceContract == null)
                throw new Exception("your server class must have ServiceContract attribute that have ServiceType == ServiceType.SeverService parameter");
            return serviceContract;
        }
        /// <summary>
        /// get client service attribute from ServiceContractAttribute
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ServiceContractAttribute GetClientServiceAttribute(this Type type)
        {
            ServiceContractAttribute serviceContract = type.GetCustomAttributes<ServiceContractAttribute>(true).Where(x => x.ServiceType == ServiceType.ClientService).FirstOrDefault();
            if (serviceContract == null)
                throw new Exception("your client class must have ServiceContract attribute that have ServiceType == ServiceType.ClientService parameter");
            return serviceContract;
        }
    }
}
