using SignalGo.Server.Helpers;
using SignalGo.Server.Models;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.IO;
using SignalGo.Shared.IO.Compressions;
using SignalGo.Shared.Managers;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Server.ServiceManager.Providers
{
    /// <summary>
    /// manage data providing of duplex services
    /// </summary>
    public class SignalGoDuplexServiceProvider : BaseProvider
    {
        public static async Task StartToReadingClientData(ClientInfo client, ServerBase serverBase)
        {
            try
            {
                Console.WriteLine($"Duplex Client Connected: {client.IPAddress}");
                PipeNetworkStream stream = client.ClientStream;
                while (true)
                {
                    Debug.WriteLine($"Wait for get data from client: {client.IPAddress}");
                    Console.WriteLine($"Wait for get data from client: {client.IPAddress}");
                    byte oneByteOfDataType = await client.StreamHelper.ReadOneByteAsync(stream);
                    //type of data
                    DataType dataType = (DataType)oneByteOfDataType;
                    Debug.WriteLine($"Call DataType received: {dataType.ToString()}");
                    Console.WriteLine($"Call DataType received: {dataType.ToString()}");
                    if (dataType == DataType.PingPong)
                    {
                        await client.StreamHelper.WriteToStreamAsync(client.ClientStream, new byte[] { 5 });
                        continue;
                    }
                    //compress mode of data
                    CompressMode compressMode = (CompressMode)await client.StreamHelper.ReadOneByteAsync(stream);
                    //a server service method called from client
                    if (dataType == DataType.CallMethod)
                    {
                        Debug.WriteLine($"Call Method Reciving data!");
                        Console.WriteLine($"Call Method Reciving data!");
                        byte[] bytes = await client.StreamHelper.ReadBlockToEndAsync(stream, CompressionHelper.GetCompression(compressMode, serverBase.GetCustomCompression), serverBase.ProviderSetting.MaximumReceiveDataBlock);
                        //if (ClientsSettings.ContainsKey(client))
                        //    bytes = DecryptBytes(bytes, client);
                        string json = Encoding.UTF8.GetString(bytes);
                        MethodCallInfo callInfo = ServerSerializationHelper.Deserialize<MethodCallInfo>(json, serverBase);
                        Debug.WriteLine($"Call method received: {callInfo.Guid}");
                        Console.WriteLine($"Call method received: {callInfo.Guid}");
                        if (callInfo.PartNumber != 0)
                        {
                            SegmentManager segmentManager = new SegmentManager();
                            ISegment result = segmentManager.GenerateAndMixSegments(callInfo);
                            if (result != null)
                                callInfo = (MethodCallInfo)result;
                            else
                                continue;
                        }

                        _ = Task.Run(new Func<Task>(async () =>
                        {
                            Debug.WriteLine($"Calling CallMethod: {callInfo.Guid}");
                            MethodCallbackInfo callbackResult = await CallMethod(callInfo, client, json, serverBase);
                            await SendCallbackDataAsync(callbackResult, client, serverBase);
                        }));
                    }

                    //reponse of client method that server called to client
                    else if (dataType == DataType.ResponseCallMethod)
                    {
                        byte[] bytes = await client.StreamHelper.ReadBlockToEndAsync(stream, CompressionHelper.GetCompression(compressMode, serverBase.GetCustomCompression), serverBase.ProviderSetting.MaximumReceiveDataBlock);
                        //if (ClientsSettings.ContainsKey(client))
                        //    bytes = DecryptBytes(bytes, client);
                        string json = Encoding.UTF8.GetString(bytes);
                        MethodCallbackInfo callback = ServerSerializationHelper.Deserialize<MethodCallbackInfo>(json, serverBase);
                        if (callback == null)
                            serverBase.AutoLogger.LogText($"{client.IPAddress} {client.ClientId} callback is null:" + json);
                        if (callback.PartNumber != 0)
                        {
                            SegmentManager segmentManager = new SegmentManager();
                            ISegment result = segmentManager.GenerateAndMixSegments(callback);
                            if (result != null)
                                callback = (MethodCallbackInfo)result;
                            else
                                continue;
                        }
                        if (serverBase.ClientServiceCallMethodsResult.TryGetValue(callback.Guid, out KeyValue<Type, object> resultTask))
                        {
                            serverBase.ClientServiceCallMethodsResult.TryRemove(callback.Guid, out resultTask);
                            if (callback.IsException)
                                resultTask.Value.GetType().FindMethod("SetException").Invoke(resultTask.Value, new object[] { new Exception(callback.Data) });
                            else
                                resultTask.Value.GetType().FindMethod("SetResult").Invoke(resultTask.Value, new object[] { ServerSerializationHelper.Deserialize(callback.Data, resultTask.Key, serverBase) });
                        }
                    }
                    else if (dataType == DataType.GetServiceDetails)
                    {
                        byte[] bytes = await client.StreamHelper.ReadBlockToEndAsync(stream, CompressionHelper.GetCompression(compressMode, serverBase.GetCustomCompression), serverBase.ProviderSetting.MaximumReceiveDataBlock);
                        //if (ClientsSettings.ContainsKey(client))
                        //    bytes = DecryptBytes(bytes, client);
                        string json = Encoding.UTF8.GetString(bytes);
                        string hostUrl = ServerSerializationHelper.Deserialize<string>(json, serverBase);
                        ServerServicesManager serverServicesManager = new ServerServicesManager();
                        ProviderDetailsInfo detail = serverServicesManager.SendServiceDetail(hostUrl, serverBase);
                        json = ServerSerializationHelper.SerializeObject(detail, serverBase);
                        List<byte> resultBytes = new List<byte>
                        {
                            (byte)DataType.GetServiceDetails,
                            (byte)CompressMode.None
                        };
                        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
                        byte[] dataLen = BitConverter.GetBytes(jsonBytes.Length);
                        resultBytes.AddRange(dataLen);
                        resultBytes.AddRange(jsonBytes);
                        await client.StreamHelper.WriteToStreamAsync(client.ClientStream, resultBytes.ToArray());
                    }
                    else if (dataType == DataType.GetMethodParameterDetails)
                    {
                        byte[] bytes = await client.StreamHelper.ReadBlockToEndAsync(stream, CompressionHelper.GetCompression(compressMode, serverBase.GetCustomCompression), serverBase.ProviderSetting.MaximumReceiveDataBlock);
                        //if (ClientsSettings.ContainsKey(client))
                        //    bytes = DecryptBytes(bytes, client);
                        string json = Encoding.UTF8.GetString(bytes);
                        MethodParameterDetails detail = ServerSerializationHelper.Deserialize<MethodParameterDetails>(json, serverBase);

                        if (!serverBase.RegisteredServiceTypes.TryGetValue(detail.ServiceName, out Type serviceType))
                            throw new Exception($"{client.IPAddress} {client.ClientId} Service {detail.ServiceName} not found");
                        if (serviceType == null)
                            throw new Exception($"{client.IPAddress} {client.ClientId} serviceType {detail.ServiceName} not found");

                        ServerServicesManager serverServicesManager = new ServerServicesManager();

                        json = serverServicesManager.SendMethodParameterDetail(serviceType, detail, serverBase);
                        List<byte> resultBytes = new List<byte>
                        {
                            (byte)DataType.GetMethodParameterDetails,
                            (byte)CompressMode.None
                        };

                        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
                        byte[] dataLen = BitConverter.GetBytes(jsonBytes.Length);
                        resultBytes.AddRange(dataLen);
                        resultBytes.AddRange(jsonBytes);
                        await client.StreamHelper.WriteToStreamAsync(client.ClientStream, resultBytes.ToArray());
                    }
                    else if (dataType == DataType.GetClientId)
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(client.ClientId);
                        //if (ClientsSettings.ContainsKey(client))
                        //    bytes = EncryptBytes(bytes, client);
                        bytes = CompressionHelper.GetCompression(serverBase.CurrentCompressionMode, serverBase.GetCustomCompression).Compress(ref bytes);
                        byte[] len = BitConverter.GetBytes(bytes.Length);
                        List<byte> data = new List<byte>
                            {
                                (byte)DataType.GetClientId,
                                (byte)CompressMode.None
                            };
                        data.AddRange(len);
                        data.AddRange(bytes);
                        if (data.Count > serverBase.ProviderSetting.MaximumSendDataBlock)
                            throw new Exception($"{client.IPAddress} {client.ClientId} GetClientId data length exceeds MaximumSendDataBlock");

                        await client.StreamHelper.WriteToStreamAsync(client.ClientStream, data.ToArray());
                    }
                    else
                    {
                        //throw new Exception($"Correct DataType Data {dataType}");
                        serverBase.AutoLogger.LogText($"Correct DataType Data {oneByteOfDataType} {client.ClientId} {client.IPAddress}");
                        break;
                    }
                }
                serverBase.DisposeClient(client, null, "StartToReadingClientData while break");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Client Disconnected exception: {ex.ToString()}");
                serverBase.AutoLogger.LogError(ex, $"{client.IPAddress} {client.ClientId} ServerBase SignalGoDuplexServiceProvider StartToReadingClientData");
                serverBase.DisposeClient(client, null, "SignalGoDuplexServiceProvider StartToReadingClientData exception");
            }
        }
    }
}
