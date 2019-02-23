using SignalGo.Server.Helpers;
using SignalGo.Server.Models;
using SignalGo.Server.ServiceManager.Providers;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Server.ServiceManager
{
    public static class ServerExtensions
    {
        public static Task<Stream> GetTcpStream(this TcpClient tcpClient, ServerBase serverBase)
        {
            if (serverBase.ProviderSetting.HttpSetting.IsHttps)
            {
                return SslTcpManager.GetStream(tcpClient, serverBase.ProviderSetting.HttpSetting.X509Certificate);
            }
            else
            {
                return Task.Run(() => (Stream)tcpClient.GetStream());
            }
        }
        //static ServerExtension()
        //{
        //    Init();
        //}

        //internal static void Init()
        //{
        //    CSCodeInjection.InvokedServerMethodAction = (client, method, parameters) =>
        //    {
        //        //Console.WriteLine($"CSCodeInjection.InvokedServerMethodAction {method.Name}");
        //        OperationCalls op = (OperationCalls)client;
        //        SendToClientDataInvoke(op, method.Name, parameters);
        //        //MethodsCallHandler.EndClientMethodCallAction?.Invoke(op.CurrentClient, null, method.Name, parameters, null, null);
        //    };

        //    CSCodeInjection.InvokedServerMethodFunction = (client, method, parameters) =>
        //    {
        //        //Console.WriteLine($"CSCodeInjection.InvokedServerMethodFunction {method.Name}");
        //        OperationCalls op = (OperationCalls)client;
        //        var data = SendToClientData((OperationCalls)client, method.Name, parameters);
        //        var result = ServerSerializationHelper.Deserialize(data.ToString(), method.ReturnType, ((OperationCalls)client).ServerBase);
        //        //MethodsCallHandler.EndClientMethodCallAction?.Invoke(op.CurrentClient, null, method.Name, parameters, result, null);
        //        return result;
        //    };
        //}

        //public static void RunOnDispatcher(this ClientInfo client, Action run)
        //{
        //    client.ServerBase.ClientDispatchers[client].Post((state) =>
        //    {
        //        run();
        //    }, null);
        //}

        //public static T SendData<T>(this OperationCalls client, string callerName, params object[] args)
        //{
        //    var data = SendToClientData(client, callerName, args);
        //    if (data == null || data.ToString() == "")
        //        return default(T);
        //    return ServerSerializationHelper.Deserialize<T>(data.ToString(), client.ServerBase);
        //}

        //public static void SendToClientDataInvoke(this OperationCalls client, string callerName, params object[] args)
        //{
        //    SendToClientData(client, callerName, args);
        //}

        //static object SendToClientData(this OperationCalls client, string callerName, params object[] args)
        //{
        //    var attribute = client.GetType().GetClientServiceAttribute();
        //    if (client.ServerBase.ClientRegistredMethods[client.CurrentClient].ContainsKey(attribute.Name))
        //    {
        //        if (client.ServerBase.ClientRegistredMethods[client.CurrentClient][attribute.Name].Contains(callerName))
        //        {
        //            return SendCallClientMethod(client, callerName, args);
        //        }
        //    }
        //    else if (client.ServerBase.RegisteredClientServicesTypes.ContainsKey(attribute.Name))
        //    {
        //        return SendCallClientMethod(client, callerName, args);
        //    }
        //    return null;
        //}

        //internal static object SendDataWithCallClientServiceMethod(ServerBase serverBase,ClientInfo client,string serviceName, string methodName, params Shared.Models.GoParameterInfo[] args)
        //{
        //    MethodCallInfo callInfo = new MethodCallInfo();
        //    callInfo.ServiceName = serviceName;
        //    callInfo.MethodName = methodName;
        //    callInfo.Parameters = args;
        //    var guid = Guid.NewGuid().ToString();
        //    callInfo.Guid = guid;
        //    var bytes = Encoding.UTF8.GetBytes(ServerSerializationHelper.SerializeObject(callInfo, serverBase));
        //    client.StreamHelper.WriteBlockToStream(client.ClientStream, bytes);
        //}

        internal static async Task<T> SendDataWithCallClientServiceMethod<T>(ServerBase serverBase, ClientInfo client, Type returnType, string serviceName, string methodName, params Shared.Models.ParameterInfo[] args)
        {
            if (returnType == null || returnType == typeof(void))
                returnType = typeof(object);
            //var method = typeof(ServerExtensions).GetMethod("SendDataWithCallClientServiceMethodGeneric").MakeGenericMethod(returnType);
            //Func<object> run = () =>
            //{
            //    return SendDataWithCallClientServiceMethodGeneric<int>(serverBase, client, returnType, serviceName, methodName, args);
            //};
            if (methodName.LastIndexOf("Async", StringComparison.OrdinalIgnoreCase) == methodName.Length - 5)
                methodName = methodName.Substring(0, methodName.Length - 5);
#if (NET35 || NET40)
            return null;// Task<object>.Factory.StartNew(run);
#else
            Type type = typeof(TaskCompletionSource<>).MakeGenericType(returnType);
            object taskCompletionSource = Activator.CreateInstance(type);
            MethodCallInfo callInfo = new MethodCallInfo();
            callInfo.ServiceName = serviceName;
            callInfo.MethodName = methodName;
            callInfo.Parameters = args;
            string guid = Guid.NewGuid().ToString();
            callInfo.Guid = guid;
            serverBase.ClientServiceCallMethodsResult.TryAdd(guid, new KeyValue<Type, object>(returnType, taskCompletionSource));
            List<byte> bytes = new List<byte>
                {
                     (byte)DataType.CallMethod,
                     (byte)CompressMode.None
                };
            byte[] jsonBytes = Encoding.UTF8.GetBytes(ServerSerializationHelper.SerializeObject(callInfo, serverBase));
            byte[] dataLen = BitConverter.GetBytes(jsonBytes.Length);
            bytes.AddRange(dataLen);
            bytes.AddRange(jsonBytes);

            await client.StreamHelper.WriteToStreamAsync(client.ClientStream, bytes.ToArray());
            Task<T> result = (Task<T>)taskCompletionSource.GetType().GetProperty("Task").GetValue(taskCompletionSource, null);
            return await result;
#endif
        }

        internal static async Task<T> SendWebSocketDataWithCallClientServiceMethod<T>(ServerBase serverBase, ClientInfo client, Type returnType, string serviceName, string methodName, params Shared.Models.ParameterInfo[] args)
        {
            if (returnType == null || returnType == typeof(void))
                returnType = typeof(object);
            else if (returnType.GetBaseType() == typeof(Task))
            {
                returnType = returnType.GetGenericArguments()[0];
            }

            if (methodName.LastIndexOf("Async", StringComparison.OrdinalIgnoreCase) == methodName.Length - 5)
                methodName = methodName.Substring(0, methodName.Length - 5);
#if (NET35 || NET40)
            return null;// Task<object>.Factory.StartNew(run);
#else
            Type type = typeof(TaskCompletionSource<>).MakeGenericType(returnType);
            object taskCompletionSource = Activator.CreateInstance(type);
            MethodCallInfo callInfo = new MethodCallInfo();
            callInfo.ServiceName = serviceName;
            callInfo.MethodName = methodName;
            callInfo.Parameters = args;
            string guid = Guid.NewGuid().ToString();
            callInfo.Guid = guid;
            serverBase.ClientServiceCallMethodsResult.TryAdd(guid, new KeyValue<Type, object>(returnType, taskCompletionSource));

            string json = ServerSerializationHelper.SerializeObject(callInfo, serverBase);
            ///when length is large we need to send data by parts
            if (json.Length > 30000)
            {
                List<string> listOfParts = WebSocketProvider.GeneratePartsOfData(json);
                int i = 1;
                foreach (string item in listOfParts)
                {
                    MethodCallInfo cb = callInfo.Clone();
                    cb.PartNumber = i == listOfParts.Count ? (short)-1 : (short)i;
                    json = (int)DataType.CallMethod + "," + (int)CompressMode.None + "/" + ServerSerializationHelper.SerializeObject(cb, serverBase);
                    byte[] bytes = Encoding.UTF8.GetBytes(json);
                    await client.StreamHelper.WriteToStreamAsync(client.ClientStream, bytes);
                    i++;
                }
            }
            else
            {
                json = (int)DataType.CallMethod + "," + (int)CompressMode.None + "/" + json;
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                await client.StreamHelper.WriteToStreamAsync(client.ClientStream, bytes);
            }
            object value = taskCompletionSource.GetType().GetProperty("Task").GetValue(taskCompletionSource, null);
            T result = await (Task<T>)value;
            return result;
#endif
        }
        //static object SendCallClientMethod(this OperationCalls client, string callerName, params object[] args)
        //{
        //    if (SynchronizationContext.Current != null && ServerBase.AllDispatchers.ContainsKey(SynchronizationContext.Current) && ServerBase.AllDispatchers[SynchronizationContext.Current].FirstOrDefault().MainContext == SynchronizationContext.Current)
        //        throw new Exception("Cannot call method from class Constractor or main Thread");
        //    var attribute = client.GetType().GetClientServiceAttribute();


        //    var waitedMethodsForResponse = client.ServerBase.WaitedMethodsForResponse[client.CurrentClient];
        //    waitedMethodsForResponse.TryAdd(guid, new KeyValue<AutoResetEvent, MethodCallbackInfo>(new AutoResetEvent(false), null));
        //    client.ServerBase.CallClientMethod(client.CurrentClient, callInfo);
        //    var seted = waitedMethodsForResponse[guid].Key.WaitOne(client.ServerBase.ProviderSetting.SendDataTimeout);
        //    if (!seted)
        //    {
        //        client.ServerBase.CheckClient(client.CurrentClient);
        //        waitedMethodsForResponse.Remove(guid);
        //        var ex = new TimeoutException();
        //        MethodsCallHandler.EndClientMethodCallAction?.Invoke(client.CurrentClient, callInfo.Guid, callInfo.ServiceName, callerName, args, null, ex);
        //        throw ex;
        //    }
        //    if (waitedMethodsForResponse[guid].Value.IsException)
        //    {
        //        var data = waitedMethodsForResponse[guid].Value.Data;
        //        waitedMethodsForResponse.Remove(guid);
        //        var ex = new Exception("method call exception: " + data);
        //        MethodsCallHandler.EndClientMethodCallAction?.Invoke(client.CurrentClient, callInfo.Guid, callInfo.ServiceName, callerName, args, null, ex);
        //        throw ex;
        //    }
        //    var result = waitedMethodsForResponse[guid].Value.Data;
        //    waitedMethodsForResponse.Remove(guid);
        //    MethodsCallHandler.EndClientMethodCallAction?.Invoke(client.CurrentClient, callInfo.Guid, callInfo.ServiceName, callerName, args, result, null);

        //    return result;
        //}

        //        public static T CallClientCallbackMethod<T>(this OperationCalls client, string methodName, params object[] values)
        //        {

        //#if (NETSTANDARD1_6 || NETCOREAPP1_1)
        //                    string serviceName = ((ServiceContractAttribute)client.GetType().GetTypeInfo().GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
        //#else
        //            string serviceName = ((ServiceContractAttribute)client.GetType().GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name;
        //#endif
        //            var data = CallClientCallbackMethod(serviceName, methodName, values);
        //            if (data == null || data.ToString() == "")
        //                return default(T);
        //            var result = ServerSerializationHelper.Deserialize<T>(data.ToString(), client.ServerBase);
        //            MethodsCallHandler.EndClientMethodCallAction?.Invoke(client, serviceName, methodName, values, result, null);

        //            return result;
        //        }

        //        public static object CallClientCallbackMethod(string serviceName, string methodName, params object[] values)
        //        {
        //            if (SynchronizationContext.Current != null && ServerBase.AllDispatchers.ContainsKey(SynchronizationContext.Current))
        //                throw new Exception("Cannot call method from class Constractor or main Thread");
        //            MethodCallInfo callInfo = new MethodCallInfo();
        //#if (NETSTANDARD1_6 || NETCOREAPP1_1)
        //                    callInfo.ServiceName = serviceName;
        //#else
        //            callInfo.ServiceName = serviceName;
        //#endif
        //            callInfo.MethodName = methodName;
        //            foreach (var item in values)
        //            {
        //                callInfo.Parameters.Add(new Shared.Models.GoParameterInfo() { Value = ServerSerializationHelper.SerializeObject(item, client.ServerBase), Type = item.GetType().FullName });
        //            }
        //            var guid = Guid.NewGuid().ToString();
        //            callInfo.Guid = guid;
        //            var waitedMethodsForResponse = client.ServerBase.WaitedMethodsForResponse[client.CurrentClient];
        //            waitedMethodsForResponse.TryAdd(guid, new KeyValue<AutoResetEvent, MethodCallbackInfo>(new AutoResetEvent(false), null));
        //            client.ServerBase.CallClientMethod(client.CurrentClient, callInfo);
        //            var seted = waitedMethodsForResponse[guid].Key.WaitOne(client.ServerBase.ProviderSetting.ReceiveDataTimeout);
        //            if (!seted)
        //            {
        //                client.ServerBase.CheckClient(client.CurrentClient);
        //                waitedMethodsForResponse.Remove(guid);
        //                throw new TimeoutException();
        //            }
        //            if (waitedMethodsForResponse[guid].Value.IsException)
        //            {
        //                var data = waitedMethodsForResponse[guid].Value.Data;
        //                waitedMethodsForResponse.Remove(guid);
        //                throw new Exception("call method return exception: " + data);
        //            }
        //            var result = waitedMethodsForResponse[guid].Value.Data;
        //            waitedMethodsForResponse.Remove(guid);
        //            return result;
        //        }

        //internal static object SendCustomCallbackToClient(ClientInfo client, MethodCallInfo callInfo)
        //{
        //    var guid = Guid.NewGuid().ToString();
        //    callInfo.Guid = guid;
        //    var waitedMethodsForResponse = client.ServerBase.WaitedMethodsForResponse[client];
        //    waitedMethodsForResponse.TryAdd(guid, new KeyValue<AutoResetEvent, MethodCallbackInfo>(new AutoResetEvent(false), null));
        //    client.ServerBase.CallClientMethod(client, callInfo);
        //    var seted = waitedMethodsForResponse[guid].Key.WaitOne(client.ServerBase.ProviderSetting.ReceiveDataTimeout);
        //    if (!seted)
        //    {
        //        client.ServerBase.CheckClient(client);
        //        waitedMethodsForResponse.Remove(guid);
        //        var ex = new TimeoutException();
        //        MethodsCallHandler.EndClientMethodCallAction?.Invoke(client,callInfo.Guid, callInfo.ServiceName, callInfo.MethodName, callInfo.Parameters.Cast<object>().ToArray(), null, ex);
        //        throw ex;
        //    }
        //    if (waitedMethodsForResponse[guid].Value.IsException)
        //    {
        //        var data = waitedMethodsForResponse[guid].Value.Data;
        //        waitedMethodsForResponse.Remove(guid);
        //        var ex = new Exception("call method return exception: " + data);
        //        MethodsCallHandler.EndClientMethodCallAction?.Invoke(client, callInfo.Guid, callInfo.ServiceName, callInfo.MethodName, callInfo.Parameters.Cast<object>().ToArray(), null, ex);
        //        throw ex;
        //    }
        //    var result = waitedMethodsForResponse[guid].Value.Data;
        //    waitedMethodsForResponse.Remove(guid);
        //    return result;
        //}

    }

}
