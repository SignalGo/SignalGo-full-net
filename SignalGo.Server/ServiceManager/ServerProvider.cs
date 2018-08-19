using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
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
    public class ServerProvider : UdpServiceBase
    {

        /// <summary>
        /// strat the server
        /// </summary>
        /// <param name="url">url of services</param>
        public void Start(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                throw new Exception("url is not valid");
            }
            else if (uri.Port <= 0)
            {
                throw new Exception("port is not valid");
            }
            if (string.IsNullOrEmpty(uri.AbsolutePath))
                throw new Exception("this path is not support,please set full path example: http://localhost:5050/SignalGo");

            ServerDataProvider.Start(this, uri.Port);
        }

        /// <summary>
        /// strat the server without manully register your services with call register services methods
        /// </summary>
        /// <param name="url">url of services</param>
        /// <param name="assemblies">add your assemblies they have servicecontract over classes</param>
        public void Start(string url, List<Assembly> assemblies)
        {
            Start(url);
            var assembly = Assembly.GetEntryAssembly();
            if (assemblies == null)
                assemblies = new List<Assembly>() { assembly };
            AutoRegisterServices(assemblies);

        }

        internal IEnumerable<Type> GetAllTypes(List<Assembly> assemblies)
        {
            foreach (var asm in assemblies)
            {
                foreach (var type in asm.GetTypes())
                {
                    yield return type;
                }
            }
        }

        internal Type GetMainInheritancedType(Type type, List<Type> allTypes)
        {
            return allTypes.FirstOrDefault(x => x != type && x.GetInterfaces().Any(y => y == type));
        }

        /// <summary>
        /// automatic find services and register
        /// </summary>
        /// <param name="assemblies">add your assemblies they have servicecontract over classes</param>
        public void AutoRegisterServices(List<Assembly> assemblies)
        {
            //List<Type> ServerServices = new List<Type>();
            //List<Type> ClientServices = new List<Type>();
            //List<Type> HttpServices = new List<Type>();
            //List<Type> StreamServices = new List<Type>();
            List<Type> AllTypes = GetAllTypes(assemblies).ToList();

            foreach (var type in AllTypes)
            {
                var attributes = type.GetCustomAttributes<ServiceContractAttribute>();
                if (attributes.Length > 0)
                {
                    foreach (var att in attributes)
                    {
                        if (att.ServiceType == ServiceType.ServerService)
                        {
                            //if (!ServerServices.Contains(type))
                            //{
                            //    if (type.GetIsInterface())
                            //    {
                            //        var find = GetMainInheritancedType(type, AllTypes);
                            //        if (find != null)
                            //            ServerServices.Add(find);
                            //    }
                            //    else
                            //        ServerServices.Add(type);
                            //}
                            RegisterServerService(type);
                        }
                        else if (att.ServiceType == ServiceType.ClientService)
                        {
                            //if (!ClientServices.Contains(type) && type.GetIsInterface())
                            //    ClientServices.Add(type);
                            RegisterServerService(type);
                        }
                        else if (att.ServiceType == ServiceType.HttpService)
                        {
                            //if (!HttpServices.Contains(type))
                            //{
                            //    if (type.GetIsInterface())
                            //    {
                            //        var find = GetMainInheritancedType(type, AllTypes);
                            //        if (find != null)
                            //            HttpServices.Add(find);
                            //    }
                            //    else
                            //        HttpServices.Add(type);
                            //}
                            RegisterServerService(type);
                        }
                        else if (att.ServiceType == ServiceType.StreamService)
                        {
                            RegisterServerService(type);
                            //if (!StreamServices.Contains(type))
                            //{
                            //    if (type.GetIsInterface())
                            //    {
                            //        var find = GetMainInheritancedType(type, AllTypes);
                            //        if (find != null)
                            //            StreamServices.Add(find);
                            //    }
                            //    else
                            //        StreamServices.Add(type);
                            //}
                        }
                        else if (att.ServiceType == ServiceType.OneWayService)
                        {
                            RegisterServerService(type);
                            //if (!StreamServices.Contains(type))
                            //{
                            //    if (type.GetIsInterface())
                            //    {
                            //        var find = GetMainInheritancedType(type, AllTypes);
                            //        if (find != null)
                            //            StreamServices.Add(find);
                            //    }
                            //    else
                            //        StreamServices.Add(type);
                            //}
                        }
                    }
                }
            }

            //if (ServerServices.Count > 0)
            //{
            //    Console.WriteLine("Registering ServerServices:");
            //    foreach (var item in ServerServices)
            //    {
            //        RegisterServerService(item);
            //        Console.WriteLine(item.FullName);
            //    }
            //}
            //            if (ClientServices.Count > 0)
            //            {
            //                Console.WriteLine("Registering ClientServices:");
            //                foreach (var item in ClientServices)
            //                {
            //#if (!NETSTANDARD1_6 && !NETCOREAPP1_1)
            //                    if (item.GetIsInterface())
            //                        RegisterClientServiceInterface(item);
            //                    else
            //#endif
            //                    RegisterClientService(item);
            //                    Console.WriteLine(item.FullName);
            //                }
            //            }

            //            if (HttpServices.Count > 0)
            //            {
            //                Console.WriteLine("Registering HttpServices:");
            //                foreach (var item in HttpServices)
            //                {
            //                    RegisterHttpService(item);
            //                    Console.WriteLine(item.FullName);
            //                }
            //            }
            //            if (StreamServices.Count > 0)
            //            {
            //                Console.WriteLine("Registering StreamServices:");
            //                foreach (var item in StreamServices)
            //                {
            //                    RegisterStreamService(item);
            //                    Console.WriteLine(item.FullName);
            //                }
            //            }
        }
        //public void StartWebSocket(string url)
        //{
        //    Uri uri = null;
        //    if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
        //    {
        //        throw new Exception("url is not valid");
        //    }
        //    else if (uri.Port <= 0)
        //    {
        //        throw new Exception("port is not valid");
        //    }

        //    //IPHostEntry Host = Dns.GetHostEntry(uri.Host);
        //    //IPHostEntry server = Dns.Resolve(uri.Host);
        //    ConnectWebSocket(uri.Port, new string[] { uri.AbsolutePath });
        //}
    }
}
