﻿using SignalGo.Shared;
using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.Models;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SignalGo.Client.ClientManager
{
    /// <summary>
    /// connector extensions
    /// </summary>
    public static class ConnectorExtensions
    {
        static ConnectorExtensions()
        {
            CSCodeInjection.InvokedClientMethodAction = (client, method, parameters) =>
            {
                //Console.WriteLine($"CSCodeInjection.InvokedClientMethodAction {method.Name}");
                //if (!(client is OperationCalls))
                //{
                //    AutoLogger.LogText($"cannot cast! {method.Name} params {parameters?.Length}");
                //}
                SendDataInvoke((OperationCalls)client, method.Name, parameters);
            };

            CSCodeInjection.InvokedClientMethodFunction = (client, method, parameters) =>
            {
                //Console.WriteLine($"CSCodeInjection.InvokedClientMethodFunction {method.Name}");
                object data = SendData((OperationCalls)client, method.Name, "", parameters);
                if (data == null)
                    return null;
                return data is StreamInfo ? data : ClientSerializationHelper.DeserializeObject(data.ToString(), method.ReturnType);
            };
        }


        /// <summary>
        /// call method wait for complete response from clients
        /// </summary>
        internal static ConcurrentDictionary<string, TaskCompletionSource<MethodCallbackInfo>> WaitedMethodsForResponse { get; set; } = new ConcurrentDictionary<string, TaskCompletionSource<MethodCallbackInfo>>();
        /// <summary>
        /// send data to client
        /// </summary>
        /// <typeparam name="T">return type data</typeparam>
        /// <param name="client">client for send data</param>
        /// <param name="callerName">method name</param>
        /// <param name="args">argumants of method</param>
        /// <returns></returns>
        internal static T SendData<T>(this OperationCalls client, string callerName, params Shared.Models.ParameterInfo[] args)
        {
            object data = SendData(client, callerName, "", args);
            if (data == null || data.ToString() == "")
                return default(T);
            return ClientSerializationHelper.DeserializeObject<T>(data.ToString());
        }
        /// <summary>
        /// send data to connector
        /// </summary>
        /// <typeparam name="T">return type data</typeparam>
        /// <param name="connector">connetor for send data</param>
        /// <param name="callInfo">method for send</param>
        /// <returns></returns>
#if (NET40 || NET35)
        internal static T SendData<T>(this ConnectorBase connector, MethodCallInfo callInfo)
        {
            string data = SendData(connector, callInfo);
#else
        internal static async Task<T> SendData<T>(this ConnectorBase connector, MethodCallInfo callInfo)
        {
            string data = await SendDataAsync(connector, callInfo);
#endif

            if (string.IsNullOrEmpty(data))
                return default(T);
            return ClientSerializationHelper.DeserializeObject<T>(data.ToString());
        }
        /// <summary>
        /// send data none return value
        /// </summary>
        /// <param name="client"></param>
        /// <param name="callerName"></param>
        /// <param name="args"></param>
        internal static void SendDataInvoke(this OperationCalls client, string callerName, params Shared.Models.ParameterInfo[] args)
        {
            SendData(client, callerName, "", args);
        }

        /// <summary>
        /// send data not use params by array object
        /// </summary>
        /// <param name="client"></param>
        /// <param name="callerName"></param>
        /// <param name="attibName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static object SendDataNoParam(this OperationCalls client, string callerName, string attibName, Shared.Models.ParameterInfo[] args)
        {
            return SendData(client, callerName, attibName, args);
        }

        /// <summary>
        /// send data to server
        /// </summary>
        /// <param name="client">client is sended</param>
        /// <param name="callerName">methos name</param>
        /// <param name="attibName">service name</param>
        /// <param name="args">method parameters</param>
        /// <returns></returns>
        internal static object SendData(this OperationCalls client, string callerName, string attibName, params Shared.Models.ParameterInfo[] args)
        {
            string serviceName = "";
            if (string.IsNullOrEmpty(attibName))
                serviceName = client.GetType().GetServerServiceName(false);
            else
                serviceName = attibName;
#if (NET40 || NET35)
            return SendData(client.Connector, serviceName, callerName, args);
#else
            return SendDataAsync(client.Connector, serviceName, callerName, args);
#endif
        }

        /// <summary>
        /// send data to server
        /// </summary>
        /// <returns></returns>
#if (NET40 || NET35)
        public static string SendData(ConnectorBase connector, string serviceName, string methodName, params Shared.Models.ParameterInfo[] args)
#else
        public static Task<string> SendDataAsync(ConnectorBase connector, string serviceName, string methodName, params Shared.Models.ParameterInfo[] args)
#endif
        {
            MethodCallInfo callInfo = new MethodCallInfo();
            callInfo.ServiceName = serviceName;

            callInfo.MethodName = methodName;
            callInfo.Parameters = args;
            //foreach (var item in args)
            //{
            //    callInfo.Parameters.Add(new Shared.Models.ParameterInfo() { Value = ClientSerializationHelper.SerializeObject(item), Name = item?.GetType().FullName });
            //}
            string guid = Guid.NewGuid().ToString();
            callInfo.Guid = guid;
#if (NET40 || NET35)
            return SendData(connector, callInfo);
#else
            return SendDataAsync(connector, callInfo);
#endif
        }

        internal static Task<T> SendDataTaskAsync<T>(ConnectorBase connector, string serviceName, string methodName, MethodInfo method, params object[] args)
        {
            MethodCallInfo callInfo = new MethodCallInfo();
            callInfo.ServiceName = serviceName;
            callInfo.MethodName = methodName;
            //foreach (var item in args)
            //{
            //    callInfo.Parameters.Add(new Shared.Models.ParameterInfo() { Value = ClientSerializationHelper.SerializeObject(item), Name = item?.GetType().FullName });
            //}
            string guid = Guid.NewGuid().ToString();
            callInfo.Guid = guid;
            return SendDataAsync<T>(connector, method, callInfo, args);
        }

#if (NET40 || NET35)
        private static string SendData(this ConnectorBase connector, MethodCallInfo callInfo)
#else
        private static async Task<string> SendDataAsync(this ConnectorBase connector, MethodCallInfo callInfo)
#endif
        {
            //TryAgain:
            bool isIgnorePriority = false;
            try
            {
                TaskCompletionSource<MethodCallbackInfo> valueData = new TaskCompletionSource<MethodCallbackInfo>();
#if (NET40 || NET35)
                CancellationTokenSource ct = new CancellationTokenSource();
#else
                CancellationTokenSource ct = new CancellationTokenSource((int)connector.ProviderSetting.ServerServiceSetting.SendDataTimeout.TotalMilliseconds);
#endif
                ct.Token.Register(() => valueData.TrySetCanceled(), useSynchronizationContext: false);

                bool added = WaitedMethodsForResponse.TryAdd(callInfo.Guid, valueData);
                object service = connector.Services.ContainsKey(callInfo.ServiceName) ? connector.Services[callInfo.ServiceName] : null;
                //#if (PORTABLE)
                MethodInfo method = service?.GetType().FindMethod(callInfo.MethodName);
                //#else
                //                var method = service?.GetType().FindMethod(callInfo.MethodName, RuntimeTypeHelper.GetMethodTypes(service.GetType(), callInfo).ToArray());
                //#endif
                //
                isIgnorePriority = method?.GetCustomAttributes<PriorityCallAttribute>().Count() > 0;

                connector.SendData(callInfo);


#if (NET40 || NET35)
                var result = WaitedMethodsForResponse[callInfo.Guid].Task.Result;
#else
                var result = await WaitedMethodsForResponse[callInfo.Guid].Task;
#endif
                //if (!seted)
                //{
                //    if (connector.IsDisposed)
                //        throw new ObjectDisposedException("Provider");
                //    if (!connector.IsConnected)
                //        throw new Exception("client disconnected");
                //    if (connector.ProviderSetting.DisconnectClientWhenTimeout)
                //        connector.Disconnect();
                //    throw new TimeoutException();
                //}

                if (result == null)
                {
                    if (connector.IsDisposed)
                        throw new ObjectDisposedException("Provider");
                    if (!connector.IsConnected)
                        throw new Exception("client disconnected");
                    return null;
                }
                if (result.IsException)
                    throw new Exception("server exception:" + ClientSerializationHelper.DeserializeObject<string>(result.Data));
                else if (result.IsAccessDenied && result.Data == null)
                    throw new Exception("server permission denied exception.");

                return result.Data;

            }
            catch (Exception ex)
            {
#if (NET40 || NET35)
                if (connector.IsConnected && !connector.SendPingAndWaitToReceive())
#else
                if (connector.IsConnected && !await connector.SendPingAndWaitToReceive())
#endif
                {
                    connector.Disconnect();
                }
                //if (connector.ProviderSetting.AutoReconnect && connector.ProviderSetting.HoldMethodCallsWhenDisconnected && !connector.IsConnected && !isIgnorePriority)
                //{
                //    AutoResetEvent resetEvent = new AutoResetEvent(true);
                //    resetEvent.Reset();
                //    connector.HoldMethodsToReconnect.Add(resetEvent);
                //    if (connector.IsConnected)
                //    {
                //        connector.HoldMethodsToReconnect.Remove(resetEvent);
                //        goto TryAgain;
                //    }
                //    else
                //    {
                //        resetEvent.WaitOne();
                //        goto TryAgain;
                //    }
                //}
                throw ex;
            }
            finally
            {
                WaitedMethodsForResponse.Remove(callInfo.Guid);
            }
        }

        internal static Task<T> SendDataAsync<T>(this ConnectorBase connector, MethodInfo method, MethodCallInfo callInfo, object[] args)
        {
#if (NET40 || NET35)
            return Task<T>.Factory.StartNew(() =>
#else
            return Task.Run(async () =>
#endif
            {
                callInfo.Parameters = method.MethodToParameters(x => ClientSerializationHelper.SerializeObject(x), args).ToArray();
#if (NET40 || NET35)
                string result = SendData(connector, callInfo);
#else
                string result = await SendDataAsync(connector, callInfo);
#endif
                object deserialeResult = ClientSerializationHelper.DeserializeObject(result, typeof(T));
                return (T)deserialeResult;
            });
        }

        //public static string SendRequest(this ConnectorBase connector, string serviceName, ServiceDetailsMethod serviceDetailMethod, ServiceDetailsRequestInfo requestInfo, out string json)
        //{
        //    MethodCallInfo callInfo = new MethodCallInfo()
        //    {
        //        ServiceName = serviceName,
        //        MethodName = serviceDetailMethod.MethodName
        //    };
        //    callInfo.Parameters = requestInfo.Parameters.Select(x => new Shared.Models.ParameterInfo() { Value = x.Value.ToString(), Name = x.Name }).ToArray();


        //    string guid = Guid.NewGuid().ToString();
        //    callInfo.Guid = guid;
        //    bool added = WaitedMethodsForResponse.TryAdd(callInfo.Guid, new KeyValue<AutoResetEvent, MethodCallbackInfo>(new AutoResetEvent(false), null));
        //    //var service = connector.Services.ContainsKey(callInfo.ServiceName) ? connector.Services[callInfo.ServiceName] : null;
        //    //var method = service == null ? null : service.GetType().GetMethod(callInfo.MethodName, RuntimeTypeHelper.GetMethodTypes(service.GetType(), callInfo).ToArray());
        //    json = ClientSerializationHelper.SerializeObject(callInfo);
        //    connector.SendData(callInfo);


        //    bool seted = WaitedMethodsForResponse[callInfo.Guid].Key.WaitOne(connector.ProviderSetting.ServerServiceSetting.SendDataTimeout);
        //    if (!seted)
        //    {
        //        if (connector.ProviderSetting.DisconnectClientWhenTimeout)
        //            connector.Disconnect();
        //        throw new TimeoutException();
        //    }
        //    MethodCallbackInfo result = WaitedMethodsForResponse[callInfo.Guid].Value;
        //    if (callInfo.MethodName == "/RegisterService")
        //    {
        //        connector.ClientId = ClientSerializationHelper.DeserializeObject<string>(result.Data);
        //        result.Data = null;
        //    }
        //    WaitedMethodsForResponse.Remove(callInfo.Guid);
        //    if (result == null)
        //    {
        //        if (connector.IsDisposed)
        //            throw new Exception("client disconnected");
        //        return "disposed";
        //    }
        //    if (result.IsException)
        //        throw new Exception("server exception:" + ClientSerializationHelper.DeserializeObject<string>(result.Data));
        //    else if (result.IsAccessDenied && result.Data == null)
        //        throw new Exception("server permission denied exception.");

        //    return result.Data;
        //}
    }

}