using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SignalGo.Server.ServiceManager
{
    /// <summary>
    /// signalGo server provider
    /// </summary>
    public class ServerProvider : ServerBase
    {
        /// <summary>
        /// start the server
        /// </summary>
        /// <param name="url">your server url exmaple : "http://localhost:80/any"</param>
        public void Start(string url)
        {
            //validate url
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                throw new Exception($"url is not valid {url}");
            }
            //validate port number
            else if (uri.Port <= 0)
            {
                throw new Exception($"port is not valid {url}");
            }

            //start the server listener
            ServerDataProvider.Start(this, uri.Port);
        }

        /// <summary>
        /// start the server without manully register your services with call register services methods
        /// </summary>
        /// <param name="url">your server url exmaple : "http://localhost:80/any"</param>
        /// <param name="assemblies">automaticaly add your server services and callbacks from an assembly without add them manualy</param>
        public void Start(string url, List<Assembly> assemblies)
        {
            Start(url);
            //validate assemblies
            if (assemblies == null || assemblies.Count == 0)
                throw new Exception("assemblies parameter is null or empty, please add your assemblies or call Start method without assebmlies");
            AutoRegisterServices(assemblies);

        }

        /// <summary>
        /// automatic find services and register
        /// </summary>
        /// <param name="assemblies">add your assemblies they have servicecontract over classes</param>
        internal void AutoRegisterServices(List<Assembly> assemblies)
        {
            //get all types of all assemblies
            IEnumerable<Type> allTypes = assemblies.GetAllTypes();

            foreach (Type type in allTypes)
            {
                //find servicecontract attributes over type
                ServiceContractAttribute[] attributes = type.GetCustomAttributes<ServiceContractAttribute>();
                if (attributes.Length > 0)
                {
                    foreach (ServiceContractAttribute att in attributes)
                    {
                        //when attribute is server services for duplex protocols like websockets,signalgo etc
                        if (att.ServiceType == ServiceType.ServerService)
                        {
                            RegisterServerService(type);
                        }
                        //when attribute is client services for duplex protocols like websockets,wss,signalgo etc
                        else if (att.ServiceType == ServiceType.ClientService)
                        {
                            RegisterClientService(type);
                        }
                        //when attribute is server services for one way protocols like http,https,signalgo etc
                        else if (att.ServiceType == ServiceType.HttpService)
                        {
                            RegisterServerService(type);
                        }
                        //when attribute is server stream services for duplex protocols like signalgo duplex
                        else if (att.ServiceType == ServiceType.StreamService)
                        {
                            RegisterServerService(type);
                        }
                        //when attribute is server services for oneway protocols like signalgo oneway
                        else if (att.ServiceType == ServiceType.OneWayService)
                        {
                            RegisterServerService(type);
                        }
                    }
                }
            }
        }
    }
}
