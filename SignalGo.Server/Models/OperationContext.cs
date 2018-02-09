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
                return null;
            }
        }

        public ServerBase ServerBase { get; set; }
        public ClientInfo Client { get; private set; }
        public HttpClientInfo HttpClient
        {
            get
            {
                return (HttpClientInfo)Client;
            }
        }
        public IEnumerable<ClientInfo> Clients { get; private set; }

        public List<ClientInfo> AllServerClients
        {
            get
            {
                return ServerBase.Clients.Values.ToList();
            }
        }

        public int ConnectedClientsCount
        {
            get
            {
                return ServerBase.Clients.Count;
            }
        }

        public T GetService<T>() where T : class
        {
            var attribute = typeof(T).GetCustomAttributes<ServiceContractAttribute>(true).FirstOrDefault();
            ServerBase.SingleInstanceServices.TryGetValue(attribute.Name, out object result);
            return (T)result;
        }

        public ClientInfo GetClientInfoByClientId(string clientId)
        {
            return Current.ServerBase.GetClientByClientId(clientId);
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
                var property = type.GetListOfProperties().Select(x => new { Info = x, Attribute = x.GetCustomAttributes<HttpKeyAttribute>().FirstOrDefault(y => !y.IsExpireField), ExpiredAttribute = x.GetCustomAttributes<HttpKeyAttribute>().FirstOrDefault(y => y.IsExpireField) }).FirstOrDefault(x => x.Attribute != null);

                if (property == null)
                    throw new Exception("HttpKeyAttribute on your one properties on class not found please made your string property that have HttpKeyAttribute on the top!");
                else if (property.Info.PropertyType != typeof(string))
                    throw new Exception("type of your HttpKeyAttribute must be as string because this will used for headers of http calls and you must made it custom");

                var httpClient = context.Client as HttpClientInfo;
                if (httpClient.RequestHeaders == null || string.IsNullOrEmpty(httpClient.RequestHeaders[property.Attribute.RequestHeaderName]))
                    return null;
                var key = ExtractValue(httpClient.RequestHeaders[property.Attribute.RequestHeaderName], property.Attribute.KeyName, property.Attribute.HeaderValueSeparate, property.Attribute.HeaderKeyValueSeparate);
                if (CustomClientSavedSettings.TryGetValue(key, out HashSet<object> result))
                {
                    var obj = result.FirstOrDefault(x => x.GetType() == type);
                    if (obj == null)
                        return null;
                    if (property.ExpiredAttribute != null && property.ExpiredAttribute.CheckIsExpired(obj))
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
                return data.Split(new string[] { valueSeparateChar }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            }
            else if (string.IsNullOrEmpty(valueSeparateChar))
            {
                return data.Split(new string[] { keyValueSeparateChar }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            }
            else
            {
                foreach (var keyValue in data.Split(new string[] { valueSeparateChar }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var separate = keyValue.Split(new string[] { keyValueSeparateChar }, StringSplitOptions.RemoveEmptyEntries);
                    if (string.IsNullOrEmpty(separate.FirstOrDefault()))
                        continue;
                    if (separate.FirstOrDefault().ToLower() == keyName.ToLower())
                        return separate.Length > 1 ? separate.LastOrDefault() : "";
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
                    var property = typeof(T).GetListOfProperties().Select(x => new { Info = x, Attribute = x.GetCustomAttributes<HttpKeyAttribute>().FirstOrDefault() }).FirstOrDefault(x => x.Attribute != null);
                    if (property == null)
                        throw new Exception("HttpKeyAttribute on your one properties on class not found please made your string property that have HttpKeyAttribute on the top!");
                    else if (property.Info.PropertyType != typeof(string))
                        throw new Exception("type of your HttpKeyAttribute must be as string because this will used for headers of http calls and you must made it custom");
                    var key = (string)property.Info.GetValue(value, null);
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
        /// get current context callback
        /// </summary>
        /// <typeparam name="T">type of callback</typeparam>
        /// <param name="context">client context</param>
        /// <returns>list of callback context</returns>
        public static ClientContext<T> GetClientCallbackOfClientContext<T>(this OperationContext context)
        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            if (typeof(T).GetTypeInfo().IsInterface)
#else
            if (typeof(T).IsInterface)
#endif
            {
                if (!context.ServerBase.Callbacks.ContainsKey(context.Client))
                {
                    context.ServerBase.RegisterCallbacksForClient(context.Client);
                    if (!context.ServerBase.Callbacks.ContainsKey(context.Client))
                    {
                        try
                        {
                            throw new Exception($"context client not exist! {context.Client.ClientId} {context.ServerBase.Callbacks.Count} {context.ServerBase.Services.Count} {DateTime.Now}");
                        }
                        catch (Exception ex)
                        {
                            AutoLogger.LogError(ex, "GetClientCallbackOfClientContext");
                        }
                        return null;
                    }
                    else
                    {
                        var attribName1 = (typeof(T).GetCustomAttributes<ServiceContractAttribute>(true).FirstOrDefault()).Name;
                        var serviceType1 = context.ServerBase.GetRegisteredCallbacksTypeByName(attribName1);
                        var find1 = context.ServerBase.FindClientCallbackByType(context.Client, serviceType1);
                        if (find1 == null)
                        {
                            try
                            {
                                throw new Exception($"context client not exist 2 ! {context.Client.ClientId} {context.ServerBase.Callbacks.Count} {context.ServerBase.Services.Count} {DateTime.Now}");
                            }
                            catch (Exception ex)
                            {
                                AutoLogger.LogError(ex, "GetClientCallbackOfClientContext 2");
                            }
                            return null;
                        }
                        return new ClientContext<T>(find1, context.Client);
                    }
                }
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                var attribName = ((ServiceContractAttribute)typeof(T).GetTypeInfo().GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#else
                var attribName = ((ServiceContractAttribute)typeof(T).GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#endif
                var serviceType = context.ServerBase.GetRegisteredCallbacksTypeByName(attribName);
                var find = context.ServerBase.FindClientCallbackByType(context.Client, serviceType);
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
                //context.ServerBase.RegisteredCallbacksTypes.ContainsKey(attribute.Name);

                context.ServerBase.Callbacks[context.Client].Add(obj);
                if (!(obj is OperationCalls))
                    Shared.Log.AutoLogger.LogText("is not OprationCalls: " + obj.ToString(), true);

                return new ClientContext<T>(obj, context.Client);
            }
            else
            {
                Shared.Log.AutoLogger.LogText("is not interface: " + typeof(T).ToString(), true);
                return new ClientContext<T>((T)context.ServerBase.FindClientCallbackByType(context.Client, typeof(T)), context.Client);

            }
        }

        static object FindClientCallback<T>(ServerBase serverBase, ClientInfo client, Type serviceType, string attribName)
        {
            var find = serverBase.FindClientCallbackByType(client, serviceType);
            if (find == null)
            {
                GetClientCallbackOfClientContext<T>(serverBase, client);
                find = serverBase.FindClientCallbackByType(client, serviceType);
                if (find == null)
                    AutoLogger.LogText($"GetAllClientCallbackListOfClientContext service not found : {serviceType.FullName} : name: {attribName} session: {client.ClientId}", true);
            }
            return find;
        }

        //public static object GetClientCallbackOfClientContext(ServerBase server, ClientInfo client, Type type)
        //{
        //    if (!server.Callbacks.ContainsKey(client))
        //    {
        //        try
        //        {
        //            throw new Exception($"context client not exist! {client.clientId} {server.Callbacks.Count} {DateTime.Now}");
        //        }
        //        catch (Exception ex)
        //        {
        //            AutoLogger.LogError(ex, "GetClientCallbackOfClientContext");
        //        }
        //        return null;
        //    }
        //    var attribName = ((ServiceContractAttribute)type.GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
        //    var serviceType = server.GetRegisteredCallbacksTypeByName(attribName);
        //    var find = server.FindClientCallbackByType(client, serviceType);
        //    if (find != null)
        //        return new ClientContext<object>(find, client);
        //    var obj = CSCodeInjection.InstanceServerInterface(serviceType, new List<Type>() { typeof(ServiceContractAttribute) });
        //    dynamic dobj = obj;
        //    if (CSCodeInjection.InvokedServerMethodAction == null)
        //        ServerExtension.Init();
        //    dobj.InvokedServerMethodAction = CSCodeInjection.InvokedServerMethodAction;
        //    dobj.InvokedServerMethodFunction = CSCodeInjection.InvokedServerMethodFunction;

        //    var op = obj as OperationCalls;
        //    op.ServerBase = server;
        //    op.CurrentClient = client;
        //    //context.ServerBase.RegisteredCallbacksTypes.ContainsKey(attribute.Name);

        //    server.Callbacks[client].Add(obj);
        //    if (!(obj is OperationCalls))
        //        Shared.Log.AutoLogger.LogText("is not OprationCalls: " + obj.ToString(), true);

        //    return new ClientContext<object>(obj, client);
        //}
        /// <summary>
        ///  get all client context callback
        /// </summary>
        /// <typeparam name="T">type of callback</typeparam>
        /// <param name="context">client context</param>
        /// <returns>list of callback context</returns>
        public static List<ClientContext<T>> GetAllClientCallbackListOfClientContext<T>(this OperationContext context)
        {
            var attribName = (typeof(T).GetCustomAttributes<ServiceContractAttribute>(true).FirstOrDefault()).Name;
            var serviceType = context.ServerBase.GetRegisteredCallbacksTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();
            foreach (var item in context.ServerBase.Clients.Values.ToArray())
            {
                var find = FindClientCallback<T>(context.ServerBase, item, serviceType, attribName);

                if (find != null)
                    items.Add(new ClientContext<T>(find, item));
            }
            return items;
        }

        /// <summary>
        /// get all client context but ignore current context
        /// </summary>
        /// <typeparam name="T">type of callback</typeparam>
        /// <param name="context">client context</param>
        /// <returns>list of callback context</returns>
        public static List<ClientContext<T>> GetAllClientCallbackListButMeOfClientContext<T>(this OperationContext context)
        {
            var attribName = (typeof(T).GetCustomAttributes<ServiceContractAttribute>(true).FirstOrDefault()).Name;
            var serviceType = context.ServerBase.GetRegisteredCallbacksTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();
            foreach (var item in context.ServerBase.Clients.Values.Where(x => x != context.Client).ToArray())
            {
                var find = FindClientCallback<T>(context.ServerBase, item, serviceType, attribName);
                if (find != null)
                    items.Add(new ClientContext<T>(find, item));
            }
            return items;
        }

        /// <summary>
        /// get clients callback context by session list
        /// </summary>
        /// <typeparam name="T">callback type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clientId">list of sessions</param>
        /// <returns>list of callback context</returns>
        public static ClientContext<T> GetClientCallbackOfClientContext<T>(this OperationContext context, string clientId)
        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var attribName = ((ServiceContractAttribute)typeof(T).GetTypeInfo().GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#else
            var attribName = ((ServiceContractAttribute)typeof(T).GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#endif
            var serviceType = context.ServerBase.GetRegisteredCallbacksTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();
            context.ServerBase.Clients.TryGetValue(clientId, out ClientInfo clientInfo);
            if (clientInfo != null)
            {
                var find = FindClientCallback<T>(context.ServerBase, clientInfo, serviceType, attribName);
                return new ClientContext<T>(find, clientInfo);
            }
            return null;
        }

        /// <summary>
        /// get clients callback context by session list
        /// </summary>
        /// <typeparam name="T">callback type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="client">client</param>
        /// <returns>list of callback context</returns>
        public static ClientContext<T> GetClientCallbackOfClientContext<T>(this OperationContext context, ClientInfo client)
        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var attribName = ((ServiceContractAttribute)typeof(T).GetTypeInfo().GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#else
            var attribName = ((ServiceContractAttribute)typeof(T).GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#endif
            var serviceType = context.ServerBase.GetRegisteredCallbacksTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();
            var find = FindClientCallback<T>(context.ServerBase, client, serviceType, attribName);


            return new ClientContext<T>(find, client);
        }

        /// <summary>
        /// get clients callback context by session list
        /// </summary>
        /// <typeparam name="T">callback type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clientIds">list of sessions</param>
        /// <returns>list of callback context</returns>
        public static T GetClientCallback<T>(this OperationContext context, string clientId)
        {
            var client = GetClientCallbackOfClientContext<T>(context, clientId);
            if (client != null)
                return client.Service;
            return default(T);
        }

        /// <summary>
        /// get clients callback context by session list
        /// </summary>
        /// <typeparam name="T">callback type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="client">client</param>
        /// <returns>list of callback context</returns>
        public static T GetClientCallback<T>(this OperationContext context, ClientInfo client)
        {
            var result = GetClientCallbackOfClientContext<T>(context, client);
            if (result != null)
                return result.Service;
            return default(T);
        }

        /// <summary>
        /// get client callback context by client list
        /// </summary>
        /// <typeparam name="T">type of callback</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clients">clients of clients to get client context</param>
        /// <returns>list of callback context</returns>
        public static List<ClientContext<T>> GetQueryClientCallbackListOfClientContext<T>(this OperationContext context, IEnumerable<ClientInfo> clients)
        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var attribName = ((ServiceContractAttribute)typeof(T).GetTypeInfo().GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#else
            var attribName = ((ServiceContractAttribute)typeof(T).GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#endif
            var serviceType = context.ServerBase.GetRegisteredCallbacksTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();
            foreach (var item in clients)
            {
                var find = FindClientCallback<T>(context.ServerBase, item, serviceType, attribName);

                if (find != null)
                    items.Add(new ClientContext<T>(find, item));
            }
            return items;
        }

        /// <summary>
        /// get clients callback context by session list
        /// </summary>
        /// <typeparam name="T">callback type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clientIds">list of sessions</param>
        /// <returns>list of callback context</returns>
        public static List<ClientContext<T>> GetQueryClientCallbackListOfClientContext<T>(this OperationContext context, IEnumerable<string> clientIds)
        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var attribName = ((ServiceContractAttribute)typeof(T).GetTypeInfo().GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#else
            var attribName = ((ServiceContractAttribute)typeof(T).GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#endif
            var serviceType = context.ServerBase.GetRegisteredCallbacksTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();

            foreach (var item in (from x in context.ServerBase.Clients.ToArray() where clientIds.Contains(x.Key) select x.Value))
            {
                var find = FindClientCallback<T>(context.ServerBase, item, serviceType, attribName);

                if (find != null)
                    items.Add(new ClientContext<T>(find, item));
            }
            return items;
        }

        /// <summary>
        /// get clients callback context list and ignore custom session list
        /// </summary>
        /// <typeparam name="T">callback type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clientIds">list of sessions to ingore get</param>
        /// <returns>list of callback context</returns>
        public static List<ClientContext<T>> GetQueryClientCallbackWithoutListOfClientContext<T>(this OperationContext context, IEnumerable<string> clientIds)
        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var attribName = ((ServiceContractAttribute)typeof(T).GetTypeInfo().GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#else
            var attribName = ((ServiceContractAttribute)typeof(T).GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#endif
            var serviceType = context.ServerBase.GetRegisteredCallbacksTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();
            foreach (var item in (from x in context.ServerBase.Clients.ToArray() where !clientIds.Contains(x.Key) select x.Value))
            {
                var find = FindClientCallback<T>(context.ServerBase, item, serviceType, attribName);
                if (find != null)
                    items.Add(new ClientContext<T>(find, item));
            }
            return items;
        }




        /// <summary>
        /// get current context callback
        /// </summary>
        /// <typeparam name="T">type of callback</typeparam>
        /// <param name="context">client context</param>
        /// <returns>list of callback context</returns>
        public static T GetClientCallback<T>(this OperationContext context)
        {
            return GetClientCallbackOfClientContext<T>(context).Service;
        }

        /// <summary>
        ///  get all client context callback
        /// </summary>
        /// <typeparam name="T">type of callback</typeparam>
        /// <param name="context">client context</param>
        /// <returns>list of callback context</returns>
        public static List<T> GetAllClientCallbackList<T>(this OperationContext context)
        {
            return (from x in GetAllClientCallbackListOfClientContext<T>(context) select x.Service).ToList();
        }

        /// <summary>
        /// get all client context but ignore current context
        /// </summary>
        /// <typeparam name="T">type of callback</typeparam>
        /// <param name="context">client context</param>
        /// <returns>list of callback context</returns>
        public static List<T> GetAllClientCallbackListButMe<T>(this OperationContext context)
        {
            return (from x in GetAllClientCallbackListButMeOfClientContext<T>(context) select x.Service).ToList();
        }

        /// <summary>
        /// get client callback context by client list
        /// </summary>
        /// <typeparam name="T">type of callback</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clients">clients of clients to get client context</param>
        /// <returns>list of callback context</returns>
        public static List<T> GetQueryClientCallbackList<T>(this OperationContext context, IEnumerable<ClientInfo> clients)
        {
            return (from x in GetQueryClientCallbackListOfClientContext<T>(context, clients) select x.Service).ToList();
        }

        /// <summary>
        /// get clients callback context by session list
        /// </summary>
        /// <typeparam name="T">callback type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clientIds">list of sessions</param>
        /// <returns>list of callback context</returns>
        public static List<T> GetQueryClientCallbackList<T>(this OperationContext context, IEnumerable<string> clientIds)
        {
            return (from x in GetQueryClientCallbackListOfClientContext<T>(context, clientIds) select x.Service).ToList();
        }

        /// <summary>
        /// get clients callback context list and ignore custom session list
        /// </summary>
        /// <typeparam name="T">callback type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clientIds">list of sessions to ingore get</param>
        /// <returns>list of callback context</returns>
        public static List<T> GetQueryClientCallbackWithoutList<T>(this OperationContext context, IEnumerable<string> clientIds)
        {
            return (from x in GetQueryClientCallbackWithoutListOfClientContext<T>(context, clientIds) select x.Service).ToList();
        }

























        /// <summary>
        /// get current context callback
        /// </summary>
        /// <typeparam name="T">type of callback</typeparam>
        /// <param name="context">client context</param>
        /// <returns>list of callback context</returns>
        public static ClientContext<T> GetClientCallbackOfClientContext<T>(this ServerBase serverBase, ClientInfo client)
        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            if (typeof(T).GetTypeInfo().IsInterface)
#else
            if (typeof(T).IsInterface)
#endif
            {
                if (!serverBase.Callbacks.ContainsKey(client))
                {
                    serverBase.RegisterCallbacksForClient(client);
                    if (!serverBase.Callbacks.ContainsKey(client))
                    {
                        try
                        {
                            throw new Exception($"context client not exist! {client.ClientId} {serverBase.Callbacks.Count} {serverBase.Services.Count} {DateTime.Now}");
                        }
                        catch (Exception ex)
                        {
                            AutoLogger.LogError(ex, "GetClientCallbackOfClientContext");
                        }
                        return null;
                    }
                }
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                var attribName = ((ServiceContractAttribute)typeof(T).GetTypeInfo().GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#else
                var attribName = ((ServiceContractAttribute)typeof(T).GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#endif
                var serviceType = serverBase.GetRegisteredCallbacksTypeByName(attribName);
                var find = serverBase.FindClientCallbackByType(client, serviceType);
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
                //serverBase.RegisteredCallbacksTypes.ContainsKey(attribute.Name);

                serverBase.Callbacks[client].Add(obj);
                if (!(obj is OperationCalls))
                    Shared.Log.AutoLogger.LogText("is not OprationCalls: " + obj.ToString(), true);

                return new ClientContext<T>(obj, client);
            }
            else
            {
                Shared.Log.AutoLogger.LogText("is not interface: " + typeof(T).ToString(), true);
                return new ClientContext<T>((T)serverBase.FindClientCallbackByType(client, typeof(T)), client);

            }
        }

        /// <summary>
        /// get all client context but ignore current context
        /// </summary>
        /// <typeparam name="T">type of callback</typeparam>
        /// <param name="context">client context</param>
        /// <returns>list of callback context</returns>
        public static List<ClientContext<T>> GetAllClientCallbackListButMeOfClientContext<T>(this ServerBase serverBase, ClientInfo client)
        {
            var attribName = (typeof(T).GetCustomAttributes<ServiceContractAttribute>(true).FirstOrDefault()).Name;
            var serviceType = serverBase.GetRegisteredCallbacksTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();
            foreach (var item in serverBase.Clients.ToArray().Where(x => x.Value != client).Select(x => x.Value))
            {
                var find = FindClientCallback<T>(serverBase, item, serviceType, attribName);

                if (find != null)
                    items.Add(new ClientContext<T>(find, item));
            }
            return items;
        }

        /// <summary>
        /// get clients callback context by session list
        /// </summary>
        /// <typeparam name="T">callback type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clientId">list of sessions</param>
        /// <returns>list of callback context</returns>
        public static ClientContext<T> GetClientCallbackOfClientContext<T>(this ServerBase serverBase, string clientId)
        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var attribName = ((ServiceContractAttribute)typeof(T).GetTypeInfo().GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#else
            var attribName = ((ServiceContractAttribute)typeof(T).GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#endif
            var serviceType = serverBase.GetRegisteredCallbacksTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();
            serverBase.Clients.TryGetValue(clientId, out ClientInfo clientInfo);
            if (clientInfo != null)
            {
                var find = FindClientCallback<T>(serverBase, clientInfo, serviceType, attribName);
                return new ClientContext<T>(find, clientInfo);
            }
            return null;
        }


        /// <summary>
        /// get clients callback context by session list
        /// </summary>
        /// <typeparam name="T">callback type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clientIds">list of sessions</param>
        /// <returns>list of callback context</returns>
        public static T GetClientCallback<T>(this ServerBase serverBase, string clientId)
        {
            var client = GetClientCallbackOfClientContext<T>(serverBase, clientId);
            if (client != null)
                return client.Service;
            return default(T);
        }

        /// <summary>
        /// get clients callback context by session list
        /// </summary>
        /// <typeparam name="T">callback type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="client">client</param>
        /// <returns>list of callback context</returns>
        public static T GetClientCallback<T>(this ServerBase serverBase, ClientInfo client)
        {
            var result = GetClientCallbackOfClientContext<T>(serverBase, client);
            if (result != null)
                return result.Service;
            return default(T);
        }

        /// <summary>
        /// get client callback context by client list
        /// </summary>
        /// <typeparam name="T">type of callback</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clients">clients of clients to get client context</param>
        /// <returns>list of callback context</returns>
        public static List<ClientContext<T>> GetQueryClientCallbackListOfClientContext<T>(this ServerBase serverBase, IEnumerable<ClientInfo> clients)
        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var attribName = ((ServiceContractAttribute)typeof(T).GetTypeInfo().GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#else
            var attribName = ((ServiceContractAttribute)typeof(T).GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#endif
            var serviceType = serverBase.GetRegisteredCallbacksTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();
            foreach (var item in clients)
            {
                var find = FindClientCallback<T>(serverBase, item, serviceType, attribName);
                if (find != null)
                    items.Add(new ClientContext<T>(find, item));
            }
            return items;
        }

        /// <summary>
        /// get clients callback context by session list
        /// </summary>
        /// <typeparam name="T">callback type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clientIds">list of sessions</param>
        /// <returns>list of callback context</returns>
        public static List<ClientContext<T>> GetQueryClientCallbackListOfClientContext<T>(this ServerBase serverBase, IEnumerable<string> clientIds)
        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var attribName = ((ServiceContractAttribute)typeof(T).GetTypeInfo().GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#else
            var attribName = ((ServiceContractAttribute)typeof(T).GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#endif
            var serviceType = serverBase.GetRegisteredCallbacksTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();
            foreach (var item in (from x in serverBase.Clients.ToArray() where clientIds.Contains(x.Key) select x.Value))
            {
                var find = FindClientCallback<T>(serverBase, item, serviceType, attribName);

                if (find != null)
                    items.Add(new ClientContext<T>(find, item));
            }
            return items;
        }

        /// <summary>
        /// get clients callback context list and ignore custom session list
        /// </summary>
        /// <typeparam name="T">callback type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clientIds">list of sessions to ingore get</param>
        /// <returns>list of callback context</returns>
        public static List<ClientContext<T>> GetQueryClientCallbackWithoutListOfClientContext<T>(this ServerBase serverBase, IEnumerable<string> clientIds)
        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var attribName = ((ServiceContractAttribute)typeof(T).GetTypeInfo().GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#else
            var attribName = ((ServiceContractAttribute)typeof(T).GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#endif
            var serviceType = serverBase.GetRegisteredCallbacksTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();
            foreach (var item in (from x in serverBase.Clients.ToArray() where !clientIds.Contains(x.Key) select x.Value))
            {
                var find = FindClientCallback<T>(serverBase, item, serviceType, attribName);

                if (find != null)
                    items.Add(new ClientContext<T>(find, item));
            }
            return items;
        }


        /// <summary>
        /// get all client context but ignore current context
        /// </summary>
        /// <typeparam name="T">type of callback</typeparam>
        /// <param name="context">client context</param>
        /// <returns>list of callback context</returns>
        public static List<T> GetAllClientCallbackListButMe<T>(this ServerBase serverBase, ClientInfo client)
        {
            return (from x in GetAllClientCallbackListButMeOfClientContext<T>(serverBase, client) select x.Service).ToList();
        }

        /// <summary>
        /// get client callback context by client list
        /// </summary>
        /// <typeparam name="T">type of callback</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clients">clients of clients to get client context</param>
        /// <returns>list of callback context</returns>
        public static List<T> GetQueryClientCallbackList<T>(this ServerBase serverBase, IEnumerable<ClientInfo> clients)
        {
            return (from x in GetQueryClientCallbackListOfClientContext<T>(serverBase, clients) select x.Service).ToList();
        }

        /// <summary>
        /// get clients callback context by session list
        /// </summary>
        /// <typeparam name="T">callback type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clientIds">list of sessions</param>
        /// <returns>list of callback context</returns>
        public static List<T> GetQueryClientCallbackList<T>(this ServerBase serverBase, IEnumerable<string> clientIds)
        {
            return (from x in GetQueryClientCallbackListOfClientContext<T>(serverBase, clientIds) select x.Service).ToList();
        }

        /// <summary>
        /// get clients callback context list and ignore custom session list
        /// </summary>
        /// <typeparam name="T">callback type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="clientIds">list of sessions to ingore get</param>
        /// <returns>list of callback context</returns>
        public static List<T> GetQueryClientCallbackWithoutList<T>(this ServerBase serverBase, IEnumerable<string> clientIds)
        {
            return (from x in GetQueryClientCallbackWithoutListOfClientContext<T>(serverBase, clientIds) select x.Service).ToList();
        }
    }
}
