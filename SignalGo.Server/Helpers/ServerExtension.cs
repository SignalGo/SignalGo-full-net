using SignalGo.Server.Models;
using SignalGo.Server.ServiceManager;
using SignalGo.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Server.Helpers
{
    /// <summary>
    /// extensions that server needed
    /// </summary>
    public static class ServerExtensions
    {
        public static Stream GetTcpStream(this TcpClient tcpClient, ServerBase serverBase)
        {
            //if (serverBase.ProviderSetting.HttpSetting.IsHttps)
            //{
            //    return SslTcpManager.GetStream(tcpClient, serverBase.ProviderSetting.HttpSetting.X509Certificate);
            //}
            //else
            //{
                return (Stream)tcpClient.GetStream();
            //}
        }

//        internal static async Task<T> SendDataWithCallClientServiceMethod<T>(ServerBase serverBase, ClientInfo client, Type returnType, string serviceName, string methodName, params Shared.Models.ParameterInfo[] args)
//        {
//            if (returnType == null || returnType == typeof(void))
//                returnType = typeof(object);
//            if (methodName.LastIndexOf("Async", StringComparison.OrdinalIgnoreCase) == methodName.Length - 5)
//                methodName = methodName.Substring(0, methodName.Length - 5);
//#if (NET35 || NET40)
//            return null;// Task<object>.Factory.StartNew(run);
//#else
//            Type type = typeof(TaskCompletionSource<>).MakeGenericType(returnType);
//            object taskCompletionSource = Activator.CreateInstance(type);
//            MethodCallInfo callInfo = new MethodCallInfo();
//            callInfo.ServiceName = serviceName;
//            callInfo.MethodName = methodName;
//            callInfo.Parameters = args;
//            string guid = Guid.NewGuid().ToString();
//            callInfo.Guid = guid;
//            serverBase.ClientServiceCallMethodsResult.TryAdd(guid, new KeyValue<Type, object>(returnType, taskCompletionSource));
//            List<byte> bytes = new List<byte>
//                {
//                     (byte)DataType.CallMethod,
//                     (byte)CompressMode.None
//                };
//            byte[] jsonBytes = Encoding.UTF8.GetBytes(ServerSerializationHelper.SerializeObject(callInfo, serverBase));
//            byte[] dataLen = BitConverter.GetBytes(jsonBytes.Length);
//            bytes.AddRange(dataLen);
//            bytes.AddRange(jsonBytes);

//            await client.StreamHelper.WriteToStreamAsync(client.ClientStream, bytes.ToArray());
//            Task<T> result = (Task<T>)taskCompletionSource.GetType().GetProperty("Task").GetValue(taskCompletionSource, null);
//            return await result;
//#endif
//        }

//        internal static async Task<T> SendWebSocketDataWithCallClientServiceMethod<T>(ServerBase serverBase, ClientInfo client, Type returnType, string serviceName, string methodName, params Shared.Models.ParameterInfo[] args)
//        {
//            if (returnType == null || returnType == typeof(void))
//                returnType = typeof(object);
//            else if (returnType.GetBaseType() == typeof(Task))
//            {
//                returnType = returnType.GetGenericArguments()[0];
//            }

//            if (methodName.LastIndexOf("Async", StringComparison.OrdinalIgnoreCase) == methodName.Length - 5)
//                methodName = methodName.Substring(0, methodName.Length - 5);
//#if (NET35 || NET40)
//            return null;// Task<object>.Factory.StartNew(run);
//#else
//            Type type = typeof(TaskCompletionSource<>).MakeGenericType(returnType);
//            object taskCompletionSource = Activator.CreateInstance(type);
//            MethodCallInfo callInfo = new MethodCallInfo();
//            callInfo.ServiceName = serviceName;
//            callInfo.MethodName = methodName;
//            callInfo.Parameters = args;
//            string guid = Guid.NewGuid().ToString();
//            callInfo.Guid = guid;
//            serverBase.ClientServiceCallMethodsResult.TryAdd(guid, new KeyValue<Type, object>(returnType, taskCompletionSource));

//            string json = ServerSerializationHelper.SerializeObject(callInfo, serverBase);
//            ///when length is large we need to send data by parts
//            if (json.Length > 30000)
//            {
//                List<string> listOfParts = WebSocketProvider.GeneratePartsOfData(json);
//                int i = 1;
//                foreach (string item in listOfParts)
//                {
//                    MethodCallInfo cb = callInfo.Clone();
//                    cb.PartNumber = i == listOfParts.Count ? (short)-1 : (short)i;
//                    json = (int)DataType.CallMethod + "," + (int)CompressMode.None + "/" + ServerSerializationHelper.SerializeObject(cb, serverBase);
//                    byte[] bytes = Encoding.UTF8.GetBytes(json);
//                    await client.StreamHelper.WriteToStreamAsync(client.ClientStream, bytes);
//                    i++;
//                }
//            }
//            else
//            {
//                json = (int)DataType.CallMethod + "," + (int)CompressMode.None + "/" + json;
//                byte[] bytes = Encoding.UTF8.GetBytes(json);
//                await client.StreamHelper.WriteToStreamAsync(client.ClientStream, bytes);
//            }
//            object value = taskCompletionSource.GetType().GetProperty("Task").GetValue(taskCompletionSource, null);
//            T result = await (Task<T>)value;
//            return result;
//#endif
//        }
       

    }

}
