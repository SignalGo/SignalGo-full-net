using SignalGo.Server.Helpers;
using SignalGo.Server.ServiceManager;
using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SignalGo.Server.Models
{
    public class OperationContext
    {
        internal static ConcurrentDictionary<int, ServerBase> CurrentTaskServerTasks = new ConcurrentDictionary<int, ServerBase>();
        internal static ServerBase CurrentTaskServer
        {
            get
            {
                if (Task.CurrentId != null && CurrentTaskServerTasks.TryGetValue(Task.CurrentId.GetValueOrDefault(), out ServerBase serverBase))
                    return serverBase;
                return null;
            }
            set
            {
                if (Task.CurrentId != null)
                    CurrentTaskServerTasks[Task.CurrentId.GetValueOrDefault()] = value;
            }
        }
        public static OperationContext Current
        {
            get
            {
                ServerBase currentServer = CurrentTaskServer;
                if (Task.CurrentId != null && currentServer != null && currentServer.TaskOfClientInfoes.ContainsKey(Task.CurrentId.GetValueOrDefault()))
                {
                    string clientId = currentServer.TaskOfClientInfoes[Task.CurrentId.GetValueOrDefault()];
                    currentServer.Clients.TryGetValue(clientId, out ClientInfo clientInfo);
                    return new OperationContext() { Client = clientInfo, ClientId = clientId, ServerBase = currentServer };
                }
                throw new Exception("Task.CurrentId is null or empty! Do not call this property or method inside of another thread or task you have to call this inside of server methods not another thread");
            }
        }
        /// <summary>
        /// server provider
        /// </summary>
        public ServerBase ServerBase { get; set; }

        public string ClientId { get; set; }
        /// <summary>
        /// current client information
        /// </summary>
        public ClientInfo Client { get; private set; }
        /// <summary>
        /// current http client information if client is http call
        /// </summary>
        public HttpClientInfo HttpClient
        {
            get
            {
                return (HttpClientInfo)Client;
            }
        }

        /// <summary>
        /// all of server clients
        /// </summary>
        public List<ClientInfo> AllServerClients
        {
            get
            {
                return ServerBase.Clients.Values.ToList();
            }
        }

        /// <summary>
        /// count of connected Clients
        /// </summary>
        public int ConnectedClientsCount
        {
            get
            {
                return ServerBase.Clients.Count;
            }
        }

        /// <summary>
        /// get server service of
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetService<T>() where T : class
        {
            ServiceContractAttribute attribute = typeof(T).GetCustomAttributes<ServiceContractAttribute>(true).Where(x => x.ServiceType == ServiceType.ServerService).FirstOrDefault();
            ServerBase.SingleInstanceServices.TryGetValue(attribute.Name, out object result);
            return (T)result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public ClientInfo GetClientInfoByClientId(string clientId)
        {
            Current.ServerBase.Clients.TryGetValue(clientId, out ClientInfo clientInfo);
            return clientInfo;
        }

        public void AddResultOfDataExchanger(object instance, CustomDataExchangerAttribute customDataExchangerAttribute)
        {

        }

        public void AddResultOfDataExchanger(Type type, CustomDataExchangerAttribute customDataExchangerAttribute)
        {

        }
    }

    public class OperationContextBase
    {
        internal static ConcurrentDictionary<ClientInfo, HashSet<object>> SavedSettings { get; set; } = new ConcurrentDictionary<ClientInfo, HashSet<object>>();
        internal static ConcurrentDictionary<string, HashSet<object>> CustomClientSavedSettings { get; set; } = new ConcurrentDictionary<string, HashSet<object>>();
        internal static object GetCurrentSetting(Type type)
        {
            OperationContext context = OperationContext.Current;
            if (context == null)
                throw new Exception("SynchronizationContext is null or empty! Do not call this property inside of another thread that do not have any synchronizationContext or you can call SynchronizationContext.SetSynchronizationContext(new SynchronizationContext()); and ServerBase.AllDispatchers must contine this");

            if (context.Client is HttpClientInfo)
            {
                var sessionPeroperty = type.GetListOfProperties().Where(x => x.GetCustomAttributes<HttpKeyAttribute>().FirstOrDefault(y => !y.IsExpireField) != null).Select(x => new { Info = x, Attribute = x.GetCustomAttributes<HttpKeyAttribute>().FirstOrDefault() }).FirstOrDefault();
                var expirePeroperty = type.GetListOfProperties().Where(x => x.GetCustomAttributes<HttpKeyAttribute>().FirstOrDefault(y => y.IsExpireField) != null).Select(x => new { Info = x, Attribute = x.GetCustomAttributes<HttpKeyAttribute>().FirstOrDefault() }).FirstOrDefault();
                //var property = type.GetListOfProperties().Select(x => new
                //{
                //    Info = x,
                //    Attribute = x.GetCustomAttributes<HttpKeyAttribute>().FirstOrDefault(y => !y.IsExpireField),
                //    ExpiredAttribute = type.GetListOfProperties().Where(y => y.GetCustomAttributes<HttpKeyAttribute>().FirstOrDefault(j => j.IsExpireField) != null).Select(y => y.GetCustomAttributes<HttpKeyAttribute>().FirstOrDefault(j => j.IsExpireField)).FirstOrDefault()
                //}).FirstOrDefault(x => x.Attribute != null);
                if (sessionPeroperty == null)
                    throw new Exception("HttpKeyAttribute on your one properties on class not found please made your string property that have HttpKeyAttribute on the top!");
                else if (sessionPeroperty.Info.PropertyType != typeof(string))
                    throw new Exception("type of your HttpKeyAttribute must be as string because this will used for headers of http calls and you must made it custom");

                HttpClientInfo httpClient = context.Client as HttpClientInfo;
                object setting = GetSetting(context.Client, type);
                if (setting == null && (httpClient.RequestHeaders == null || string.IsNullOrEmpty(httpClient.RequestHeaders[sessionPeroperty.Attribute.RequestHeaderName])))
                    return null;

                string key = "";
                if (setting == null)
                    key = ExtractValue(httpClient.RequestHeaders[sessionPeroperty.Attribute.RequestHeaderName], sessionPeroperty.Attribute.KeyName, sessionPeroperty.Attribute.HeaderValueSeparate, sessionPeroperty.Attribute.HeaderKeyValueSeparate);
                else
                    key = GetKeyFromSetting(type, setting);
                if (CustomClientSavedSettings.TryGetValue(key, out HashSet<object> result))
                {
                    object obj = result.FirstOrDefault(x => x.GetType() == type);
                    if (obj == null)
                        return null;
                    if (expirePeroperty != null && obj != null && expirePeroperty.Attribute.CheckIsExpired(obj.GetType().GetProperty(expirePeroperty.Info.Name).GetValue(obj, null)))
                    {
                        result.Remove(obj);
                        if (result.Count == 0)
                            CustomClientSavedSettings.TryRemove(key, out result);
                        return null;
                    }
                    return obj;
                }
            }
            else if (SavedSettings.TryGetValue(context.Client, out HashSet<object> result))
            {
                return result.FirstOrDefault(x => x.GetType() == type);
            }
            return null;
        }

        /// <summary>
        /// set setting for this client
        /// </summary>
        /// <param name="setting"></param>
        public static void SetSetting(object setting, OperationContext context)
        {
            if (!SavedSettings.ContainsKey(context.Client))
                SavedSettings.TryAdd(context.Client, new HashSet<object>() { setting });
            else if (SavedSettings.TryGetValue(context.Client, out HashSet<object> result) && !result.Contains(setting))
                result.Add(setting);
        }

        public static object GetSetting(ClientInfo client, Type type)
        {
            if (SavedSettings.TryGetValue(client, out HashSet<object> result))
            {
                return result.FirstOrDefault(x => x.GetType() == type);
            }
            return null;
        }

        private static string ExtractValue(string data, string keyName, string valueSeparateChar, string keyValueSeparateChar)
        {
            if (string.IsNullOrEmpty(data) || string.IsNullOrEmpty(keyName) || (string.IsNullOrEmpty(valueSeparateChar) && string.IsNullOrEmpty(keyValueSeparateChar)))
                return data;
            if (string.IsNullOrEmpty(keyValueSeparateChar))
            {
                return data.Split(new string[] { valueSeparateChar }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault().Trim();
            }
            else if (string.IsNullOrEmpty(valueSeparateChar))
            {
                return data.Split(new string[] { keyValueSeparateChar }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault().Trim();
            }
            else
            {
                foreach (string keyValue in data.Split(new string[] { valueSeparateChar }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] separate = keyValue.Split(new string[] { keyValueSeparateChar }, StringSplitOptions.RemoveEmptyEntries);
                    if (string.IsNullOrEmpty(separate.FirstOrDefault()))
                        continue;
                    if (separate.FirstOrDefault().ToLower().Trim() == keyName.ToLower())
                        return separate.Length > 1 ? separate.LastOrDefault().Trim() : "";
                }
            }
            return "";
        }

        internal static string IncludeValue(string value, string keyName, string valueSeparateChar, string keyValueSeparateChar)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(keyName) || string.IsNullOrEmpty(keyValueSeparateChar))
                return value;
            return keyName + keyValueSeparateChar + value;

        }

        internal static string GetKeyFromSetting(Type type, object setting)
        {
            var property = type.GetListOfProperties().Select(x => new { Info = x, Attribute = x.GetCustomAttributes<HttpKeyAttribute>().FirstOrDefault() }).FirstOrDefault(x => x.Attribute != null);
            if (property == null)
                throw new Exception("HttpKeyAttribute on your one properties on class not found please made your string property that have HttpKeyAttribute on the top!");
            else if (property.Info.PropertyType != typeof(string))
                throw new Exception("type of your HttpKeyAttribute must be as string because this will used for headers of http calls and you must made it custom");
            return (string)property.Info.GetValue(setting, null);
        }
    }

    /// <summary>
    /// operation contract for client that help you to save a class and get it later inside of your service class
    /// </summary>
    /// <typeparam name="T">type of your setting</typeparam>
    public class OperationContext<T> : OperationContextBase where T : class
    {
        /// <summary>
        /// get seeting of one type that you set it
        /// </summary>
        public static T CurrentSetting
        {
            get
            {
                return (T)GetCurrentSetting(typeof(T));
            }
            set
            {
                OperationContext context = OperationContext.Current;
                if (context == null)
                    throw new Exception("SynchronizationContext is null or empty! Do not call this property inside of another thread that do not have any synchronizationContext or you can call SynchronizationContext.SetSynchronizationContext(new SynchronizationContext()); and ServerBase.AllDispatchers must contine this");

                if (context.Client is HttpClientInfo)
                {
                    string key = GetKeyFromSetting(typeof(T), value);
                    SetCustomClientSetting(key, value);
                    //SetSetting(value, context);
                }
                SetSetting(value, context);
            }
        }


        /// <summary>
        /// get first setting of type that setted
        /// </summary>
        /// <typeparam name="T">type of setting</typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetSettings()
        {
            OperationContext context = OperationContext.Current;
            if (SynchronizationContext.Current == null)
                throw new Exception("SynchronizationContext is null or empty! Do not call this property inside of another thread that do not have any synchronizationContext or you can call SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());");
            if (SavedSettings.TryGetValue(context.Client, out HashSet<object> result))
            {
                return result.Where(x => x.GetType() == typeof(T)).Select(x => (T)x);
            }
            return null;
        }

        public static IEnumerable<T> GetSettings(ClientInfo client)
        {
            if (SavedSettings.TryGetValue(client, out HashSet<object> result))
            {
                return result.Where(x => x.GetType() == typeof(T)).Select(x => (T)x);
            }
            return null;
        }

        public static T GetSetting(ClientInfo client)
        {
            if (SavedSettings.TryGetValue(client, out HashSet<object> result))
            {
                return (T)result.FirstOrDefault(x => x.GetType() == typeof(T));
            }
            return default(T);
        }



        public static IEnumerable<T> GetSettings(string clientId)
        {
            ClientInfo clientInfo = OperationContext.Current.GetClientInfoByClientId(clientId);
            return GetSettings(clientInfo);
        }

        public static T GetSetting(string clientId)
        {
            ClientInfo clientInfo = OperationContext.Current.GetClientInfoByClientId(clientId);
            return GetSetting(clientInfo);
        }

        public static void SetCustomClientSetting(string customClientId, object setting)
        {
            if (setting == null)
                throw new Exception("setting is null or empty! please fill all parameters");
            if (string.IsNullOrEmpty(customClientId))
                throw new Exception("customClientId is null or empty! please fill all parameters on headers or etc");
            //if (!CustomClientSavedSettings.ContainsKey(customClientId))
            //    ;
            else if (!CustomClientSavedSettings.TryAdd(customClientId, new HashSet<object>() { setting }) && CustomClientSavedSettings.TryGetValue(customClientId, out HashSet<object> result) && !result.Contains(setting))
                result.Add(setting);
        }

        /// <summary>
        /// get setting of your custom client id or sessions or etc
        /// </summary>
        /// <param name="customClientId"></param>
        /// <returns></returns>
        public static T GetCustomClientSetting(string customClientId)
        {
            if (string.IsNullOrEmpty(customClientId))
                throw new Exception("customClientId parameter is null or empty!");
            if (CustomClientSavedSettings.TryGetValue(customClientId, out HashSet<object> result))
            {
                return (T)result.FirstOrDefault(x => x.GetType() == typeof(T));
            }
            return default(T);
        }

        public static T RemoveCustomClientSetting(string customClientId)
        {
            if (string.IsNullOrEmpty(customClientId))
                throw new Exception("customClientId parameter is null or empty!");
            if (CustomClientSavedSettings.TryRemove(customClientId, out HashSet<object> result))
            {
                return (T)result.FirstOrDefault(x => x.GetType() == typeof(T));
            }
            return default(T);
        }

        public static IEnumerable<T> GetCustomClientSettings(string customClientId)
        {
            if (string.IsNullOrEmpty(customClientId))
                throw new Exception("customClientId parameter is null or empty!");
            if (CustomClientSavedSettings.TryGetValue(customClientId, out HashSet<object> result))
            {
                return result.Where(x => x.GetType() == typeof(T)).Select(x => (T)x);
            }
            return null;
        }

        public static IEnumerable<T2> GetSettings<T2>(IEnumerable<ClientInfo> clients, Func<T2, bool> func)
        {
            foreach (ClientInfo item in clients)
            {
                if (SavedSettings.TryGetValue(item, out HashSet<object> result))
                {
                    return result.Where(x => x.GetType() == typeof(T2) && func((T2)x)).Select(x => (T2)x);
                }
            }
            return null;
        }

        public static IEnumerable<T> GetSettings(IEnumerable<ClientInfo> clients, Func<T, bool> func)
        {
            foreach (ClientInfo item in clients)
            {
                if (SavedSettings.TryGetValue(item, out HashSet<object> result))
                {
                    T find = result.Where(x => x.GetType() == typeof(T) && func((T)x)).Select(x => (T)x).FirstOrDefault();
                    if (find != null)
                        yield return find;
                }
            }
        }

        /// <summary>
        /// get all settings of client
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object> GetAllSettings()
        {
            OperationContext context = OperationContext.Current;
            if (SynchronizationContext.Current == null)
                throw new Exception("SynchronizationContext is null or empty! Do not call this property inside of another thread that do not have any synchronizationContext or you can call SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());");
            if (SavedSettings.TryGetValue(context.Client, out HashSet<object> result))
            {
                return result;
            }
            return null;
        }
    }

    public class ClientContext<T>
    {
        public ClientContext(object service, ClientInfo client)
        {
            Service = (T)service;
            Client = client;
        }

        public T Service { get; set; }
        public ClientInfo Client { get; set; }
    }

    public static class OCExtension
    {
        /// <summary>
        /// get current context service
        /// </summary>
        /// <typeparam name="T">type of service</typeparam>
        /// <param name="context">client context</param>
        /// <returns>list of service context</returns>
        internal static T GenerateClientServiceInstance<T>(ServerBase serverBase, ClientInfo client) where T : class
        {
            if (typeof(T).GetIsInterface())
            {
                T objectInstance = InterfaceWrapper.Wrap<T>((serviceName, method, args) =>
                {
                    //this is async action
                    if (method.ReturnType == typeof(Task))
                    {
                        string methodName = method.Name;
                        Task task = ServerExtensions.SendDataWithCallClientServiceMethod(serverBase, client, null, serviceName, method.Name, method.MethodToParameters(x => ServerSerializationHelper.SerializeObject(x, serverBase), args).ToArray());
                        return task;
                    }
                    //this is async function
                    else if (method.ReturnType.GetBaseType() == typeof(Task))
                    {
                        string methodName = method.Name;
                        Task task = ServerExtensions.SendDataWithCallClientServiceMethod(serverBase, client, method.ReturnType.GetGenericArguments()[0], serviceName, method.Name, method.MethodToParameters(x => ServerSerializationHelper.SerializeObject(x, serverBase), args).ToArray());
                        return task;
                    }
                    else
                    {
                        string methodName = method.Name;
                        Task task = ServerExtensions.SendDataWithCallClientServiceMethod(serverBase, client, method.ReturnType, serviceName, method.Name, method.MethodToParameters(x => ServerSerializationHelper.SerializeObject(x, serverBase), args).ToArray());
                        task.Wait();
                        return task.GetType().GetProperty("Result").GetValue(task, null);
                    }
                });

                return objectInstance;
            }
            else
            {
                object instance = Activator.CreateInstance(typeof(T));
                return (T)instance;
            }
        }

        /// <summary>
        ///  get all client context service
        /// </summary>
        /// <typeparam name="T">type of service</typeparam>
        /// <param name="context">client context</param>
        /// <returns>list of service context</returns>
        public static IEnumerable<ClientContext<T>> GetAllClientClientContextServices<T>(this OperationContext context) where T : class
        {
            return GetAllClientClientContextServices<T>(context.ServerBase);
        }

        /// <summary>
        /// get all client context but ignore current context
        /// </summary>
        /// <typeparam name="T">type of service</typeparam>
        /// <param name="context">client context</param>
        /// <returns>list of service context</returns>
        public static IEnumerable<ClientContext<T>> GetAllClientContextServicesButMe<T>(this OperationContext context) where T : class
        {
            foreach (KeyValuePair<string, ClientInfo> item in context.ServerBase.Clients)
            {
                if (item.Value == context.Client)
                    continue;
                T find = GenerateClientServiceInstance<T>(context.ServerBase, item.Value);
                yield return new ClientContext<T>(find, item.Value);
            }
        }

        /// <summary>
        /// get clients service context by session list
        /// </summary>
        /// <typeparam name="T">service type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clientId">list of sessions</param>
        /// <returns>list of service context</returns>
        public static ClientContext<T> GetClientContextService<T>(this OperationContext context, string clientId) where T : class
        {
            return GetClientContextService<T>(context.ServerBase, clientId);

        }

        /// <summary>
        /// get clients service context by session list
        /// </summary>
        /// <typeparam name="T">service type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="client">client</param>
        /// <returns>list of service context</returns>
        public static ClientContext<T> GetClientContextService<T>(this OperationContext context, ClientInfo client) where T : class
        {
            return GetClientContextService<T>(context.ServerBase, client);
        }

        /// <summary>
        /// get clients service context by session list
        /// </summary>
        /// <typeparam name="T">service type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clientId">id of client</param>
        /// <returns>list of service context</returns>
        public static T GetClientService<T>(this OperationContext context, string clientId) where T : class
        {
            return GetClientService<T>(context.ServerBase, clientId);
        }

        /// <summary>
        /// get clients service context by session list
        /// </summary>
        /// <typeparam name="T">service type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="client">client</param>
        /// <returns>list of service context</returns>
        public static T GetClientService<T>(this OperationContext context, ClientInfo client) where T : class
        {
            return GetClientService<T>(context.ServerBase, client);
        }

        /// <summary>
        /// get client service context by client list
        /// </summary>
        /// <typeparam name="T">type of service</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clients">clients of clients to get client context</param>
        /// <returns>list of service context</returns>
        public static IEnumerable<ClientContext<T>> GetListOfClientContextServices<T>(this OperationContext context, IEnumerable<ClientInfo> clients) where T : class
        {
            return GetListOfClientContextServices<T>(context.ServerBase, clients);
        }

        /// <summary>
        /// get clients service context by session list
        /// </summary>
        /// <typeparam name="T">service type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clientIds">list of sessions</param>
        /// <returns>list of service context</returns>
        public static IEnumerable<ClientContext<T>> GetListOfClientContextServices<T>(this OperationContext context, IEnumerable<string> clientIds) where T : class
        {
            return GetListOfClientContextServices<T>(context.ServerBase, clientIds);
        }

        /// <summary>
        /// get clients service context list and ignore custom session list
        /// </summary>
        /// <typeparam name="T">service type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clientIds">list of sessions to ingore get</param>
        /// <returns>list of service context</returns>
        public static IEnumerable<ClientContext<T>> GetListOfExcludeClientContextServices<T>(this OperationContext context, IEnumerable<string> clientIds) where T : class
        {
            return GetListOfExcludeClientContextServices<T>(context.ServerBase, clientIds);
        }




        /// <summary>
        /// get current context service
        /// </summary>
        /// <typeparam name="T">type of service</typeparam>
        /// <param name="context">client context</param>
        /// <returns>list of service context</returns>
        public static T GetClientService<T>(this OperationContext context) where T : class
        {
            return GetClientService<T>(context, context.ClientId);
        }

        /// <summary>
        ///  get all client context service
        /// </summary>
        /// <typeparam name="T">type of service</typeparam>
        /// <param name="context">client context</param>
        /// <returns>list of service context</returns>
        public static IEnumerable<T> GetAllClientServices<T>(this OperationContext context) where T : class
        {
            return GetAllClientServices<T>(context.ServerBase);
        }

        /// <summary>
        /// get all client context but ignore current context
        /// </summary>
        /// <typeparam name="T">type of service</typeparam>
        /// <param name="context">client context</param>
        /// <returns>list of service context</returns>
        public static IEnumerable<T> GetAllClientServicesButMe<T>(this OperationContext context) where T : class
        {
            return (from x in GetAllClientContextServicesButMe<T>(context) select x.Service);
        }

        /// <summary>
        /// get client service by client list
        /// </summary>
        /// <typeparam name="T">type of service</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clients">clients of clients to get client service</param>
        /// <returns>list of service</returns>
        public static IEnumerable<T> GetListOfClientServices<T>(this OperationContext context, IEnumerable<ClientInfo> clients) where T : class
        {
            return GetListOfClientServices<T>(context.ServerBase, clients);
        }

        /// <summary>
        /// get clients service by session list
        /// </summary>
        /// <typeparam name="T">service type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clientIds">list of sessions</param>
        /// <returns>list of service</returns>
        public static IEnumerable<T> GetListOfClientServices<T>(this OperationContext context, IEnumerable<string> clientIds) where T : class
        {
            return GetListOfClientServices<T>(context.ServerBase, clientIds);
        }

        /// <summary>
        /// get clients services list and ignore custom session list
        /// </summary>
        /// <typeparam name="T">service type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clientIds">list of sessions to ingore get</param>
        /// <returns>list of services</returns>
        public static IEnumerable<T> GetListOfExcludeClientServices<T>(this OperationContext context, IEnumerable<string> clientIds) where T : class
        {
            return GetListOfExcludeClientServices<T>(context.ServerBase, clientIds);
        }























        /// <summary>
        ///  get all client context service
        /// </summary>
        /// <typeparam name="T">type of service</typeparam>
        /// <param name="context">client context</param>
        /// <returns>list of service context</returns>
        public static IEnumerable<ClientContext<T>> GetAllClientClientContextServices<T>(this ServerBase serverBase) where T : class
        {
            foreach (KeyValuePair<string, ClientInfo> item in serverBase.Clients)
            {
                T find = GenerateClientServiceInstance<T>(serverBase, item.Value);
                yield return new ClientContext<T>(find, item.Value);
            }
        }


        /// <summary>
        /// get clients service context by session list
        /// </summary>
        /// <typeparam name="T">service type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clientId">list of sessions</param>
        /// <returns>list of service context</returns>
        public static ClientContext<T> GetClientContextService<T>(this ServerBase serverBase, string clientId) where T : class
        {
            serverBase.Clients.TryGetValue(clientId, out ClientInfo clientInfo);
            if (clientInfo == null)
                return null;
            T find = GenerateClientServiceInstance<T>(serverBase, clientInfo);
            return new ClientContext<T>(find, clientInfo);
        }

        /// <summary>
        /// get clients service context by session list
        /// </summary>
        /// <typeparam name="T">service type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="client">client</param>
        /// <returns>list of service context</returns>
        public static ClientContext<T> GetClientContextService<T>(this ServerBase serverBase, ClientInfo client) where T : class
        {
            string serviceName = typeof(T).GetClientServiceName(true);
            serverBase.RegisteredServiceTypes.TryGetValue(serviceName, out Type serviceType);
            T find = GenerateClientServiceInstance<T>(serverBase, client);
            return new ClientContext<T>(find, client);
        }

        /// <summary>
        /// get clients service context by session list
        /// </summary>
        /// <typeparam name="T">service type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clientId">id of client</param>
        /// <returns>list of service context</returns>
        public static T GetClientService<T>(this ServerBase serverBase, string clientId) where T : class
        {
            ClientContext<T> client = GetClientContextService<T>(serverBase, clientId);
            if (client != null)
                return client.Service;
            return default(T);
        }

        /// <summary>
        /// get clients service context by session list
        /// </summary>
        /// <typeparam name="T">service type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="client">client</param>
        /// <returns>list of service context</returns>
        public static T GetClientService<T>(this ServerBase serverBase, ClientInfo client) where T : class
        {
            ClientContext<T> result = GetClientContextService<T>(serverBase, client);
            if (result != null)
                return result.Service;
            return default(T);
        }

        /// <summary>
        /// get client service context by client list
        /// </summary>
        /// <typeparam name="T">type of service</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clients">clients of clients to get client context</param>
        /// <returns>list of service context</returns>
        public static IEnumerable<ClientContext<T>> GetListOfClientContextServices<T>(this ServerBase serverBase, IEnumerable<ClientInfo> clients) where T : class
        {
            foreach (ClientInfo item in clients)
            {
                T find = GenerateClientServiceInstance<T>(serverBase, item);
                yield return new ClientContext<T>(find, item);
            }
        }

        /// <summary>
        /// get clients service context by session list
        /// </summary>
        /// <typeparam name="T">service type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clientIds">list of sessions</param>
        /// <returns>list of service context</returns>
        public static IEnumerable<ClientContext<T>> GetListOfClientContextServices<T>(this ServerBase serverBase, IEnumerable<string> clientIds) where T : class
        {
            foreach (string clientId in clientIds)
            {
                serverBase.Clients.TryGetValue(clientId, out ClientInfo clientInfo);
                if (clientInfo == null)
                    continue;
                T find = GenerateClientServiceInstance<T>(serverBase, clientInfo);
                yield return new ClientContext<T>(find, clientInfo);
            }
        }

        /// <summary>
        /// get clients service context list and ignore custom session list
        /// </summary>
        /// <typeparam name="T">service type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clientIds">list of sessions to ingore get</param>
        /// <returns>list of service context</returns>
        public static IEnumerable<ClientContext<T>> GetListOfExcludeClientContextServices<T>(this ServerBase serverBase, IEnumerable<string> clientIds) where T : class
        {
            foreach (KeyValuePair<string, ClientInfo> client in serverBase.Clients)
            {
                if (clientIds.Contains(client.Key))
                    continue;
                T find = GenerateClientServiceInstance<T>(serverBase, client.Value);
                yield return new ClientContext<T>(find, client.Value);
            }
        }




        /// <summary>
        ///  get all client context service
        /// </summary>
        /// <typeparam name="T">type of service</typeparam>
        /// <param name="context">client context</param>
        /// <returns>list of service context</returns>
        public static IEnumerable<T> GetAllClientServices<T>(this ServerBase serverBase) where T : class
        {
            return (from x in GetAllClientClientContextServices<T>(serverBase) select x.Service);
        }

        /// <summary>
        /// get client service by client list
        /// </summary>
        /// <typeparam name="T">type of service</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clients">clients of clients to get client service</param>
        /// <returns>list of service</returns>
        public static IEnumerable<T> GetListOfClientServices<T>(this ServerBase serverBase, IEnumerable<ClientInfo> clients) where T : class
        {
            return (from x in GetListOfClientContextServices<T>(serverBase, clients) select x.Service);
        }

        /// <summary>
        /// get clients service by session list
        /// </summary>
        /// <typeparam name="T">service type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clientIds">list of sessions</param>
        /// <returns>list of service</returns>
        public static IEnumerable<T> GetListOfClientServices<T>(this ServerBase serverBase, IEnumerable<string> clientIds) where T : class
        {
            return (from x in GetListOfClientContextServices<T>(serverBase, clientIds) select x.Service);
        }

        /// <summary>
        /// get clients services list and ignore custom session list
        /// </summary>
        /// <typeparam name="T">service type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clientIds">list of sessions to ingore get</param>
        /// <returns>list of services</returns>
        public static IEnumerable<T> GetListOfExcludeClientServices<T>(this ServerBase serverBase, IEnumerable<string> clientIds) where T : class
        {
            return (from x in GetListOfExcludeClientContextServices<T>(serverBase, clientIds) select x.Service);
        }

    }
}
