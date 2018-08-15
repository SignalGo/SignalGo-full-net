using SignalGo.Server.DataTypes;
using SignalGo.Server.Helpers;
using SignalGo.Server.IO;
using SignalGo.Server.Models;
using SignalGo.Server.ServiceManager.Versions;
using SignalGo.Server.Settings;
using SignalGo.Shared;
using SignalGo.Shared.Converters;
using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Events;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.Http;
using SignalGo.Shared.IO;
using SignalGo.Shared.Log;
using SignalGo.Shared.Managers;
using SignalGo.Shared.Models;
using SignalGo.Shared.Security;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SignalGo.Server.ServiceManager
{
    /// <summary>
    /// base of server
    /// </summary>
    public abstract class ServerBase : IDisposable
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public ServerBase()
        {
            JsonSettingHelper.Initialize();
        }

        /// <summary>
        /// server data provider communication between client and server
        /// </summary>
        internal IServerDataProvider ServerDataProvider { get; set; } = new ServerDataProviderV4();
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
        internal ConcurrentDictionary<string, ClientInfo> Clients { get; set; } = new ConcurrentDictionary<string, ClientInfo>();
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
        /// Register server service
        /// </summary>
        /// <param name="serviceType"></param>
        public void RegisterServerService(Type serviceType)
        {
            var service = serviceType.GetServerServiceAttribute();
            if (service != null)
            {
                var name = service.Name.ToLower();
                if (!RegisteredServiceTypes.ContainsKey(name))
                    RegisteredServiceTypes.TryAdd(name, serviceType);
            }
            else
            {
                throw new NotSupportedException("your service is not type of ServerService or HttpService or StreamService");
            }
        }

        public void RegisterClientService(Type serviceType)
        {
            var service = serviceType.GetClientServiceAttribute();
            if (service != null)
            {
                var name = service.Name.ToLower();
                if (!RegisteredServiceTypes.ContainsKey(name))
                    RegisteredServiceTypes.TryAdd(name, serviceType);
            }
            else
            {
                throw new NotSupportedException("your service is not type of ServerService or HttpService or StreamService");
            }
        }

        internal void DisposeClient(ClientInfo client, string reason)
        {
            try
            {
                Console.WriteLine($"Client disposed " + (client == null ? "null!" : client.ClientId) + " reason: " + reason);
                if (client == null)
                    return;
                try
                {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                    client.TcpClient.Dispose();
#else
                    client.TcpClient.Close();
#endif
                }
                catch (Exception ex)
                {
                    AutoLogger.LogError(ex, $"{client.IPAddress} {client.ClientId} CloseCllient");
                }
                //ClientRemove(client);

                foreach (var item in TaskOfClientInfoes.Where(x => x.Value == client.ClientId).ToArray())
                {
                    DataExchanger.Clear(item.Key);
                }

                OperationContextBase.SavedSettings.Remove(client);

                client.OnDisconnected?.Invoke();
                OnDisconnectedClientAction?.Invoke(client);
                GC.Collect();
            }
            catch (Exception ex)
            {
                Console.WriteLine("DisposeClient " + ex);
                AutoLogger.LogError(ex, "DisposeClientError");
            }
        }
        /// <summary>
        /// This closes the client passing its clientInfo
        /// </summary>
        /// <param name="client">client info</param>
        public void CloseClient(ClientInfo client)
        {
            DisposeClient(client, "manualy called CloseClient");
        }
        /// <summary>
        ///  This closes the client passing its id
        /// </summary>
        /// <param name="clientId">client session id</param>
        public void CloseClient(string clientId)
        {
            if (Clients.TryGetValue(clientId, out ClientInfo client))
                DisposeClient(client, "manualy called CloseClient 2");
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
            foreach (var item in Clients.ToList())
            {
                DisposeClient(item.Value, "server stopped");
            }
            IsStarted = false;
            OnServerDisconnectedAction?.Invoke();
        }
    }
}
