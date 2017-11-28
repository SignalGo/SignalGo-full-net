using Newtonsoft.Json;
using SignalGo.Server.Helpers;
using SignalGo.Server.Models;
using SignalGo.Shared;
using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace SignalGo.Server.ServiceManager
{
    public static class ServerExtension
    {
        static ServerExtension()
        {
            Init();
        }

        internal static void Init()
        {
            CSCodeInjection.InvokedServerMethodAction = (client, method, parameters) =>
            {
                SendDataInvoke((OperationCalls)client, method.Name, parameters);
            };

            CSCodeInjection.InvokedServerMethodFunction = (client, method, parameters) =>
            {
                var data = SendData((OperationCalls)client, method.Name, parameters);
                return ServerSerializationHelper.Deserialize(data.ToString(), method.ReturnType, ((OperationCalls)client).ServerBase);
            };
        }

        //public static void RunOnDispatcher(this ClientInfo client, Action run)
        //{
        //    client.ServerBase.ClientDispatchers[client].Post((state) =>
        //    {
        //        run();
        //    }, null);
        //}

        public static T SendData<T>(this OperationCalls client, string callerName, params object[] args)
        {
            var data = SendData(client, callerName, args);
            if (data == null || data.ToString() == "")
                return default(T);
            return ServerSerializationHelper.Deserialize<T>(data.ToString(), client.ServerBase);
        }

        public static void SendDataInvoke(this OperationCalls client, string callerName, params object[] args)
        {
            SendData(client, callerName, args);
        }

        static object SendData(this OperationCalls client, string callerName, params object[] args)
        {
            var attribute = client.GetType().GetCustomAttributes<ServiceContractAttribute>(true).FirstOrDefault();
            if (client.ServerBase.ClientRegistredMethods[client.CurrentClient].ContainsKey(attribute.Name))
            {
                if (client.ServerBase.ClientRegistredMethods[client.CurrentClient][attribute.Name].Contains(callerName))
                {
                    return SendDataNow(client, callerName, args);
                }
            }
            else if (client.ServerBase.RegisteredCallbacksTypes.ContainsKey(attribute.Name))
            {
                return SendDataNow(client, callerName, args);
            }
            return null;
        }

        static object SendDataNow(this OperationCalls client, string callerName, params object[] args)
        {
            if (SynchronizationContext.Current != null && ServerBase.AllDispatchers.ContainsKey(SynchronizationContext.Current) && ServerBase.AllDispatchers[SynchronizationContext.Current].FirstOrDefault().MainContext == SynchronizationContext.Current)
                throw new Exception("Cannot call method from class Constractor or main Thread");
            var attribute = client.GetType().GetCustomAttributes<ServiceContractAttribute>(true).FirstOrDefault();
            MethodCallInfo callInfo = new MethodCallInfo();
            callInfo.ServiceName = attribute.Name;
            callInfo.MethodName = callerName;
            foreach (var item in args)
            {
                callInfo.Parameters.Add(new Shared.Models.ParameterInfo() { Value = item == null ? null : ServerSerializationHelper.SerializeObject(item, client.ServerBase), Type = item == null ? null : item.GetType().FullName });
            }
            var guid = Guid.NewGuid().ToString();
            callInfo.Guid = guid;
            var waitedMethodsForResponse = client.ServerBase.WaitedMethodsForResponse[client.CurrentClient];
            waitedMethodsForResponse.TryAdd(guid, new KeyValue<AutoResetEvent, MethodCallbackInfo>(new AutoResetEvent(false), null));
            client.ServerBase.CallClientMethod(client.CurrentClient, callInfo);
            var seted = waitedMethodsForResponse[guid].Key.WaitOne(client.ServerBase.ProviderSetting.SendDataTimeout);
            if (!seted)
            {
                client.ServerBase.CheckClient(client.CurrentClient);
                waitedMethodsForResponse.Remove(guid);
                throw new TimeoutException();
            }
            if (waitedMethodsForResponse[guid].Value.IsException)
            {
                var data = waitedMethodsForResponse[guid].Value.Data;
                waitedMethodsForResponse.Remove(guid);
                throw new Exception("method call exception: " + data);
            }
            var result = waitedMethodsForResponse[guid].Value.Data;
            waitedMethodsForResponse.Remove(guid);
            return result;
        }

        public static T CallClientCallbackMethod<T>(this OperationCalls client, string methodName, params object[] values)
        {
            var data = CallClientCallbackMethod(client, methodName, values);
            if (data == null || data.ToString() == "")
                return default(T);
            return ServerSerializationHelper.Deserialize<T>(data.ToString(), client.ServerBase);
        }

        public static object CallClientCallbackMethod(this OperationCalls client, string methodName, params object[] values)
        {
            if (SynchronizationContext.Current != null && ServerBase.AllDispatchers.ContainsKey(SynchronizationContext.Current))
                throw new Exception("Cannot call method from class Constractor or main Thread");
            MethodCallInfo callInfo = new MethodCallInfo();
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            callInfo.ServiceName = ((ServiceContractAttribute)client.GetType().GetTypeInfo().GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#else
            callInfo.ServiceName = ((ServiceContractAttribute)client.GetType().GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
#endif
            callInfo.MethodName = methodName;
            foreach (var item in values)
            {
                callInfo.Parameters.Add(new Shared.Models.ParameterInfo() { Value = ServerSerializationHelper.SerializeObject(item, client.ServerBase), Type = item.GetType().FullName });
            }
            var guid = Guid.NewGuid().ToString();
            callInfo.Guid = guid;
            var waitedMethodsForResponse = client.ServerBase.WaitedMethodsForResponse[client.CurrentClient];
            waitedMethodsForResponse.TryAdd(guid, new KeyValue<AutoResetEvent, MethodCallbackInfo>(new AutoResetEvent(false), null));
            client.ServerBase.CallClientMethod(client.CurrentClient, callInfo);
            var seted = waitedMethodsForResponse[guid].Key.WaitOne(client.ServerBase.ProviderSetting.ReceiveDataTimeout);
            if (!seted)
            {
                client.ServerBase.CheckClient(client.CurrentClient);
                waitedMethodsForResponse.Remove(guid);
                throw new TimeoutException();
            }
            if (waitedMethodsForResponse[guid].Value.IsException)
            {
                var data = waitedMethodsForResponse[guid].Value.Data;
                waitedMethodsForResponse.Remove(guid);
                throw new Exception("call method return exception: " + data);
            }
            var result = waitedMethodsForResponse[guid].Value.Data;
            waitedMethodsForResponse.Remove(guid);
            return result;
        }

        internal static object SendCustomCallbackToClient(ClientInfo client, MethodCallInfo callInfo)
        {
            var guid = Guid.NewGuid().ToString();
            callInfo.Guid = guid;
            var waitedMethodsForResponse = client.ServerBase.WaitedMethodsForResponse[client];
            waitedMethodsForResponse.TryAdd(guid, new KeyValue<AutoResetEvent, MethodCallbackInfo>(new AutoResetEvent(false), null));
            client.ServerBase.CallClientMethod(client, callInfo);
            var seted = waitedMethodsForResponse[guid].Key.WaitOne(client.ServerBase.ProviderSetting.ReceiveDataTimeout);
            if (!seted)
            {
                client.ServerBase.CheckClient(client);
                waitedMethodsForResponse.Remove(guid);
                throw new TimeoutException();
            }
            if (waitedMethodsForResponse[guid].Value.IsException)
            {
                var data = waitedMethodsForResponse[guid].Value.Data;
                waitedMethodsForResponse.Remove(guid);
                throw new Exception("call method return exception: " + data);
            }
            var result = waitedMethodsForResponse[guid].Value.Data;
            waitedMethodsForResponse.Remove(guid);
            return result;
        }

    }

}
