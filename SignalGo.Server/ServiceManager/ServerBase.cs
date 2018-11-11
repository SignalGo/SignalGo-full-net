using SignalGo.Server.Models;
using SignalGo.Server.ServiceManager.Versions;
using SignalGo.Shared;
using SignalGo.Shared.Converters;
using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.Log;
using SignalGo.Shared.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;

namespace SignalGo.Server.ServiceManager
{
    /// <summary>
    /// base of server
    /// </summary>
    public abstract class ServerBase : IDisposable, IValidationRuleInfo
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public ServerBase()
        {
            JsonSettingHelper.Initialize();
        }
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
        private JsonSettingHelper JsonSettingHelper { get; set; } = new JsonSettingHelper();

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
        public Action<ClientInfo> OnConnectedClientAction { get; set; }
        /// <summary>
        /// after client disconnected
        /// </summary>
        public Action<ClientInfo> OnDisconnectedClientAction { get; set; }

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
        /// task ids of client calling methods etc
        /// key is task id and value is client id
        /// </summary>
        internal ConcurrentDictionary<int, string> TaskOfClientInfoes { get; set; } = new ConcurrentDictionary<int, string>();
        /// <summary>
        /// all of the clients service call methods from server those wait for recevice data from client and set result to task that waited
        /// </summary>
        internal ConcurrentDictionary<string, KeyValue<Type, object>> ClientServiceCallMethodsResult { get; set; } = new ConcurrentDictionary<string, KeyValue<Type, object>>();

        /// <summary>
        /// Error handling methods that return types (not void)
        /// exception is exception trow
        /// Type is service Type
        /// MethodInfo method of service
        /// object is your return value
        /// </summary>
        public Func<Exception, Type, MethodInfo, object> ErrorHandlingFunction { get; set; }
        /// <summary>
        /// if you don't want to trhow exception when method have error validation you can fill this function to customize your result for client
        /// </summary>
        public Func<List<ValidationRuleInfoAttribute>, object, MethodInfo, object> ValidationResultHandlingFunction { get; set; }
        /// <summary>
        /// Register server service
        /// </summary>
        /// <param name="serviceType"></param>
        public void RegisterServerService<T>()
        {
            RegisterServerService(typeof(T));
        }
        /// <summary>
        /// Register server service
        /// </summary>
        /// <param name="serviceType"></param>
        public void RegisterServerService(Type serviceType)
        {
            ServiceContractAttribute[] services = serviceType.GetServiceContractAttributes();
            foreach (ServiceContractAttribute service in services)
            {
                string name = service.GetServiceName(false).ToLower();
                if (!RegisteredServiceTypes.ContainsKey(name))
                    RegisteredServiceTypes.TryAdd(name, serviceType);
            }
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
                if (!RegisteredServiceTypes.ContainsKey(name))
                    RegisteredServiceTypes.TryAdd(name, serviceType);
            }
            else
            {
                throw new NotSupportedException("your service is not type of ServerService or HttpService or StreamService");
            }
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
                Clients.Remove(client.ClientId);
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
                //ClientRemove(client);

                OperationContextBase.SavedSettings.Remove(client);

                client.OnDisconnected?.Invoke();
                OnDisconnectedClientAction?.Invoke(client);
            }
            catch (Exception ex)
            {
                Console.WriteLine("DisposeClient " + ex);
                AutoLogger.LogError(ex, "DisposeClientError");
            }
            finally
            {
                //GC.Collect();
            }
        }

        internal void RemoveTask(int TaskId)
        {
            DataExchanger.Clear(TaskId);
            TaskOfClientInfoes.Remove(TaskId);
            OperationContext.CurrentTaskServerTasks.Remove(TaskId);
            ValidationRuleInfoManager.RemoveTask(TaskId);
        }

        internal void AddTask(int TaskId, string clientId)
        {
            DataExchanger.Clear(TaskId);
            TaskOfClientInfoes.TryAdd(TaskId, clientId);
            OperationContext.CurrentTaskServerTasks.TryAdd(TaskId, this);
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
