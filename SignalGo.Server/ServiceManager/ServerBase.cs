using SignalGo.Server.IO;
using SignalGo.Server.Models;
using SignalGo.Server.ServiceManager.Firewall;
using SignalGo.Server.ServiceManager.Versions;
using SignalGo.Shared;
using SignalGo.Shared.Converters;
using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.IO;
using SignalGo.Shared.IO.Compressions;
using SignalGo.Shared.Log;
using SignalGo.Shared.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SignalGo.Server.ServiceManager
{
    /// <summary>
    /// when method is calling or called you can log full data
    /// </summary>
    /// <param name="serviceName"></param>
    /// <param name="guid"></param>
    /// <param name="methodName"></param>
    /// <param name="parameters"></param>
    /// <param name="jsonParameters"></param>
    /// <param name="client"></param>
    /// <param name="json"></param>
    /// <param name="serverBase"></param>
    /// <param name="fileInfo"></param>
    /// <param name="canTakeMethod"></param>
    /// <param name="result"></param>
    public delegate void OnCallMethod(string serviceName, string guid, string methodName, SignalGo.Shared.Models.ParameterInfo[] parameters, string jsonParameters, ClientInfo client, string json, ServerBase serverBase, Shared.Http.HttpPostedFileInfo fileInfo, Func<MethodInfo, bool> canTakeMethod, object result);
    public delegate void OnInvokeMethod(ClientInfo client, ServerBase serverBase, MethodInfo method, object service, List<object> parametersValues, string guid);
    /// <summary>
    /// base of server
    /// </summary>
    public abstract class ServerBase : IDisposable, IValidationRuleInfo
    {
        /// <summary>
        /// password of code generation to get full generate of services and methods
        /// </summary>
        public string CodeGeneratorPassword { get; set; }
        static ServerBase()
        {
            WebcoketDatagramBase.Current = new WebcoketDatagram();
        }

        /// <summary>
        /// default constructor
        /// </summary>
        public ServerBase()
        {
            JsonSettingHelper.Initialize();
        }

        internal FirewallBase Firewall { get; set; } = new FirewallBase();
        /// <summary>
        /// get custom compression of client
        /// </summary>
        public Func<ICompression> GetCustomCompression { get; set; }
        /// <summary>
        /// current Compress mode
        /// </summary>
        public CompressMode CurrentCompressionMode { get; set; } = CompressMode.None;

        /// <summary>
        /// lock for this server
        /// </summary>
        public SemaphoreSlim LockWaitToRead { get; set; } = new SemaphoreSlim(1, 1);
        /// <summary>
        /// validation rules manager
        /// </summary>
        public ValidationRuleInfoManager ValidationRuleInfoManager { get; set; } = new ValidationRuleInfoManager();
        /// <summary>
        /// server data provider communication between client and server
        /// </summary>
        public IServerDataProvider ServerDataProvider { get; private set; } = new ServerDataProviderV4();
        /// <summary>
        /// log errors and warnings
        /// </summary>
        internal AutoLogger AutoLogger { get; private set; } = new AutoLogger() { FileName = "ServerBase Logs.log" };
        /// <summary>
        /// json serialize and deserialize error handling
        /// </summary>
        public JsonSettingHelper JsonSettingHelper { get; set; } = new JsonSettingHelper();

        /// <summary>
        /// Server is started or not
        /// </summary>
        public bool IsStarted { get; set; }

        /// <summary>
        /// The server settings
        /// </summary>
        public ProviderSetting ProviderSetting { get; set; } = new ProviderSetting();

        /// <summary>
        /// Action raised when when server disconnects
        /// </summary>
        public Action OnServerDisconnectedAction { get; set; }
        /// <summary>
        /// Action raised when server had internal exception
        /// </summary>
        public Action<Exception> OnServerInternalExceptionAction { get; set; }
        /// <summary>
        /// Action raised when a client connected successfully
        /// </summary>
        public Action<ClientInfo> OnClientConnectedAction { get; set; }
        /// <summary>
        /// after client disconnected
        /// </summary>
        public Action<ClientInfo> OnClientDisconnectedAction { get; set; }

        /// <summary>
        /// all of registred services like server services, client services, http services etc
        /// key is service name and value is service type
        /// </summary>
        internal ConcurrentDictionary<string, Type> RegisteredServiceTypes { get; set; } = new ConcurrentDictionary<string, Type>();

        /// <summary>
        /// list of clients
        /// key is clientId and value is client information
        /// </summary>
        public ConcurrentDictionary<string, ClientInfo> Clients { get; set; } = new ConcurrentDictionary<string, ClientInfo>();
        /// <summary>
        /// single instance services
        /// key is service name and value is instance of service
        /// </summary>
        internal ConcurrentDictionary<string, object> SingleInstanceServices { get; set; } = new ConcurrentDictionary<string, object>();
        /// <summary>
        /// multipe instance services that instance is per client
        /// first key is service name
        /// key is client info id and value is service names and instance of services
        /// </summary>
        internal ConcurrentDictionary<string, ConcurrentDictionary<string, object>> MultipleInstanceServices { get; set; } = new ConcurrentDictionary<string, ConcurrentDictionary<string, object>>();

        /// <summary>
        /// include models to server reference when client want to add or update service reference
        /// </summary>
        internal List<Assembly> ModellingReferencesAssemblies { get; set; } = new List<Assembly>();
        /// <summary>
        /// assemblies of test examples to manage examples of test cases and documantation
        /// </summary>
        internal List<Assembly> TestExampleAssemblies { get; set; } = new List<Assembly>();
        /// <summary>
        /// task ids of client calling methods etc
        /// key is task id and value is client id
        /// </summary>
        internal ConcurrentDictionary<int, string> TaskOfClientInfoes { get; set; } = new ConcurrentDictionary<int, string>();
        /// <summary>
        /// all of the clients service call methods from server those wait for recevice data from client and set result to task that waited
        /// </summary>
        internal ConcurrentDictionary<string, KeyValue<Type, object>> ClientServiceCallMethodsResult { get; set; } = new ConcurrentDictionary<string, KeyValue<Type, object>>();
        /// <summary>
        /// all of callbacks added for a client that will be remove after the client works done or client disposed
        /// </summary>
        internal ConcurrentDictionary<ClientInfo, List<Guid>> ClientServiceCallMethods { get; set; } = new ConcurrentDictionary<ClientInfo, List<Guid>>();
        /// <summary>
        /// before method calls
        /// </summary>
        public OnCallMethod OnBeforeCallMethodAction { get; set; }
        /// <summary>
        /// before invoke service method
        /// </summary>
        public OnInvokeMethod OnBeforeInvokeMethodAction { get; set; }
        /// <summary>
        /// after method calls
        /// </summary>
        public OnCallMethod OnAfterCallMethodAction { get; set; }

        /// <summary>
        /// you can edit http headers before send
        /// </summary>
        public Action<HttpClientInfo, ServerBase, object> OnBeforeSendHttpHeaderAction { get; set; }
        /// <summary>
        /// you can edit http response data before send
        /// </summary>
        public Func<HttpClientInfo, ServerBase, byte[], byte[]> OnBeforeSendHttpDataFunction { get; set; }
        /// <summary>
        /// Error handling methods that return types (not void)
        /// exception is exception trow
        /// Type is service Type
        /// MethodInfo method of service
        /// client is the calling client
        /// object is your return value
        /// ex, type, method, parameters, jsonParameter, client
        /// </summary>
        public Func<Exception, Type, MethodInfo, SignalGo.Shared.Models.ParameterInfo[], string, ClientInfo, object> ErrorHandlingFunction { get; set; }
        /// <summary>
        /// when server try to send response to client you can change response or customize it
        /// </summary>
        public Func<object, object, Type, MethodInfo, ClientInfo, string, object> OnSendResponseToClientFunction { get; set; }
        /// <summary>
        /// if you don't want to trhow exception when method have error validation you can fill this function to customize your result for client
        /// </summary>
        public Func<List<BaseValidationRuleInfoAttribute>, object, MethodInfo, object> ValidationResultHandlingFunction { get; set; }
        /// <summary>
        /// Register server service
        /// </summary>
        /// <param name="serviceType"></param>
        public void RegisterServerService<T>()
        {
            RegisterServerService(typeof(T), null);
        }

        /// <summary>
        /// register server sevice with name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        public void RegisterServerService<T>(string name)
        {
            name = name.ToLower();
            RegisterServerService(typeof(T), name);
        }

        /// <summary>
        /// Register server service
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="name">custom service name when servie hasn't attribute</param>
        public void RegisterServerService(Type serviceType, string name = null)
        {
            if (serviceType.HasServiceAttribute())
            {
                ServiceContractAttribute[] services = serviceType.GetServiceContractAttributes();
                foreach (ServiceContractAttribute service in services)
                {
                    name = service.GetServiceName(false).ToLower();
                    if (!RegisteredServiceTypes.ContainsKey(name))
                        RegisteredServiceTypes.TryAdd(name, serviceType);
                    else
                        throw new Exception($"service name {name} of type '{serviceType.FullName}' exist!");
                }
            }
            else if (!string.IsNullOrEmpty(name))
            {
                if (!RegisteredServiceTypes.ContainsKey(name))
                    RegisteredServiceTypes.TryAdd(name, serviceType);
                else
                    throw new Exception($"service name {name} of type '{serviceType.FullName}' exist!");
            }
            else
                throw new Exception("service name is null or empty!");
        }

        public void RegisterClientService<T>()
        {
            RegisterClientService(typeof(T));
        }

        public void RegisterClientService(Type serviceType)
        {
            ServiceContractAttribute service = serviceType.GetClientServiceAttribute();
            if (service != null)
            {
                string name = service.GetServiceName(false).ToLower();
                name = ServiceContractExtensions.GetServiceNameWithGeneric(serviceType, name);
                if (!RegisteredServiceTypes.ContainsKey(name))
                    RegisteredServiceTypes.TryAdd(name, serviceType);
            }
            else
            {
                throw new NotSupportedException("your service is not type of ServerService or HttpService or StreamService");
            }
        }

        /// <summary>
        /// get service route name by service type
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public string GetServiceName(Type serviceType)
        {
            if (serviceType == null)
                return null;
            if (serviceType.IsClientService())
            {
                ServiceContractAttribute service = serviceType.GetClientServiceAttribute();
                if (service != null)
                {
                    return service.GetServiceName(false);
                }
            }
            else if (serviceType.IsAnyOfServerService())
            {
                ServiceContractAttribute service = serviceType.GetServerServiceAttribute();
                if (service != null)
                {
                    return service.GetServiceName(false);
                }
            }
            else
                throw new Exception($"type of {serviceType} is not server or clientService I think you forgot to register it!");

            return null;
        }

        /// <summary>
        /// GetListOfRegistredTypes
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Type> GetListOfRegistredTypes()
        {
            return RegisteredServiceTypes.Values.Distinct();
        }

        public void AddAssemblyToSkipServiceReferences(Assembly assembly)
        {
            ModellingReferencesAssemblies.Add(assembly);
        }

        /// <summary>
        /// add test example assmblies by type
        /// </summary>
        /// <param name="types"></param>
        public void AddTestExamplesAssemblies(params Type[] types)
        {
#if (NETSTANDARD1_6)
            throw new NotSupportedException();
#else
            TestExampleAssemblies.AddRange(types.Select(x => x.Assembly));
#endif
        }

        internal void DisposeClient(ClientInfo client, TcpClient tcpClient, string reason)
        {
            try
            {
                //Console.WriteLine($"Client disposed " + (client == null ? "null!" : client.ClientId) + " reason: " + reason);
                if (client == null)
                {
                    if (tcpClient != null)
                    {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                        tcpClient.Dispose();
#else
                        tcpClient.Close();
#endif
                    }
                    return;
                }
                client.DisposeReason = reason;
                client.IsDisposed = true;
                try
                {
                    if (client.TcpClient != null)
                    {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                        client.TcpClient.Dispose();
#else
                        client.TcpClient.Close();
#endif
                    }
                }
                catch (Exception ex)
                {
                    AutoLogger.LogError(ex, $"{client.IPAddress} {client.ClientId} CloseCllient");
                }
                foreach (KeyValuePair<string, ConcurrentDictionary<string, object>> service in MultipleInstanceServices)
                {
                    foreach (KeyValuePair<string, object> clientInfo in service.Value.Where(x => x.Key == client.ClientId))
                    {
                        service.Value.Remove(clientInfo.Key);
                    }
                }

                client.OnDisconnected?.Invoke();
                Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(30));
                        Clients.Remove(client.ClientId);
                        OperationContextBase.SavedSettings.Remove(client);

                        if (ClientServiceCallMethods.TryGetValue(client, out List<Guid> callbacks))
                        {
                            ClientServiceCallMethods.TryRemove(client, out callbacks);
                            foreach (var guid in callbacks)
                            {
                                if (ClientServiceCallMethodsResult.TryGetValue(guid.ToString(), out KeyValue<Type, object> callback))
                                {
                                    ClientServiceCallMethodsResult.Remove(guid.ToString());
                                    callback.Value.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                        .FirstOrDefault(x => x.Name == "SetException" && x.GetParameters()[0].Name == "exception").Invoke(callback.Value, new object[] { new Exception("Client Disposed!") });
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        AutoLogger.LogError(ex, "DisposeClient remove");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("DisposeClient " + ex);
                AutoLogger.LogError(ex, "DisposeClientError");
            }
            finally
            {
                try
                {
                    OnClientDisconnectedAction?.Invoke(client);
                }
                catch (Exception ex)
                {

                }
            }
        }

        public void SetFirewall(IFirewall firewall)
        {
            if (firewall == null)
                throw new NullReferenceException("parameter firewall cannot be null or empty!");
            Firewall.DefaultFirewall = firewall;
        }

        public IFirewall GetFirewall()
        {
            return Firewall.DefaultFirewall;
        }

        public void RemoveTask(int taskId)
        {
            DataExchanger.Clear(taskId);
            TaskOfClientInfoes.Remove(taskId);
            OperationContext.CurrentTaskServerTasks.Remove(taskId);
            ValidationRuleInfoManager.RemoveTask(taskId);
        }

        public void AddTask(int taskId, string clientId)
        {
            DataExchanger.Clear(taskId);
            TaskOfClientInfoes.ForceAdd(taskId, clientId);
            OperationContext.CurrentTaskServerTasks.ForceAdd(taskId, this);
        }

        /// <summary>
        /// This closes the client passing its clientInfo
        /// </summary>
        /// <param name="client">client info</param>
        public void CloseClient(ClientInfo client)
        {
            DisposeClient(client, null, "manualy called CloseClient");
        }
        /// <summary>
        ///  This closes the client passing its id
        /// </summary>
        /// <param name="clientId">client session id</param>
        public void CloseClient(string clientId)
        {
            if (Clients.TryGetValue(clientId, out ClientInfo client))
                DisposeClient(client, null, "manualy called CloseClient 2");
        }
        /// <summary>
        /// When server is disposed
        /// </summary>
        public bool IsDisposed { get; set; }
        /// <summary>
        /// dispose service
        /// </summary>
        public void Dispose()
        {
            IsDisposed = true;
            Stop();
        }

        public void Stop()
        {
            foreach (KeyValuePair<string, ClientInfo> item in Clients.ToList())
            {
                DisposeClient(item.Value, null, "server stopped");
            }
            IsStarted = false;
            OnServerDisconnectedAction?.Invoke();
        }
    }
}
