using SignalGo.Server.Helpers;
using SignalGo.Server.Models;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.IO;
using SignalGo.Shared.Managers;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Server.ServiceManager.Providers
{
    /// <summary>
    /// manage data providing of duplex services
    /// </summary>
    public class SignalGoDuplexServiceProvider : BaseProvider
    {
        public static void StartToReadingClientData(ClientInfo client, ServerBase serverBase)
        {
            try
            {
                Console.WriteLine($"Duplex Client Connected: {client.IPAddress}");
                var stream = client.ClientStream;
                while (client.TcpClient.Connected)
                {
                    var oneByteOfDataType = client.StreamHelper.ReadOneByte(stream, CompressMode.None, serverBase.ProviderSetting.MaximumReceiveDataBlock);
                    //type of data
                    var dataType = (DataType)oneByteOfDataType;
                    if (dataType == DataType.PingPong)
                    {
                        client.StreamHelper.WriteToStream(client.ClientStream, new byte[] { 5 });
                        continue;
                    }
                    //compress mode of data
                    var compressMode = (CompressMode)client.StreamHelper.ReadOneByte(stream, CompressMode.None, serverBase.ProviderSetting.MaximumReceiveDataBlock);
                    //a server service method called from client
                    if (dataType == DataType.CallMethod)
                    {
                        var bytes = client.StreamHelper.ReadBlockToEnd(stream, compressMode, serverBase.ProviderSetting.MaximumReceiveDataBlock);
                        //if (ClientsSettings.ContainsKey(client))
                        //    bytes = DecryptBytes(bytes, client);
                        var json = Encoding.UTF8.GetString(bytes);
                        MethodCallInfo callInfo = ServerSerializationHelper.Deserialize<MethodCallInfo>(json, serverBase);
                        if (callInfo.PartNumber != 0)
                        {
                            SegmentManager segmentManager = new SegmentManager();
                            var result = segmentManager.GenerateAndMixSegments(callInfo);
                            if (result != null)
                                callInfo = (MethodCallInfo)result;
                            else
                                continue;
                        }
                        var callbackResult = CallMethod(callInfo, client, json, serverBase);
                        SendCallbackDataAsync(callbackResult, client, serverBase);
                    }

                    //reponse of client method that server called to client
                    else if (dataType == DataType.ResponseCallMethod)
                    {
                        var bytes = client.StreamHelper.ReadBlockToEnd(stream, compressMode, serverBase.ProviderSetting.MaximumReceiveDataBlock);
                        //if (ClientsSettings.ContainsKey(client))
                        //    bytes = DecryptBytes(bytes, client);
                        var json = Encoding.UTF8.GetString(bytes);
                        MethodCallbackInfo callback = ServerSerializationHelper.Deserialize<MethodCallbackInfo>(json, serverBase);
                        if (callback == null)
                            serverBase.AutoLogger.LogText($"{client.IPAddress} {client.ClientId} callback is null:" + json);
                        if (callback.PartNumber != 0)
                        {
                            SegmentManager segmentManager = new SegmentManager();
                            var result = segmentManager.GenerateAndMixSegments(callback);
                            if (result != null)
                                callback = (MethodCallbackInfo)result;
                            else
                                continue;
                        }
                        if (serverBase.ClientServiceCallMethodsResult.TryGetValue(callback.Guid, out KeyValue<Type, object> resultTask))
                            resultTask.Value.GetType().FindMethod("SetResult").Invoke(resultTask.Value, new object[] { ServerSerializationHelper.Deserialize(callback.Data, resultTask.Key, serverBase) });
                    }
                    else if (dataType == DataType.GetServiceDetails)
                    {
                        var bytes = client.StreamHelper.ReadBlockToEnd(stream, compressMode, serverBase.ProviderSetting.MaximumReceiveDataBlock);
                        //if (ClientsSettings.ContainsKey(client))
                        //    bytes = DecryptBytes(bytes, client);
                        var json = Encoding.UTF8.GetString(bytes);
                        var hostUrl = ServerSerializationHelper.Deserialize<string>(json, serverBase);
                        ServerServicesManager serverServicesManager = new ServerServicesManager();
                        serverServicesManager.SendServiceDetail(client, hostUrl, serverBase);
                    }
                    else if (dataType == DataType.GetMethodParameterDetails)
                    {
                        var bytes = client.StreamHelper.ReadBlockToEnd(stream, compressMode, serverBase.ProviderSetting.MaximumReceiveDataBlock);
                        //if (ClientsSettings.ContainsKey(client))
                        //    bytes = DecryptBytes(bytes, client);
                        var json = Encoding.UTF8.GetString(bytes);
                        var detail = ServerSerializationHelper.Deserialize<MethodParameterDetails>(json, serverBase);
                        ServerServicesManager serverServicesManager = new ServerServicesManager();
                        serverServicesManager.SendMethodParameterDetail(client, detail, serverBase);
                    }
                    else if (dataType == DataType.GetClientId)
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(client.ClientId);
                        //if (ClientsSettings.ContainsKey(client))
                        //    bytes = EncryptBytes(bytes, client);
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

                        client.StreamHelper.WriteToStream(client.ClientStream, data.ToArray());
                    }
                    else
                    {
                        //throw new Exception($"Correct DataType Data {dataType}");
                        serverBase.AutoLogger.LogText($"Correct DataType Data {oneByteOfDataType} {client.ClientId} {client.IPAddress}");
                        break;
                    }
                }
                serverBase.DisposeClient(client, "StartToReadingClientData while break");
            }
            catch (Exception ex)
            {
                serverBase.AutoLogger.LogError(ex, $"{client.IPAddress} {client.ClientId} ServerBase SignalGoDuplexServiceProvider StartToReadingClientData");
                serverBase.DisposeClient(client, "SignalGoDuplexServiceProvider StartToReadingClientData exception");
            }
        }
    }
}
