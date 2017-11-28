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
        public IEnumerable<ClientInfo> Clients { get; private set; }

        public T GetService<T>() where T : class
        {
            var attribute = typeof(T).GetCustomAttributes<ServiceContractAttribute>(true).FirstOrDefault();
            ServerBase.SingleInstanceServices.TryGetValue(attribute.Name, out object result);
            return (T)result;
        }
    }

    /// <summary>
    /// operation contract for client that help you to save a class and get it later inside of your service class
    /// </summary>
    /// <typeparam name="T">type of your setting</typeparam>
    public class OperationContext<T> where T : class
    {
        static ConcurrentDictionary<ClientInfo, HashSet<object>> savedSettings = new ConcurrentDictionary<ClientInfo, HashSet<object>>();
        static T _Current = null;
        /// <summary>
        /// get seeting of one type that you set it
        /// </summary>
        public static T CurrentSetting
        {
            get
            {
                var context = OperationContext.Current;
                if (context == null)
                    throw new Exception("SynchronizationContext is null or empty! Do not call this property inside of another thread that do not have any synchronizationContext or you can call SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());");

                if (savedSettings.TryGetValue(context.Client, out HashSet<object> result))
                {
                    return (T)result.FirstOrDefault(x => x.GetType() == typeof(T));
                }
                return null;
            }
            set
            {
                SetSetting(value);
            }
        }

        /// <summary>
        /// set setting for this client
        /// </summary>
        /// <param name="setting"></param>
        public static void SetSetting(object setting)
        {
            var context = OperationContext.Current;
            if (SynchronizationContext.Current == null)
                throw new Exception("SynchronizationContext is null or empty! Do not call this property inside of another thread that do not have any synchronizationContext or you can call SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());");
            if (!savedSettings.ContainsKey(context.Client))
                savedSettings.TryAdd(context.Client, new HashSet<object>() { setting });
            else if (savedSettings.TryGetValue(context.Client, out HashSet<object> result) && !result.Contains(setting))
                result.Add(setting);
        }

        /// <summary>
        /// get first setting of type that setted
        /// </summary>
        /// <typeparam name="T">type of setting</typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetSettings<T>()
        {
            var context = OperationContext.Current;
            if (SynchronizationContext.Current == null)
                throw new Exception("SynchronizationContext is null or empty! Do not call this property inside of another thread that do not have any synchronizationContext or you can call SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());");
            if (savedSettings.TryGetValue(context.Client, out HashSet<object> result))
            {
                return result.Where(x => x.GetType() == typeof(T)).Select(x => (T)x);
            }
            return null;
        }

        /// <summary>
        /// get all settings of client
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object> GetSettings()
        {
            var context = OperationContext.Current;
            if (SynchronizationContext.Current == null)
                throw new Exception("SynchronizationContext is null or empty! Do not call this property inside of another thread that do not have any synchronizationContext or you can call SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());");
            if (savedSettings.TryGetValue(context.Client, out HashSet<object> result))
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
                            throw new Exception($"context client not exist! {context.Client.SessionId} {context.ServerBase.Callbacks.Count} {context.ServerBase.Services.Count} {DateTime.Now}");
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
                                throw new Exception($"context client not exist 2 ! {context.Client.SessionId} {context.ServerBase.Callbacks.Count} {context.ServerBase.Services.Count} {DateTime.Now}");
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
                    AutoLogger.LogText($"GetAllClientCallbackListOfClientContext service not found : {serviceType.FullName} : name: {attribName} session: {client.SessionId}", true);
            }
            return find;
        }

        //public static object GetClientCallbackOfClientContext(ServerBase server, ClientInfo client, Type type)
        //{
        //    if (!server.Callbacks.ContainsKey(client))
        //    {
        //        try
        //        {
        //            throw new Exception($"context client not exist! {client.SessionId} {server.Callbacks.Count} {DateTime.Now}");
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
            foreach (var item in context.ServerBase.Clients.ToArray())
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
            foreach (var item in context.ServerBase.Clients.ToArray())
            {
                if (item == context.Client)
                    continue;
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
        /// <param name="sessionId">list of sessions</param>
        /// <returns>list of callback context</returns>
        public static ClientContext<T> GetClientCallbackOfClientContext<T>(this OperationContext context, string sessionId)
        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var attribName = ((ServiceContractAttribute)typeof(T).GetTypeInfo().GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#else
            var attribName = ((ServiceContractAttribute)typeof(T).GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#endif
            var serviceType = context.ServerBase.GetRegisteredCallbacksTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();
            var client = (from x in context.ServerBase.Clients.ToArray() where sessionId == x.SessionId select x).FirstOrDefault();
            if (client != null)
            {
                var find = FindClientCallback<T>(context.ServerBase, client, serviceType, attribName);
                return new ClientContext<T>(find, client);
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
        /// <param name="sessionIds">list of sessions</param>
        /// <returns>list of callback context</returns>
        public static T GetClientCallback<T>(this OperationContext context, string sessionId)
        {
            var client = GetClientCallbackOfClientContext<T>(context, sessionId);
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
        /// <param name="sessionIds">list of sessions</param>
        /// <returns>list of callback context</returns>
        public static List<ClientContext<T>> GetQueryClientCallbackListOfClientContext<T>(this OperationContext context, IEnumerable<string> sessionIds)
        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var attribName = ((ServiceContractAttribute)typeof(T).GetTypeInfo().GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#else
            var attribName = ((ServiceContractAttribute)typeof(T).GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#endif
            var serviceType = context.ServerBase.GetRegisteredCallbacksTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();
            foreach (var item in (from x in context.ServerBase.Clients.ToArray() where sessionIds.Contains(x.SessionId) select x))
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
        /// <param name="sessionIds">list of sessions to ingore get</param>
        /// <returns>list of callback context</returns>
        public static List<ClientContext<T>> GetQueryClientCallbackWithoutListOfClientContext<T>(this OperationContext context, IEnumerable<string> sessionIds)
        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var attribName = ((ServiceContractAttribute)typeof(T).GetTypeInfo().GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#else
            var attribName = ((ServiceContractAttribute)typeof(T).GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#endif
            var serviceType = context.ServerBase.GetRegisteredCallbacksTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();
            foreach (var item in (from x in context.ServerBase.Clients.ToArray() where !sessionIds.Contains(x.SessionId) select x))
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
        /// <param name="sessionIds">list of sessions</param>
        /// <returns>list of callback context</returns>
        public static List<T> GetQueryClientCallbackList<T>(this OperationContext context, IEnumerable<string> sessionIds)
        {
            return (from x in GetQueryClientCallbackListOfClientContext<T>(context, sessionIds) select x.Service).ToList();
        }

        /// <summary>
        /// get clients callback context list and ignore custom session list
        /// </summary>
        /// <typeparam name="T">callback type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="sessionIds">list of sessions to ingore get</param>
        /// <returns>list of callback context</returns>
        public static List<T> GetQueryClientCallbackWithoutList<T>(this OperationContext context, IEnumerable<string> sessionIds)
        {
            return (from x in GetQueryClientCallbackWithoutListOfClientContext<T>(context, sessionIds) select x.Service).ToList();
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
                            throw new Exception($"context client not exist! {client.SessionId} {serverBase.Callbacks.Count} {serverBase.Services.Count} {DateTime.Now}");
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
            foreach (var item in serverBase.Clients.ToArray())
            {
                if (item == client)
                    continue;
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
        /// <param name="sessionId">list of sessions</param>
        /// <returns>list of callback context</returns>
        public static ClientContext<T> GetClientCallbackOfClientContext<T>(this ServerBase serverBase, string sessionId)
        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var attribName = ((ServiceContractAttribute)typeof(T).GetTypeInfo().GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#else
            var attribName = ((ServiceContractAttribute)typeof(T).GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#endif
            var serviceType = serverBase.GetRegisteredCallbacksTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();
            var client = (from x in serverBase.Clients.ToArray() where sessionId == x.SessionId select x).FirstOrDefault();
            if (client != null)
            {
                var find = FindClientCallback<T>(serverBase, client, serviceType, attribName);
                return new ClientContext<T>(find, client);
            }
            return null;
        }


        /// <summary>
        /// get clients callback context by session list
        /// </summary>
        /// <typeparam name="T">callback type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="sessionIds">list of sessions</param>
        /// <returns>list of callback context</returns>
        public static T GetClientCallback<T>(this ServerBase serverBase, string sessionId)
        {
            var client = GetClientCallbackOfClientContext<T>(serverBase, sessionId);
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
        /// <param name="sessionIds">list of sessions</param>
        /// <returns>list of callback context</returns>
        public static List<ClientContext<T>> GetQueryClientCallbackListOfClientContext<T>(this ServerBase serverBase, IEnumerable<string> sessionIds)
        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var attribName = ((ServiceContractAttribute)typeof(T).GetTypeInfo().GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#else
            var attribName = ((ServiceContractAttribute)typeof(T).GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#endif
            var serviceType = serverBase.GetRegisteredCallbacksTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();
            foreach (var item in (from x in serverBase.Clients.ToArray() where sessionIds.Contains(x.SessionId) select x))
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
        /// <param name="sessionIds">list of sessions to ingore get</param>
        /// <returns>list of callback context</returns>
        public static List<ClientContext<T>> GetQueryClientCallbackWithoutListOfClientContext<T>(this ServerBase serverBase, IEnumerable<string> sessionIds)
        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var attribName = ((ServiceContractAttribute)typeof(T).GetTypeInfo().GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#else
            var attribName = ((ServiceContractAttribute)typeof(T).GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#endif
            var serviceType = serverBase.GetRegisteredCallbacksTypeByName(attribName);
            List<ClientContext<T>> items = new List<ClientContext<T>>();
            foreach (var item in (from x in serverBase.Clients.ToArray() where !sessionIds.Contains(x.SessionId) select x))
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
        /// <param name="sessionIds">list of sessions</param>
        /// <returns>list of callback context</returns>
        public static List<T> GetQueryClientCallbackList<T>(this ServerBase serverBase, IEnumerable<string> sessionIds)
        {
            return (from x in GetQueryClientCallbackListOfClientContext<T>(serverBase, sessionIds) select x.Service).ToList();
        }

        /// <summary>
        /// get clients callback context list and ignore custom session list
        /// </summary>
        /// <typeparam name="T">callback type</typeparam>
        /// <param name="context">client context</param>
        /// <param name="sessionIds">list of sessions to ingore get</param>
        /// <returns>list of callback context</returns>
        public static List<T> GetQueryClientCallbackWithoutList<T>(this ServerBase serverBase, IEnumerable<string> sessionIds)
        {
            return (from x in GetQueryClientCallbackWithoutListOfClientContext<T>(serverBase, sessionIds) select x.Service).ToList();
        }
    }
}
