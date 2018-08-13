using SignalGo.Shared;
using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                var data = SendData((OperationCalls)client, method.Name, "", parameters);
                if (data == null)
                    return null;
                return data is StreamInfo ? data : ClientSerializationHelper.DeserializeObject(data.ToString(), method.ReturnType);
            };
        }

        /// <summary>
        /// call method wait for complete response from clients
        /// </summary>
        internal static ConcurrentDictionary<string, KeyValue<AutoResetEvent, MethodCallbackInfo>> WaitedMethodsForResponse { get; set; } = new ConcurrentDictionary<string, KeyValue<AutoResetEvent, MethodCallbackInfo>>();
        /// <summary>
        /// send data to client
        /// </summary>
        /// <typeparam name="T">return type data</typeparam>
        /// <param name="client">client for send data</param>
        /// <param name="callerName">method name</param>
        /// <param name="args">argumants of method</param>
        /// <returns></returns>
        internal static T SendData<T>(this OperationCalls client, string callerName, params object[] args)
        {
            var data = SendData(client, callerName, "", args);
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
        internal static T SendData<T>(this ConnectorBase connector, MethodCallInfo callInfo)
        {
            var data = SendData(connector, callInfo);
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
        internal static void SendDataInvoke(this OperationCalls client, string callerName, params object[] args)
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
        internal static object SendDataNoParam(this OperationCalls client, string callerName, string attibName, object[] args)
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
        internal static object SendData(this OperationCalls client, string callerName, string attibName, params object[] args)
        {
            string serviceName = "";
            if (string.IsNullOrEmpty(attibName))
                serviceName = client.GetType().GetServerServiceName();
            else
                serviceName = attibName;

            return SendData(client.Connector, serviceName, callerName, args);
        }

        /// <summary>
        /// send data to server
        /// </summary>
        /// <returns></returns>
        internal static string SendData(ConnectorBase connector, string serviceName, string methodName, params object[] args)
        {
            MethodCallInfo callInfo = new MethodCallInfo();
            callInfo.ServiceName = serviceName;

            callInfo.MethodName = methodName;
            foreach (var item in args)
            {
                callInfo.Parameters.Add(new Shared.Models.ParameterInfo() { Value = ClientSerializationHelper.SerializeObject(item), Type = item?.GetType().FullName });
            }
            var guid = Guid.NewGuid().ToString();
            callInfo.Guid = guid;
            return SendData(connector, callInfo);
        }

        internal static Task<T> SendDataTask<T>(ConnectorBase connector, string serviceName, string methodName, params object[] args)
        {
            MethodCallInfo callInfo = new MethodCallInfo();
            callInfo.ServiceName = serviceName;

            callInfo.MethodName = methodName;
            foreach (var item in args)
            {
                callInfo.Parameters.Add(new Shared.Models.ParameterInfo() { Value = ClientSerializationHelper.SerializeObject(item), Type = item?.GetType().FullName });
            }
            var guid = Guid.NewGuid().ToString();
            callInfo.Guid = guid;
            return SendDataAsync<T>(connector, callInfo);
        }

        static string SendData(this ConnectorBase connector, MethodCallInfo callInfo)
        {
            //TryAgain:
            bool isIgnorePriority = false;
            try
            {
                var valueData = new KeyValue<AutoResetEvent, MethodCallbackInfo>(new AutoResetEvent(false), null);
                var added = WaitedMethodsForResponse.TryAdd(callInfo.Guid, valueData);
                var service = connector.Services.ContainsKey(callInfo.ServiceName) ? connector.Services[callInfo.ServiceName] : null;
#if (PORTABLE)
                var method = service?.GetType().FindMethod(callInfo.MethodName);
#else
                var method = service?.GetType().FindMethod(callInfo.MethodName, RuntimeTypeHelper.GetMethodTypes(service.GetType(), callInfo).ToArray());
#endif
                isIgnorePriority = method?.GetCustomAttributes<PriorityCallAttribute>().Count() > 0;

                connector.SendData(callInfo);


                var seted = WaitedMethodsForResponse[callInfo.Guid].Key.WaitOne(connector.ProviderSetting.ServerServiceSetting.SendDataTimeout);
                if (!seted)
                {
                    if (connector.IsDisposed)
                        throw new ObjectDisposedException("Provider");
                    if (!connector.IsConnected)
                        throw new Exception("client disconnected");
                    if (connector.ProviderSetting.DisconnectClientWhenTimeout)
                        connector.Disconnect();
                    throw new TimeoutException();
                }

                var result = valueData.Value;
                if (result != null && !result.IsException && callInfo.MethodName == "/RegisterService")
                {
                    connector.ClientId = ClientSerializationHelper.DeserializeObject<string>(result.Data);
                    result.Data = null;
                }
                WaitedMethodsForResponse.Remove(callInfo.Guid);
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
                if (connector.IsConnected && !connector.SendPingAndWaitToReceive())
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

        }

        static Task<T> SendDataAsync<T>(this ConnectorBase connector, MethodCallInfo callInfo)
        {
            return Task<T>.Factory.StartNew(() =>
            {
                var result = SendData(connector, callInfo);
                var deserialeResult = ClientSerializationHelper.DeserializeObject(result, typeof(T));
                return (T)deserialeResult;
            });
        }

        public static string SendRequest(this ConnectorBase connector, string serviceName, ServiceDetailsMethod serviceDetailMethod, ServiceDetailsRequestInfo requestInfo, out string json)
        {
            MethodCallInfo callInfo = new MethodCallInfo()
            {
                ServiceName = serviceName,
                MethodName = serviceDetailMethod.MethodName
            };
            foreach (var item in requestInfo.Parameters)
            {
                callInfo.Parameters.Add(new Shared.Models.ParameterInfo() { Value = item.Value.ToString(), Type = item.FullTypeName });
            }

            var guid = Guid.NewGuid().ToString();
            callInfo.Guid = guid;
            var added = WaitedMethodsForResponse.TryAdd(callInfo.Guid, new KeyValue<AutoResetEvent, MethodCallbackInfo>(new AutoResetEvent(false), null));
            //var service = connector.Services.ContainsKey(callInfo.ServiceName) ? connector.Services[callInfo.ServiceName] : null;
            //var method = service == null ? null : service.GetType().GetMethod(callInfo.MethodName, RuntimeTypeHelper.GetMethodTypes(service.GetType(), callInfo).ToArray());
            json = ClientSerializationHelper.SerializeObject(callInfo);
            connector.SendData(callInfo);


            var seted = WaitedMethodsForResponse[callInfo.Guid].Key.WaitOne(connector.ProviderSetting.ServerServiceSetting.SendDataTimeout);
            if (!seted)
            {
                if (connector.ProviderSetting.DisconnectClientWhenTimeout)
                    connector.Disconnect();
                throw new TimeoutException();
            }
            var result = WaitedMethodsForResponse[callInfo.Guid].Value;
            if (callInfo.MethodName == "/RegisterService")
            {
                connector.ClientId = ClientSerializationHelper.DeserializeObject<string>(result.Data);
                result.Data = null;
            }
            WaitedMethodsForResponse.Remove(callInfo.Guid);
            if (result == null)
            {
                if (connector.IsDisposed)
                    throw new Exception("client disconnected");
                return "disposed";
            }
            if (result.IsException)
                throw new Exception("server exception:" + ClientSerializationHelper.DeserializeObject<string>(result.Data));
            else if (result.IsAccessDenied && result.Data == null)
                throw new Exception("server permission denied exception.");

            return result.Data;
        }
    }

}
