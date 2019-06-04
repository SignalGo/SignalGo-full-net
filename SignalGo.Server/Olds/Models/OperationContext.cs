using SignalGo.Server.Helpers;
using SignalGo.Server.ServiceManager;
using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        /// <summary>
        /// if return null: Task.CurrentId is null or empty! Do not call this property or method inside of another thread or task you have to call this inside of server methods not another thread
        /// </summary>
        public static OperationContext Current
        {
            get
            {
                ServerBase currentServer = CurrentTaskServer;
                if (Task.CurrentId != null && currentServer != null && currentServer.TaskOfClientInfoes.TryGetValue(Task.CurrentId.GetValueOrDefault(), out string clientId))
                {
                    if (currentServer.Clients.TryGetValue(clientId, out ClientInfo clientInfo))
                        return new OperationContext() { Client = clientInfo, ClientId = clientId, ServerBase = currentServer };
                }
                return null;
            }
        }

        /// <summary>
        /// get operationcontext by client
        /// </summary>
        /// <param name="clientInfo"></param>
        /// <returns></returns>
        public static OperationContext GetCurrentByClient(ClientInfo clientInfo)
        {
            return new OperationContext() { Client = clientInfo, ClientId = clientInfo.ClientId, ServerBase = clientInfo.CurrentClientServer };
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
        internal static ConcurrentDictionary<string, string> SavedKeyParametersNameSettings { get; set; } = new ConcurrentDictionary<string, string>();
        internal static ConcurrentDictionary<string, HashSet<object>> CustomClientSavedSettings { get; set; } = new ConcurrentDictionary<string, HashSet<object>>();
        internal static object GetCurrentSetting(Type type, OperationContext context)
        {
            if (context == null)
                throw new Exception("Context is null or empty! Do not call this property inside of another thread or after await or another task");

            if (context.Client is HttpClientInfo)
            {
                bool isFindSessionProperty = false;
                List<string> keys = new List<string>();
                var properties = type.GetListOfProperties().Select(x => new { Info = x, Attribute = x.GetCustomAttributes<HttpKeyAttribute>().GroupBy(y => y.KeyType) });
                bool hasExpireField = false;
                foreach (var property in properties)
                {
                    foreach (IGrouping<HttpKeyType, HttpKeyAttribute> group in property.Attribute)
                    {
                        if (group.Key == HttpKeyType.Cookie)
                        {
                            if (property.Info.PropertyType != typeof(string))
                                throw new Exception("type of your HttpKeyAttribute must be as string because this will used for headers of http calls and you must made it custom");
                            foreach (HttpKeyAttribute httpKey in group.ToList())
                            {
                                isFindSessionProperty = true;
                                HttpClientInfo httpClient = context.Client as HttpClientInfo;
                                object setting = GetSetting(context.Client, type);
                                if (setting == null && (httpClient.RequestHeaders == null || string.IsNullOrEmpty(httpClient.GetRequestHeaderValue(httpKey.RequestHeaderName))))
                                    continue;

                                if (setting == null)
                                    keys.Add(ExtractValue(httpClient.GetRequestHeaderValue(httpKey.RequestHeaderName), httpKey.KeyName, httpKey.HeaderValueSeparate, httpKey.HeaderKeyValueSeparate));
                                else
                                    keys.Add(GetKeyFromSetting(type, setting));
                            }
                        }
                        else if (group.Key == HttpKeyType.ParameterName)
                        {
                            if (property.Info.PropertyType != typeof(string))
                                throw new Exception("type of your HttpKeyAttribute must be as string because this will used for headers of http calls and you must made it custom");
                            foreach (HttpKeyAttribute httpKey in group.ToList())
                            {
                                isFindSessionProperty = true;
                                HttpClientInfo httpClient = context.Client as HttpClientInfo;
                                if (httpClient.HttpKeyParameterValue != null)
                                {
                                    keys.Add(httpClient.HttpKeyParameterValue);
                                }

                            }
                        }
                        else if (group.Key == HttpKeyType.ExpireField)
                        {
                            hasExpireField = true;
                        }
                    }
                }
                foreach (var property in properties)
                {
                    foreach (IGrouping<HttpKeyType, HttpKeyAttribute> group in property.Attribute)
                    {
                        if (hasExpireField)
                        {
                            if (group.Key == HttpKeyType.ExpireField)
                            {
                                foreach (HttpKeyAttribute httpKey in group.ToList())
                                {
                                    foreach (string key in keys)
                                    {
                                        if (CustomClientSavedSettings.TryGetValue(key, out HashSet<object> result))
                                        {
                                            object obj = result.FirstOrDefault(x => x.GetType() == type);
                                            if (obj == null)
                                                continue;
                                            if (httpKey.CheckIsExpired(obj.GetType().GetProperty(property.Info.Name).GetValue(obj, null)))
                                            {
                                                result.Remove(obj);
                                                if (result.Count == 0)
                                                    CustomClientSavedSettings.TryRemove(key, out result);
                                                continue;
                                            }
                                            return obj;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (HttpKeyAttribute httpKey in group.ToList())
                            {
                                foreach (string key in keys)
                                {
                                    if (CustomClientSavedSettings.TryGetValue(key, out HashSet<object> result))
                                    {
                                        object obj = result.FirstOrDefault(x => x.GetType() == type);
                                        if (obj == null)
                                            continue;
                                        return obj;
                                    }
                                }
                            }
                        }
                    }
                }
                if (!isFindSessionProperty)
                    throw new Exception("HttpKeyAttribute on your one properties on class not found please made your string property that have HttpKeyAttribute on the top!");
                else
                    return null;
            }
            else if (SavedSettings.TryGetValue(context.Client, out HashSet<object> result))
            {
                return result.FirstOrDefault(x => x.GetType() == type);
            }
            return null;
        }

        /// <summary>
        /// return all of setting of current context
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object> GetAllSettings()
        {
            OperationContext context = OperationContext.Current;
            if (context == null)
                throw new Exception("Context is null or empty! Do not call this property inside of another thread or after await or another task");
            if (SavedSettings.TryGetValue(context.Client, out HashSet<object> result))
            {
                foreach (object item in result)
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// return all of settings that have http key attribute top of properties
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object> GetAllHttpKeySettings(OperationContext context)
        {
            if (context != null)
            {
                if (SavedSettings.TryGetValue(context.Client, out HashSet<object> result))
                {
                    foreach (object item in result)
                    {
                        if (item.GetType().GetListOfProperties().Any(x => x.GetCustomAttributes(typeof(HttpKeyAttribute), true).Count() > 0))
                            yield return item;
                    }
                }
            }
        }

        public static void SetCustomClientSetting(string customClientId, object setting)
        {
            if (setting == null)
                throw new Exception("setting is null or empty! please fill all parameters");
            if (string.IsNullOrEmpty(customClientId))
                throw new Exception("customClientId is null or empty! please fill all parameters on headers or etc");
            else if (!CustomClientSavedSettings.TryAdd(customClientId, new HashSet<object>() { setting }) && CustomClientSavedSettings.TryGetValue(customClientId, out HashSet<object> result) && !result.Contains(setting))
                result.Add(setting);
            List<string> httpKeys = setting.GetType().GetListOfProperties().SelectMany(x => x.GetCustomAttributes(typeof(HttpKeyAttribute), true).Cast<HttpKeyAttribute>()).Where(x => x.KeyType == HttpKeyType.ParameterName).Select(x => x.KeyParameterName).ToList();
            foreach (string item in httpKeys)
            {
                SavedKeyParametersNameSettings.TryAdd(item.ToLower(), item);
            }
        }

        internal static bool HasSettingNoHttp(Type type, ClientInfo clientInfo)
        {
            if (clientInfo is HttpClientInfo)
                return false;
            else if (SavedSettings.TryGetValue(clientInfo, out HashSet<object> result))
            {
                return result.Any(x => x.GetType() == type);
            }
            return false;
        }
        /// <summary>
        /// set setting for this client
        /// </summary>
        /// <param name="setting"></param>
        public static void SetSetting(object setting, OperationContext context)
        {
            if (context.Client is HttpClientInfo)
            {
                string key = GetKeyFromSetting(setting.GetType(), setting);
                SetCustomClientSetting(key, setting);
            }
            if (!SavedSettings.ContainsKey(context.Client))
                SavedSettings.TryAdd(context.Client, new HashSet<object>() { setting });
            else if (SavedSettings.TryGetValue(context.Client, out HashSet<object> result) && !result.Contains(setting))
            {
                result.RemoveWhere(x => x.GetType() == setting.GetType());
                result.Add(setting);
            }
        }

        public static object GetSetting(ClientInfo client, Type type)
        {
            if (SavedSettings.TryGetValue(client, out HashSet<object> result))
            {
                return result.FirstOrDefault(x => x.GetType() == type);
            }
            return null;
        }

        public static T GetSetting<T>(ClientInfo client)
        {
            Type type = typeof(T);
            if (SavedSettings.TryGetValue(client, out HashSet<object> result))
            {
                return (T)result.FirstOrDefault(x => x.GetType() == type);
            }
            return default(T);
        }

        /// <summary>
        /// clear all settings of client
        /// </summary>
        /// <param name="client"></param>
        public static void ClearSetting(ClientInfo client)
        {
            if (SavedSettings.TryGetValue(client, out HashSet<object> result))
            {
                result.Clear();
            }
        }
        /// <summary>
        /// clear all custom client settings
        /// </summary>
        /// <param name="client"></param>
        public static void ClearCustomSetting(ClientInfo client)
        {
            if (CustomClientSavedSettings.TryGetValue(client.ClientId, out HashSet<object> result))
            {
                result.Clear();
            }
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
                return (T)GetCurrentSetting(typeof(T), OperationContext.Current);
            }
            set
            {
                OperationContext context = OperationContext.Current;
                if (context == null)
                    throw new Exception("Context is null or empty! Do not call this property inside of another thread or after await or another task");

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
            if (context == null)
                throw new Exception("Context is null or empty! Do not call this property inside of another thread or after await or another task");
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
            if (context == null)
                throw new Exception("Context is null or empty! Do not call this property inside of another thread or after await or another task");
            if (SavedSettings.TryGetValue(context.Client, out HashSet<object> result))
            {
                return result;
            }
            return null;
        }

        public static bool HasSettingNoHttp(ClientInfo clientInfo)
        {
            return HasSettingNoHttp(typeof(T), clientInfo);
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

    /// <summary>
    /// operation context extentions
    /// </summary>
    public static class OCExtension
    {
        #region normal context services
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
                    string methodName = method.Name;
                    Task task = null;
                    Type returnType = method.ReturnType;
                    if (returnType == typeof(void))
                        returnType = typeof(object);
                    if (client.IsWebSocket)
                    {
                        MethodInfo sendDataMethod = typeof(ServerExtensions).GetMethod("SendWebSocketDataWithCallClientServiceMethod", BindingFlags.Static | BindingFlags.NonPublic)
                            .MakeGenericMethod(returnType);
                        task = (Task)sendDataMethod.Invoke(null, new object[] { serverBase, client, returnType, serviceName, method.Name, method.MethodToParameters(x => ServerSerializationHelper.SerializeObject(x, serverBase), args).ToArray() });
                    }
                    else
                    {
                        MethodInfo sendDataMethod = typeof(ServerExtensions).GetMethod("SendDataWithCallClientServiceMethod", BindingFlags.Static | BindingFlags.NonPublic)
                            .MakeGenericMethod(returnType);
                        task = (Task)sendDataMethod.Invoke(null, new object[] { serverBase, client, returnType, serviceName, method.Name, method.MethodToParameters(x => ServerSerializationHelper.SerializeObject(x, serverBase), args).ToArray() });
                    }
                    task.GetAwaiter().GetResult();
                    object result1 = task.GetType().GetProperty("Result").GetValue(task);
                    if (result1 is Task task2)
                    {
                        task2.GetAwaiter().GetResult();
                        return task2.GetType().GetProperty("Result").GetValue(task2, null);
                    }
                    if (method.ReturnType == typeof(Task))
                        return Task.FromResult(result1);
                    return result1;
                }, (serviceName, method, args) =>
                {
                    //this is async action
                    Type returnType = method.ReturnType;

                    if (method.ReturnType.GetBaseType() == typeof(Task))
                    {
                        returnType = method.ReturnType.GetGenericArguments()[0];
                    }
                    string methodName = method.Name;
                    if (client.IsWebSocket)
                    {
                        MethodInfo sendDataMethod = typeof(ServerExtensions).GetMethod("SendWebSocketDataWithCallClientServiceMethod", BindingFlags.Static | BindingFlags.NonPublic)
                            .MakeGenericMethod(returnType);
                        return sendDataMethod.Invoke(null, new object[] { serverBase, client, returnType, serviceName, method.Name, method.MethodToParameters(x => ServerSerializationHelper.SerializeObject(x, serverBase), args).ToArray() });
                    }
                    else
                    {
                        MethodInfo sendDataMethod = typeof(ServerExtensions).GetMethod("SendDataWithCallClientServiceMethod", BindingFlags.Static | BindingFlags.NonPublic)
                            .MakeGenericMethod(returnType);
                        return sendDataMethod.Invoke(null, new object[] { serverBase, client, returnType, serviceName, method.Name, method.MethodToParameters(x => ServerSerializationHelper.SerializeObject(x, serverBase), args).ToArray() });
                    }
                    //}
                    //this is async function
                    //else if (method.ReturnType.GetBaseType() == typeof(Task))
                    //{
                    //    string methodName = method.Name;
                    //    if (client.IsWebSocket)
                    //    {
                    //        MethodInfo sendDataMethod = typeof(ServerExtensions).GetMethod("SendWebSocketDataWithCallClientServiceMethod", BindingFlags.Static | BindingFlags.NonPublic)
                    //            .MakeGenericMethod(method.ReturnType);
                    //        return sendDataMethod.Invoke(null, new object[] { serverBase, client, method.ReturnType.GetGenericArguments()[0], serviceName, method.Name, method.MethodToParameters(x => ServerSerializationHelper.SerializeObject(x, serverBase), args).ToArray() });
                    //    }
                    //    else
                    //    {
                    //        MethodInfo sendDataMethod = typeof(ServerExtensions).GetMethod("SendDataWithCallClientServiceMethod", BindingFlags.Static | BindingFlags.NonPublic)
                    //               .MakeGenericMethod(method.ReturnType);
                    //        return sendDataMethod.Invoke(null, new object[] { serverBase, client, method.ReturnType.GetGenericArguments()[0], serviceName, method.Name, method.MethodToParameters(x => ServerSerializationHelper.SerializeObject(x, serverBase), args).ToArray() });
                    //    }
                    //}
                    throw new NotSupportedException();
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
        /// filter all client services context by client list
        /// </summary>
        /// <typeparam name="T">type of service</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clients">clients of clients to get client context</param>
        /// <returns>list of service context</returns>
        public static IEnumerable<ClientContext<T>> FilterClientContextServices<T>(this OperationContext context, Func<ClientInfo, bool> where) where T : class
        {
            return FilterClientContextServices<T>(context.ServerBase, where);
        }

        /// <summary>
        /// filter all client services context by client list
        /// </summary>
        /// <typeparam name="T">type of service</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clients">clients of clients to get client context</param>
        /// <returns>list of service context</returns>
        public static IEnumerable<ClientContext<T>> FilterClientContextServicesButMe<T>(this OperationContext context, Func<ClientInfo, bool> where) where T : class
        {
            foreach (ClientInfo item in context.ServerBase.Clients.Values.Where(where))
            {
                if (item == context.Client)
                    continue;
                T find = GenerateClientServiceInstance<T>(context.ServerBase, item);
                yield return new ClientContext<T>(find, item);
            }
        }

        #endregion















        #region client context services




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
        /// filter all client services context by client list
        /// </summary>
        /// <typeparam name="T">type of service</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clients">clients of clients to get client context</param>
        /// <returns>list of service context</returns>
        public static IEnumerable<ClientContext<T>> FilterClientContextServices<T>(this ServerBase serverBase, Func<ClientInfo, bool> where) where T : class
        {
            foreach (ClientInfo item in serverBase.Clients.Values.Where(where))
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


        #endregion
        /// <summary>
        /// get all client clienccontext services with setting query
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TSetting"></typeparam>
        /// <param name="serverBase"></param>
        /// <param name="canTake"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public static IEnumerable<ClientContext<TService>> GetAllClientClientContextServices<TService, TSetting>(this OperationContext operationContext, Func<TSetting, bool> canTake) where TService : class
        {
            return GetAllClientClientContextServices<TService, TSetting>(operationContext.ServerBase, canTake);
        }

        /// <summary>
        /// get all client clienccontext services with setting query
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TSetting"></typeparam>
        /// <param name="serverBase"></param>
        /// <param name="canTake"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public static IEnumerable<ClientContext<TService>> GetAllClientClientContextServices<TService, TSetting>(this ServerBase serverBase, Func<TSetting, bool> canTake) where TService : class
        {
            foreach (KeyValuePair<string, ClientInfo> item in serverBase.Clients)
            {
                var setting = (TSetting)OperationContextBase.GetSetting(item.Value, typeof(TSetting));
                if (setting == null)
                    continue;
                else if (canTake.Invoke(setting))
                {
                    TService find = GenerateClientServiceInstance<TService>(serverBase, item.Value);
                    yield return new ClientContext<TService>(find, item.Value);
                }
            }
        }
    }
}
