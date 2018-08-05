using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SignalGo.Server.DataTypes;
using SignalGo.Server.Helpers;
using SignalGo.Server.IO;
using SignalGo.Server.Models;
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

        private volatile int _callingCount;
        /// <summary>
        /// Method calling counter. If this reaches zero server can be stopped 
        /// </summary>
        public int CallingCount
        {
            get
            {
                return _callingCount;
            }
            private set
            {
                _callingCount = value;
                if (_callingCount == 0)
                    CallAfterFinishAction?.Invoke();
            }
        }

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
        /// key is client info id and value is service names and instance of services
        /// </summary>
        internal ConcurrentDictionary<string, ConcurrentDictionary<string, object>> MultipleInstanceServices { get; set; } = new ConcurrentDictionary<string, ConcurrentDictionary<string, object>>();

        /// <summary>
        /// include models to server reference when client want to add or update service reference
        /// </summary>
        internal List<Assembly> ModellingReferencesAssemblies { get; set; } = new List<Assembly>();

        /// <summary>
        /// Register server service
        /// </summary>
        /// <param name="serviceType"></param>
        public void RegisterServerService(Type serviceType)
        {
            var service = serviceType.GetServerServiceAttribute();
            if (service != null)
            {
                var name = service.Name;
                if (!RegisteredServiceTypes.ContainsKey(name))
                    RegisteredServiceTypes.TryAdd(name, serviceType);
            }
            else
            {
                throw new NotSupportedException("your service is not type of ServerService or HttpService or StreamService");
            }
        }
    }
}
