using Newtonsoft.Json;
using SignalGo;
using SignalGo.Server.Helpers;
using SignalGo.Server.Models;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.IO;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace SignalGo.Server.ServiceManager
{
    public abstract class ServerStreamBase : ServerBase
    {
        public override StreamInfo RegisterFileToDownload(NetworkStream stream, CompressMode compressMode, ClientInfo client, bool isWebSocket)
        {
            var bytes = GoStreamReader.ReadBlockToEnd(stream, compressMode, ProviderSetting.MaximumReceiveDataBlock, isWebSocket);
            var json = Encoding.UTF8.GetString(bytes);
            MethodCallInfo callInfo = ServerSerializationHelper.Deserialize<MethodCallInfo>(json);

            MethodCallbackInfo callback = new MethodCallbackInfo();
            callback.Guid = callInfo.Guid;

            var serviceType = RegisteredServiceTypes[callInfo.ServiceName];
            var sessionId = callInfo.Data.ToString();
            var clientInfo = (from x in Services.ToArray() where x.Key.SessionId == sessionId select x.Key).FirstOrDefault();
            if (clientInfo == null)
                throw new Exception("RegisterFile client not found!");
            var service = FindClientServiceByType(clientInfo, serviceType);

#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var method = serviceType.GetTypeInfo().GetMethod(callInfo.MethodName, RuntimeTypeHelper.GetMethodTypes(serviceType, callInfo).ToArray());
#else
            var method = serviceType.GetMethod(callInfo.MethodName, RuntimeTypeHelper.GetMethodTypes(serviceType, callInfo).ToArray());
#endif
            List<object> parameters = new List<object>();
            int index = 0;
            var prms = method.GetParameters();
            foreach (var item in callInfo.Parameters)
            {
                parameters.Add(ServerSerializationHelper.Deserialize(item.Value, prms[index].ParameterType));
                index++;
            }
            if (method.ReturnType != typeof(StreamInfo))
            {
                throw new Exception("return type for upload must StreamInfo!");
            }
            else
            {
                StreamInfo data = null;
                data = (StreamInfo)method.Invoke(service, parameters.ToArray());
                if (data == null)
                    throw new Exception($"StreamInfo cannot be null");
                var streamReader = data.Stream;
                data.Stream = null;
                callback.Data = ServerSerializationHelper.SerializeObject(data, this);
                SendCallbackData(callback, client);
                data.Stream = streamReader;
                return data;
            }
        }

        public override void WriteStreamToClient(StreamInfo streamInfo, NetworkStream toWrite, bool isWebSocket)
        {
            var readStream = streamInfo.Stream;
            while (true)
            {
                var bytes = new byte[1024];
                var readCount = readStream.Read(bytes, 0, bytes.Length);
                if (readCount == 0)
                    break;
                GoStreamWriter.WriteToStream(toWrite, bytes.ToList().GetRange(0, readCount).ToArray(), isWebSocket);
            }
        }

        public override void RegisterFileToUpload(NetworkStream stream, CompressMode compressMode, ClientInfo client, bool isWebSocket)
        {
            var bytes = GoStreamReader.ReadBlockToEnd(stream, compressMode, ProviderSetting.MaximumReceiveDataBlock, isWebSocket);
            var json = Encoding.UTF8.GetString(bytes);
            MethodCallInfo callInfo = ServerSerializationHelper.Deserialize<MethodCallInfo>(json, this);

            MethodCallbackInfo callback = new MethodCallbackInfo();
            callback.Guid = callInfo.Guid;

            var serviceType = RegisteredServiceTypes[callInfo.ServiceName];
            var sessionId = callInfo.Data.ToString();
            var clientInfo = (from x in Services.ToArray() where x.Key.SessionId == sessionId select x.Key).FirstOrDefault();
            if (clientInfo == null)
                throw new Exception("RegisterFile client not found!");
            var service = FindClientServiceByType(clientInfo, serviceType);

#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var method = serviceType.GetTypeInfo().GetMethod(callInfo.MethodName, RuntimeTypeHelper.GetMethodTypes(serviceType, callInfo).ToArray());
#else
            var method = serviceType.GetMethod(callInfo.MethodName, RuntimeTypeHelper.GetMethodTypes(serviceType, callInfo).ToArray());
#endif
            List<object> parameters = new List<object>();
            int index = 0;
            var prms = method.GetParameters();
            foreach (var item in callInfo.Parameters)
            {
                parameters.Add(ServerSerializationHelper.Deserialize(item.Value, prms[index].ParameterType, this));
                index++;
            }
            StreamInfo data = parameters[0] as StreamInfo;
            data.Stream = stream;
            method.Invoke(service, parameters.ToArray());
        }
    }
}
