using SignalGo.Server.ServiceManager;
using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.Log;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace SignalGo.Server.Models
{
    public class OperationContext
    {
        public static OperationContext Current
        {
            get
            {
                if (SynchronizationContext.Current != null && ServerBase.AllDispatchers.ContainsKey(SynchronizationContext.Current))
                {
                    var clients = ServerBase.AllDispatchers[SynchronizationContext.Current];
                    return new OperationContext() { Client = clients.FirstOrDefault(), Clients = clients, ServerBase = clients.FirstOrDefault().ServerBase };
                }
                throw new Exception("SynchronizationContext is null or empty! Do not call this property or method inside of another thread you have to call this inside of server methods not another thread");
            }
        }
        /// <summary>
        /// server provider
        /// </summary>
        public ServerBase ServerBase { get; set; }
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
        /// all client info of this thread
        /// </summary>
        public IEnumerable<ClientInfo> Clients { get; private set; }
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
            var attribute = typeof(T).GetCustomAttributes<ServiceContractAttribute>(true).Where(x => x.ServiceType == ServiceType.ServerService).FirstOrDefault();
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
            return Current.ServerBase.GetClientByClientId(clientId);
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
            var context = OperationContext.Current;
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

                var httpClient = context.Client as HttpClientInfo;
                var setting = GetSetting(context.Client, type);
                if (setting == null && (httpClient.RequestHeaders == null || string.IsNullOrEmpty(httpClient.RequestHeaders[sessionPeroperty.Attribute.RequestHeaderName])))
                    return null;

                string key = "";
                if (setting == null)
                    key = ExtractValue(httpClient.RequestHeaders[sessionPeroperty.Attribute.RequestHeaderName], sessionPeroperty.Attribute.KeyName, sessionPeroperty.Attribute.HeaderValueSeparate, sessionPeroperty.Attribute.HeaderKeyValueSeparate);
                else
                    key = GetKeyFromSetting(type, setting);
                if (CustomClientSavedSettings.TryGetValue(key, out HashSet<object> result))
                {
                    var obj = result.FirstOrDefault(x => x.GetType() == type);
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

        static string ExtractValue(string data, string keyName, string valueSeparateChar, string keyValueSeparateChar)
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
                foreach (var keyValue in data.Split(new string[] { valueSeparateChar }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var separate = keyValue.Split(new string[] { keyValueSeparateChar }, StringSplitOptions.RemoveEmptyEntries);
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

        static T _Current = null;
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
                var context = OperationContext.Current;
                if (context == null)
                    throw new Exception("SynchronizationContext is null or empty! Do not call this property inside of another thread that do not have any synchronizationContext or you can call SynchronizationContext.SetSynchronizationContext(new SynchronizationContext()); and ServerBase.AllDispatchers must contine this");

                if (context.Client is HttpClientInfo)
                {
                    var key = GetKeyFromSetting(typeof(T), value);
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
            var context = OperationContext.Current;
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
            var clientInfo = OperationContext.Current.GetClientInfoByClientId(clientId);
            return GetSettings(clientInfo);
        }

        public static T GetSetting(string clientId)
        {
            var clientInfo = OperationContext.Current.GetClientInfoByClientId(clientId);
            return GetSetting(clientInfo);
        }

        public static void SetCustomClientSetting(string customClientId, object setting)
        {
            if (setting == null)
                throw new Exception("setting is null or empty! please fill all parameters");
            if (string.IsNullOrEmpty(customClientId))
                throw new Exception("customClientId is null or empty! please fill all parameters");
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

        public static IEnumerable<T> GetSettings<T>(IEnumerable<ClientInfo> clients, Func<T, bool> func)
        {
            foreach (var item in clients)
            {
                if (SavedSettings.TryGetValue(item, out HashSet<object> result))
                {
                    return result.Where(x => x.GetType() == typeof(T) && func((T)x)).Select(x => (T)x);
                }
            }
            return null;
        }

        public static IEnumerable<T> GetSettings(IEnumerable<ClientInfo> clients, Func<T, bool> func)
        {
            foreach (var item in clients)
            {
                if (SavedSettings.TryGetValue(item, out HashSet<object> result))
                {
                    var find = result.Where(x => x.GetType() == typeof(T) && func((T)x)).Select(x => (T)x).FirstOrDefault();
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
            var context = OperationContext.Current;
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
        internal static ClientContext<T> GetClientClientContextService<T>(this OperationContext context)
        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            if (typeof(T).GetTypeInfo().IsInterface)
#else
            if (typeof(T).IsInterface)
#endif
            {
                if (!context.ServerBase.ClientServices.ContainsKey(context.Client))
                {
                    context.ServerBase.RegisterClientServices(context.Client);
                    if (!context.ServerBase.ClientServices.ContainsKey(context.Client))
                    {
                        try
                        {
                            throw new Exception($"context client not exist! {context.Client.ClientId} {context.ServerBase.ClientServices.Count} {context.ServerBase.Services.Count} {DateTime.Now}");
                        }
                        catch (Exception ex)
                        {
                            context.ServerBase.AutoLogger.LogError(ex, "GetClientClientContextService");
                        }
                        return null;
                    }
                    else
                    {
                        var attribName1 = typeof(T).GetClientServiceName();
                        var serviceType1 = context.ServerBase.GetRegisteredClientServiceTypeByName(attribName1);
                        var find1 = context.ServerBase.FindClientServerByType(context.Client, serviceType1);
                        if (find1 == null)
                        {
                            try
                            {
                                throw new Exception($"context client not exist 2 ! {context.Client.ClientId} {context.ServerBase.ClientServices.Count} {context.ServerBase.Services.Count} {DateTime.Now}");
                            }
                            catch (Exception ex)
                            {
                                context.ServerBase.AutoLogger.LogError(ex, "GetClientClientContextService 2");
                            }
                            return null;
                        }
                        return new ClientContext<T>(find1, context.Client);
                    }
                }
                var attribName = typeof(T).GetClientServiceName();

                var serviceType = context.ServerBase.GetRegisteredClientServiceTypeByName(attribName);
                var find = context.ServerBase.FindClientServerByType(context.Client, serviceType);
                if (find != null)
                    return new ClientContext<T>(find, context.Client);
                var obj = CSCodeInjection.InstanceServerInterface<T>(serviceType, new List<Type>() { typeof(ServiceContractAttribute) });
                //dynamic dobj = obj;
                if (CSCodeInjection.InvokedServerMethodAction == null)
                    ServerExtension.Init();

                var field = serviceType
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                .GetTypeInfo()
#endif
                .GetProperty("InvokedServerMethodAction");

                field.SetValue(obj, CSCodeInjection.InvokedServerMethodAction, null);

                var field2 = serviceType
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                .GetTypeInfo()
#endif
                .GetProperty("InvokedServerMethodFunction");

                field2.SetValue(obj, CSCodeInjection.InvokedServerMethodFunction, null);

                //dobj.InvokedServerMethodAction = CSCodeInjection.InvokedServerMethodAction;
                //dobj.InvokedServerMethodFunction = CSCodeInjection.InvokedServerMethodFunction;

                var op = obj as OperationCalls;
                op.ServerBase = context.ServerBase;
                op.CurrentClient = context.Client;

                context.ServerBase.ClientServices[context.Client].Add(obj);
                if (!(obj is OperationCalls))
                    context.ServerBase.AutoLogger.LogText("is not OprationCalls: " + obj.ToString(), true);

                return new ClientContext<T>(obj, context.Client);
            }
            else
            {
                context.ServerBase.AutoLogger.LogText("is not interface: " + typeof(T).ToString(), true);
                return new ClientContext<T>((T)context.ServerBase.FindClientServerByType(context.Client, typeof(T)), context.Client);

            }
        }

        static object FindClientService<T>(ServerBase serverBase, ClientInfo client, Type serviceType, string attribName)
        {
            var find = serverBase.FindClientServerByType(client, serviceType);
            if (find == null)
            {
                GetClientContextService<T>(serverBase, client);
                find = serverBase.FindClientServerByType(client, serviceType);
                if (find == null)
                    serverBase.AutoLogger.LogText($"FindClientService service not found : {serviceType.FullName} : name: {attribName} clientId: {client.ClientId}", true);
            }
            return find;
        }

        /// <summary>
        ///  get all client context service
        /// </summary>
        /// <typeparam name="T">type of service</typeparam>
        /// <param name="context">client context</param>
        /// <returns>list of service context</returns>
        public static List<ClientContext<T>> GetAllClientClientContextServices<T>(this OperationContext context)
        {
            var attribName = typeof(T).GetClientServiceName();
            var serviceType = context.ServerBase.GetRegisteredClientServiceTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();
            foreach (var item in context.ServerBase.Clients.Values.ToArray())
            {
                var find = FindClientService<T>(context.ServerBase, item, serviceType, attribName);

                if (find != null)
                    items.Add(new ClientContext<T>(find, item));
            }
            return items;
        }

        /// <summary>
        /// get all client context but ignore current context
        /// </summary>
        /// <typeparam name="T">type of service</typeparam>
        /// <param name="context">client context</param>
        /// <returns>list of service context</returns>
        public static List<ClientContext<T>> GetAllClientContextServicesButMe<T>(this OperationContext context)
        {
            var attribName = typeof(T).GetClientServiceName();
            var serviceType = context.ServerBase.GetRegisteredClientServiceTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();
            foreach (var item in context.ServerBase.Clients.Values.Where(x => x != context.Client).ToArray())
            {
                var find = FindClientService<T>(context.ServerBase, item, serviceType, attribName);
                if (find != null)
                    items.Add(new ClientContext<T>(find, item));
            }
            return items;
        }

        /// <summary>
        /// get clients service context by session list
        /// </summary>
        /// <typeparam name="T">service type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clientId">list of sessions</param>
        /// <returns>list of service context</returns>
        public static ClientContext<T> GetClientContextService<T>(this OperationContext context, string clientId)
        {
            var attribName = typeof(T).GetClientServiceName();
            var serviceType = context.ServerBase.GetRegisteredClientServiceTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();
            context.ServerBase.Clients.TryGetValue(clientId, out ClientInfo clientInfo);
            if (clientInfo != null)
            {
                var find = FindClientService<T>(context.ServerBase, clientInfo, serviceType, attribName);
                return new ClientContext<T>(find, clientInfo);
            }
            return null;
        }

        /// <summary>
        /// get clients service context by session list
        /// </summary>
        /// <typeparam name="T">service type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="client">client</param>
        /// <returns>list of service context</returns>
        public static ClientContext<T> GetClientContextService<T>(this OperationContext context, ClientInfo client)
        {
            var attribName = typeof(T).GetClientServiceName();
            var serviceType = context.ServerBase.GetRegisteredClientServiceTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();
            var find = FindClientService<T>(context.ServerBase, client, serviceType, attribName);


            return new ClientContext<T>(find, client);
        }

        /// <summary>
        /// get clients service context by session list
        /// </summary>
        /// <typeparam name="T">service type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clientId">id of client</param>
        /// <returns>list of service context</returns>
        public static T GetClientService<T>(this OperationContext context, string clientId)
        {
            var client = GetClientContextService<T>(context, clientId);
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
        public static T GetClientService<T>(this OperationContext context, ClientInfo client)
        {
            var result = GetClientContextService<T>(context, client);
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
        public static List<ClientContext<T>> GetListOfClientContextServices<T>(this OperationContext context, IEnumerable<ClientInfo> clients)
        {
            var attribName = typeof(T).GetClientServiceName();
            var serviceType = context.ServerBase.GetRegisteredClientServiceTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();
            foreach (var item in clients)
            {
                var find = FindClientService<T>(context.ServerBase, item, serviceType, attribName);

                if (find != null)
                    items.Add(new ClientContext<T>(find, item));
            }
            return items;
        }

        /// <summary>
        /// get clients service context by session list
        /// </summary>
        /// <typeparam name="T">service type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clientIds">list of sessions</param>
        /// <returns>list of service context</returns>
        public static List<ClientContext<T>> GetListOfClientContextServices<T>(this OperationContext context, IEnumerable<string> clientIds)
        {
            var attribName = typeof(T).GetClientServiceName();
            var serviceType = context.ServerBase.GetRegisteredClientServiceTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();

            foreach (var item in (from x in context.ServerBase.Clients.ToArray() where clientIds.Contains(x.Key) select x.Value))
            {
                var find = FindClientService<T>(context.ServerBase, item, serviceType, attribName);

                if (find != null)
                    items.Add(new ClientContext<T>(find, item));
            }
            return items;
        }

        /// <summary>
        /// get clients service context list and ignore custom session list
        /// </summary>
        /// <typeparam name="T">service type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clientIds">list of sessions to ingore get</param>
        /// <returns>list of service context</returns>
        public static List<ClientContext<T>> GetListOfExcludeClientContextServices<T>(this OperationContext context, IEnumerable<string> clientIds)
        {
            var attribName = typeof(T).GetClientServiceName();
            var serviceType = context.ServerBase.GetRegisteredClientServiceTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();
            foreach (var item in (from x in context.ServerBase.Clients.ToArray() where !clientIds.Contains(x.Key) select x.Value))
            {
                var find = FindClientService<T>(context.ServerBase, item, serviceType, attribName);
                if (find != null)
                    items.Add(new ClientContext<T>(find, item));
            }
            return items;
        }




        /// <summary>
        /// get current context service
        /// </summary>
        /// <typeparam name="T">type of service</typeparam>
        /// <param name="context">client context</param>
        /// <returns>list of service context</returns>
        public static T GetClientService<T>(this OperationContext context)
        {
            return GetClientClientContextService<T>(context).Service;
        }

        /// <summary>
        ///  get all client context service
        /// </summary>
        /// <typeparam name="T">type of service</typeparam>
        /// <param name="context">client context</param>
        /// <returns>list of service context</returns>
        public static List<T> GetAllClientServices<T>(this OperationContext context)
        {
            return (from x in GetAllClientClientContextServices<T>(context) select x.Service).ToList();
        }

        /// <summary>
        /// get all client context but ignore current context
        /// </summary>
        /// <typeparam name="T">type of service</typeparam>
        /// <param name="context">client context</param>
        /// <returns>list of service context</returns>
        public static List<T> GetAllClientServicesButMe<T>(this OperationContext context)
        {
            return (from x in GetAllClientContextServicesButMe<T>(context) select x.Service).ToList();
        }

        /// <summary>
        /// get client service by client list
        /// </summary>
        /// <typeparam name="T">type of service</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clients">clients of clients to get client service</param>
        /// <returns>list of service</returns>
        public static List<T> GetListOfClientServices<T>(this OperationContext context, IEnumerable<ClientInfo> clients)
        {
            return (from x in GetListOfClientContextServices<T>(context, clients) select x.Service).ToList();
        }

        /// <summary>
        /// get clients service by session list
        /// </summary>
        /// <typeparam name="T">service type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clientIds">list of sessions</param>
        /// <returns>list of service</returns>
        public static List<T> GetListOfClientServices<T>(this OperationContext context, IEnumerable<string> clientIds)
        {
            return (from x in GetListOfClientContextServices<T>(context, clientIds) select x.Service).ToList();
        }

        /// <summary>
        /// get clients services list and ignore custom session list
        /// </summary>
        /// <typeparam name="T">service type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clientIds">list of sessions to ingore get</param>
        /// <returns>list of services</returns>
        public static List<T> GetListOfExcludeClientServices<T>(this OperationContext context, IEnumerable<string> clientIds)
        {
            return (from x in GetListOfExcludeClientContextServices<T>(context, clientIds) select x.Service).ToList();
        }

























        /// <summary>
        /// get current context service
        /// </summary>
        /// <typeparam name="T">type of service</typeparam>
        /// <param name="serverBase">server context</param>
        /// <param name="client"></param>
        /// <returns>list of service context</returns>
        public static ClientContext<T> GetClientContextService<T>(this ServerBase serverBase, ClientInfo client)
        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            if (typeof(T).GetTypeInfo().IsInterface)
#else
            if (typeof(T).IsInterface)
#endif
            {
                if (!serverBase.ClientServices.ContainsKey(client))
                {
                    serverBase.RegisterClientServices(client);
                    if (!serverBase.ClientServices.ContainsKey(client))
                    {
                        try
                        {
                            throw new Exception($"context client not exist! {client.ClientId} {serverBase.ClientServices.Count} {serverBase.Services.Count} {DateTime.Now}");
                        }
                        catch (Exception ex)
                        {
                            serverBase.AutoLogger.LogError(ex, "GetClientContextService");
                        }
                        return null;
                    }
                }
                var attribName = typeof(T).GetClientServiceName();
                var serviceType = serverBase.GetRegisteredClientServiceTypeByName(attribName);
                var find = serverBase.FindClientServerByType(client, serviceType);
                if (find != null)
                    return new ClientContext<T>(find, client);
                var obj = CSCodeInjection.InstanceServerInterface<T>(serviceType, new List<Type>() { typeof(ServiceContractAttribute) });
                //dynamic dobj = obj;
                if (CSCodeInjection.InvokedServerMethodAction == null)
                    ServerExtension.Init();
                //dobj.InvokedServerMethodAction = CSCodeInjection.InvokedServerMethodAction;
                //dobj.InvokedServerMethodFunction = CSCodeInjection.InvokedServerMethodFunction;

                var field = serviceType
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                .GetTypeInfo()
#endif
                .GetProperty("InvokedServerMethodAction");

                field.SetValue(obj, CSCodeInjection.InvokedServerMethodAction, null);

                var field2 = serviceType
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                .GetTypeInfo()
#endif
                .GetProperty("InvokedServerMethodFunction");

                field2.SetValue(obj, CSCodeInjection.InvokedServerMethodFunction, null);

                var op = obj as OperationCalls;
                op.ServerBase = serverBase;
                op.CurrentClient = client;

                serverBase.ClientServices[client].Add(obj);
                if (!(obj is OperationCalls))
                    serverBase.AutoLogger.LogText("is not OprationCalls: " + obj.ToString(), true);

                return new ClientContext<T>(obj, client);
            }
            else
            {
                serverBase.AutoLogger.LogText("is not interface: " + typeof(T).ToString(), true);
                return new ClientContext<T>((T)serverBase.FindClientServerByType(client, typeof(T)), client);

            }
        }

        /// <summary>
        /// get all client context but ignore current context
        /// </summary>
        /// <typeparam name="T">type of service</typeparam>
        /// <param name="serverBase"></param>
        /// <param name="client"></param>
        /// <returns>list of service context</returns>
        public static List<ClientContext<T>> GetAllClientContextServicesButMe<T>(this ServerBase serverBase, ClientInfo client)
        {
            var attribName = typeof(T).GetClientServiceName();
            var serviceType = serverBase.GetRegisteredClientServiceTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();
            foreach (var item in serverBase.Clients.ToArray().Where(x => x.Value != client).Select(x => x.Value))
            {
                var find = FindClientService<T>(serverBase, item, serviceType, attribName);

                if (find != null)
                    items.Add(new ClientContext<T>(find, item));
            }
            return items;
        }

        /// <summary>
        /// get clients service context by session list
        /// </summary>
        /// <typeparam name="T">service type</typeparam>
        /// <param name="serverBase"></param>
        /// <param name="clientId">list of sessions</param>
        /// <returns>list of service context</returns>
        public static ClientContext<T> GetClientContextService<T>(this ServerBase serverBase, string clientId)
        {
            var attribName = typeof(T).GetClientServiceName();
            var serviceType = serverBase.GetRegisteredClientServiceTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();
            serverBase.Clients.TryGetValue(clientId, out ClientInfo clientInfo);
            if (clientInfo != null)
            {
                var find = FindClientService<T>(serverBase, clientInfo, serviceType, attribName);
                return new ClientContext<T>(find, clientInfo);
            }
            return null;
        }


        /// <summary>
        /// get clients service context by session list
        /// </summary>
        /// <typeparam name="T">service type</typeparam>
        /// <param name="serverBase"></param>
        /// <param name="clientId"></param>
        /// <returns>list of service context</returns>
        public static T GetClientService<T>(this ServerBase serverBase, string clientId)
        {
            var client = GetClientContextService<T>(serverBase, clientId);
            if (client != null)
                return client.Service;
            return default(T);
        }

        /// <summary>
        /// get clients service context by session list
        /// </summary>
        /// <typeparam name="T">service type</typeparam>
        /// <param name="serverBase"></param>
        /// <param name="client">client</param>
        /// <returns>service context</returns>
        public static T GetClientService<T>(this ServerBase serverBase, ClientInfo client)
        {
            var result = GetClientContextService<T>(serverBase, client);
            if (result != null)
                return result.Service;
            return default(T);
        }

        /// <summary>
        /// get client service context by client list
        /// </summary>
        /// <typeparam name="T">type of service</typeparam>
        /// <param name="serverBase"></param>
        /// <param name="clients">clients of clients to get client context</param>
        /// <returns>list of service context</returns>
        public static List<ClientContext<T>> GetListOfClientContextServices<T>(this ServerBase serverBase, IEnumerable<ClientInfo> clients)
        {
            var attribName = typeof(T).GetClientServiceName();
            var serviceType = serverBase.GetRegisteredClientServiceTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();
            foreach (var item in clients)
            {
                var find = FindClientService<T>(serverBase, item, serviceType, attribName);
                if (find != null)
                    items.Add(new ClientContext<T>(find, item));
            }
            return items;
        }

        /// <summary>
        /// get clients service context by session list
        /// </summary>
        /// <typeparam name="T">service type</typeparam>
        /// <param name="serverBase"></param>
        /// <param name="clientIds">list of sessions</param>
        /// <returns>list of service context</returns>
        public static List<ClientContext<T>> GetListOfClientContextServices<T>(this ServerBase serverBase, IEnumerable<string> clientIds)
        {
            var attribName = typeof(T).GetClientServiceName();
            var serviceType = serverBase.GetRegisteredClientServiceTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();
            foreach (var item in (from x in serverBase.Clients.ToArray() where clientIds.Contains(x.Key) select x.Value))
            {
                var find = FindClientService<T>(serverBase, item, serviceType, attribName);

                if (find != null)
                    items.Add(new ClientContext<T>(find, item));
            }
            return items;
        }

        /// <summary>
        /// get clients service context list and ignore custom session list
        /// </summary>
        /// <typeparam name="T">service type</typeparam>
        /// <param name="serverBase"></param>
        /// <param name="clientIds">list of sessions to ingore get</param>
        /// <returns>list of service context</returns>
        public static List<ClientContext<T>> GetListOfExcludeClientContextServices<T>(this ServerBase serverBase, IEnumerable<string> clientIds)
        {
            var attribName = typeof(T).GetClientServiceName();
            var serviceType = serverBase.GetRegisteredClientServiceTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();
            foreach (var item in (from x in serverBase.Clients.ToArray() where !clientIds.Contains(x.Key) select x.Value))
            {
                var find = FindClientService<T>(serverBase, item, serviceType, attribName);

                if (find != null)
                    items.Add(new ClientContext<T>(find, item));
            }
            return items;
        }


        /// <summary>
        /// get all client context but ignore current context
        /// </summary>
        /// <typeparam name="T">type of service</typeparam>
        /// <param name="serverBase"></param>
        /// <param name="client"></param>
        /// <returns>list of service context</returns>
        public static List<T> GetAllClientServicesButMe<T>(this ServerBase serverBase, ClientInfo client)
        {
            return (from x in GetAllClientContextServicesButMe<T>(serverBase, client) select x.Service).ToList();
        }

        /// <summary>
        /// get client service context by client list
        /// </summary>
        /// <typeparam name="T">type of service</typeparam>
        /// <param name="serverBase"></param>
        /// <param name="clients">clients of clients to get client context</param>
        /// <returns>list of service context</returns>
        public static List<T> GetListOfClientServices<T>(this ServerBase serverBase, IEnumerable<ClientInfo> clients)
        {
            return (from x in GetListOfClientContextServices<T>(serverBase, clients) select x.Service).ToList();
        }

        /// <summary>
        /// get client service context by client list
        /// </summary>
        /// <typeparam name="T">type of service</typeparam>
        /// <param name="serverBase"></param>
        /// <param name="clientIds">clients of clients to get client context</param>
        /// <returns>list of service context</returns>
        public static List<T> GetListOfClientServices<T>(this ServerBase serverBase, IEnumerable<string> clientIds)
        {
            return (from x in GetListOfClientContextServices<T>(serverBase, clientIds) select x.Service).ToList();
        }

        /// <summary>
        /// get clients callback context list and ignore custom session list
        /// </summary>
        /// <typeparam name="T">callback type</typeparam>
        /// <param name="serverBase"></param>
        /// <param name="clientIds">list of sessions to ingore get</param>
        /// <returns>list of callback context</returns>
        public static List<T> GetListOfExcludeClientServices<T>(this ServerBase serverBase, IEnumerable<string> clientIds)
        {
            return (from x in GetListOfExcludeClientContextServices<T>(serverBase, clientIds) select x.Service).ToList();
        }

        /// <summary>
        /// get all client services
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serverBase"></param>
        /// <returns></returns>
        public static List<ClientContext<T>> GetAllClientContextServices<T>(this ServerBase serverBase)
        {
            var attribName = typeof(T).GetClientServiceName();
            var serviceType = serverBase.GetRegisteredClientServiceTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();
            foreach (var item in serverBase.Clients.ToArray())
            {
                var find = FindClientService<T>(serverBase, item.Value, serviceType, attribName);

                if (find != null)
                    items.Add(new ClientContext<T>(find, item.Value));
            }
            return items;
        }

        /// <summary>
        /// get all client services
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serverBase"></param>
        /// <returns></returns>
        public static List<T> GetAllClientServices<T>(this ServerBase serverBase)
        {
            var attribName = typeof(T).GetClientServiceName();
            var serviceType = serverBase.GetRegisteredClientServiceTypeByName(attribName);
            List<T> items = new List<T>();
            foreach (var item in serverBase.Clients.ToArray())
            {
                var find = FindClientService<T>(serverBase, item.Value, serviceType, attribName);

                if (find != null)
                    items.Add((T)find);
            }
            return items;
        }
    }
}
