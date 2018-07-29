using Newtonsoft.Json;
using SignalGo;
using SignalGo.Server.Helpers;
using SignalGo.Server.IO;
using SignalGo.Server.Models;
using SignalGo.Shared.Events;
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
        public override StreamInfo RegisterFileToDownload(Stream stream, CompressMode compressMode, ClientInfo client, bool isWebSocket)
        {
            var bytes = GoStreamReader.ReadBlockToEnd(stream, compressMode, ProviderSetting.MaximumReceiveDataBlock, isWebSocket);
            var json = Encoding.UTF8.GetString(bytes);
            MethodCallInfo callInfo = ServerSerializationHelper.Deserialize<MethodCallInfo>(json);

            MethodCallbackInfo callback = new MethodCallbackInfo();
            callback.Guid = callInfo.Guid;

            var serviceType = RegisteredServiceTypes[callInfo.ServiceName];
            var clientId = callInfo.Data.ToString();
            var clientInfo = Clients[clientId];
            if (clientInfo == null)
                throw new Exception("RegisterFile client not found!");
            var service = FindServerServiceByType(clientInfo, serviceType, null);

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

        public override void WriteStreamToClient(StreamInfo streamInfo, Stream toWrite, bool isWebSocket)
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

        public override void RegisterFileToUpload(Stream stream, CompressMode compressMode, ClientInfo client, bool isWebSocket)
        {
            var bytes = GoStreamReader.ReadBlockToEnd(stream, compressMode, ProviderSetting.MaximumReceiveDataBlock, isWebSocket);
            var json = Encoding.UTF8.GetString(bytes);
            MethodCallInfo callInfo = ServerSerializationHelper.Deserialize<MethodCallInfo>(json, this);

            MethodCallbackInfo callback = new MethodCallbackInfo();
            callback.Guid = callInfo.Guid;

            var serviceType = RegisteredServiceTypes[callInfo.ServiceName];
            var clientId = callInfo.Data.ToString();
            var clientInfo = Clients[clientId];
            if (clientInfo == null)
                throw new Exception("RegisterFile client not found!");
            var service = FindServerServiceByType(clientInfo, serviceType, null);

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


        /// <summary>
        /// this method call when client want to upload file or stream to your server
        /// </summary>
        /// <param name="stream">client stream</param>
        /// <param name="client">client</param>
        public override void DownloadStreamFromClient(Stream stream, ClientInfo client)
        {
            MethodCallbackInfo callback = new MethodCallbackInfo();
            string guid = Guid.NewGuid().ToString();
            Exception exception = null;
            string serviceName = null;
            string methodName = null;
            string jsonResult = null;
            List<SignalGo.Shared.Models.ParameterInfo> values = null;
            try
            {
                var bytes = GoStreamReader.ReadBlockToEnd(stream, CompressMode.None, ProviderSetting.MaximumReceiveStreamHeaderBlock, false);
                var json = Encoding.UTF8.GetString(bytes);
                MethodCallInfo callInfo = ServerSerializationHelper.Deserialize<MethodCallInfo>(json, this);
                serviceName = callInfo.ServiceName;
                methodName = callInfo.MethodName;
                values = callInfo.Parameters;
                var service = FindStreamServiceByName(serviceName);
                MethodsCallHandler.BeginStreamCallAction?.Invoke(client, guid, serviceName, methodName, values);
                if (service == null)
                    DisposeClient(client, "DownloadStreamFromClient service not found!");
                else
                {
                    //, typeof(StreamInfo<>)
                    var serviceType = service.GetType();
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                    var method = serviceType.GetTypeInfo().GetMethod(methodName, RuntimeTypeHelper.GetMethodTypes(serviceType, callInfo).ToArray());
#else
                    var method = serviceType.GetMethod(methodName, RuntimeTypeHelper.GetMethodTypes(serviceType, callInfo).ToArray());
#endif
                    List<object> parameters = new List<object>();
                    int index = 0;
                    var prms = method.GetParameters();
                    foreach (var item in values)
                    {
                        parameters.Add(ServerSerializationHelper.Deserialize(item.Value, prms[index].ParameterType, this));
                        index++;
                    }
                    var data = (IStreamInfo)parameters.FirstOrDefault(x => x.GetType() == typeof(StreamInfo) || (x.GetType().GetIsGenericType() && x.GetType().GetGenericTypeDefinition() == typeof(StreamInfo<>)));
                    //var upStream = new UploadStreamGo(stream);
                    data.Stream = stream;
                    //upStream.SetLengthOfBase(data.Length);
                    //data.Stream = stream;
                    if (method.ReturnType == typeof(void))
                    {
                        method.Invoke(service, parameters.ToArray());
                    }
                    else
                    {
                        var result = method.Invoke(service, parameters.ToArray());
                        jsonResult = callback.Data = ServerSerializationHelper.SerializeObject(result);
                    }
                    //if (!upStream.IsFinished)
                    //{
                    //    throw new IOException("stream read must finished for send response to client");
                    //}
                }
            }
            catch (IOException ex)
            {
                exception = ex;
                AutoLogger.LogError(ex, "upload stream error");
                DisposeClient(client, "DownloadStreamFromClient exception");
                return;
            }
            catch (Exception ex)
            {
                exception = ex;
                callback.IsException = true;
                callback.Data = ServerSerializationHelper.SerializeObject(ex);
            }
            finally
            {
                MethodsCallHandler.EndStreamCallAction?.Invoke(client, guid, serviceName, methodName, values, jsonResult, exception);
            }
            SendCallbackData(callback, client);
            if (callback.IsException)
            {
                DisposeClient(client, "DownloadStreamFromClient exception 2");
            }
        }
        /// <summary>
        /// this method calll when client want to download file or stream from your server
        /// </summary>
        /// <param name="stream">client stream</param>
        /// <param name="client">client</param>
        public override void UploadStreamToClient(Stream stream, ClientInfo client)
        {
            MethodCallbackInfo callback = new MethodCallbackInfo();
            IDisposable userStreamDisposable = null;
            Stream userStream = null;

            string guid = Guid.NewGuid().ToString();
            Exception exception = null;
            string serviceName = null;
            string methodName = null;
            string jsonResult = null;
            List<SignalGo.Shared.Models.ParameterInfo> values = null;

            try
            {
                var bytes = GoStreamReader.ReadBlockToEnd(stream, CompressMode.None, ProviderSetting.MaximumReceiveStreamHeaderBlock, false);
                var json = Encoding.UTF8.GetString(bytes);
                MethodCallInfo callInfo = ServerSerializationHelper.Deserialize<MethodCallInfo>(json, this);
                serviceName = callInfo.ServiceName;
                methodName = callInfo.MethodName;
                values = callInfo.Parameters;
                var service = FindStreamServiceByName(serviceName);
                MethodsCallHandler.BeginStreamCallAction?.Invoke(client, guid, serviceName, methodName, values);
                if (service == null)
                    DisposeClient(client, "UploadStreamToClient service not found");
                else
                {
                    var serviceType = service.GetType();
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                    var method = serviceType.GetTypeInfo().GetMethod(methodName, RuntimeTypeHelper.GetMethodTypes(serviceType, callInfo).ToArray());
#else
                    var method = serviceType.GetMethod(methodName, RuntimeTypeHelper.GetMethodTypes(serviceType, callInfo).ToArray());
#endif
                    List<object> parameters = new List<object>();
                    int index = 0;
                    var prms = method.GetParameters();
                    foreach (var item in values)
                    {
                        parameters.Add(ServerSerializationHelper.Deserialize(item.Value, prms[index].ParameterType, this));
                        index++;
                    }

                    userStreamDisposable = (IDisposable)method.Invoke(service, parameters.ToArray());
                    userStream = (Stream)userStreamDisposable.GetType().GetPropertyInfo("Stream").GetValue(userStreamDisposable, null);
                    long len = (long)userStreamDisposable.GetType().GetPropertyInfo("Length").GetValue(userStreamDisposable, null);

                    jsonResult = callback.Data = ServerSerializationHelper.SerializeObject(userStreamDisposable);

                    json = ServerSerializationHelper.SerializeObject(callback, this);
                    bytes = Encoding.UTF8.GetBytes(json);
                    byte[] lenBytes = BitConverter.GetBytes(bytes.Length);
                    stream.Write(lenBytes, 0, lenBytes.Length);
                    stream.Write(bytes, 0, bytes.Length);

                    //read one byte to start send data to client client can cancel socket after get headers
                    //for example when client is cach images and get last upadate heade and dont need to download file he can dispose socket befor sending any data
                    //client.TcpClient.Client.
                    //Console.WriteLine("poll to read");
                    //if (client.TcpClient.Client.Poll(10000, SelectMode.SelectRead))
                    //{
                    //    Console.WriteLine("poll read ok");
                    //    stream.ReadByte();
                    //}
                    //else
                    //{
                    //    var connected = client.TcpClient.Connected;
                    //    var dataav = stream.DataAvailable;
                    //    userStream.Dispose();
                    //    DisposeClient(client);
                    //    Console.WriteLine("poll read error, client disposed " + connected + "," + dataav);
                    //    return;
                    //}
                    //Console.WriteLine("poll read started");
                    GoStreamReader.ReadOneByte(stream, new TimeSpan(0, 0, 10));
                    long writeLen = 0;
                    while (writeLen < len)
                    {
                        bytes = new byte[1024 * 100];
                        var readCount = userStream.Read(bytes, 0, bytes.Length);
                        stream.Write(bytes, 0, readCount);
                        writeLen += readCount;
                    }
                    userStream.Dispose();
                    Console.WriteLine("user stream finished");
                    stream.Dispose();
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                if (userStreamDisposable == null)
                    userStreamDisposable.Dispose();
                stream.Dispose();
                if (userStream != null)
                {
                    userStream.Dispose();
                    Console.WriteLine("user stream disposed");
                }
                callback.IsException = true;
                callback.Data = ServerSerializationHelper.SerializeObject(ex);
                SendCallbackData(callback, client);
            }
            finally
            {
                MethodsCallHandler.EndStreamCallAction?.Invoke(client, guid, serviceName, methodName, values, jsonResult, exception);
            }
            if (callback.IsException)
            {
                DisposeClient(client, "UploadStreamToClient exception");
            }
        }

    }
}
