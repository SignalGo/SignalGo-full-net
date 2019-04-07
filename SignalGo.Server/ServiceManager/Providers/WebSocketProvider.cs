using SignalGo.Server.Helpers;
using SignalGo.Server.Models;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.Managers;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Server.ServiceManager.Providers
{
    public class WebSocketProvider : BaseProvider
    {
        public static async Task StartToReadingClientData(ClientInfo client, ServerBase serverBase)
        {
            try
            {
                Console.WriteLine($"WebSocket Client Connected: {client.IPAddress}");
                Shared.IO.PipeNetworkStream stream = client.ClientStream;
                while (true)
                {
                    byte oneByteOfDataType = await client.StreamHelper.ReadOneByteAsync(stream);
                    //type of data
                    DataType dataType = (DataType)oneByteOfDataType;
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

                        string json = "";
                        //if (client.IsOwinClient)
                        //{
                        json = await stream.ReadLineAsync("#end");
                        //}
                        //else
                        //{
                        //    do
                        //    {
                        //        byte[] bytes = await client.StreamHelper.ReadBlockToEndAsync(stream, compressMode, serverBase.ProviderSetting.MaximumReceiveDataBlock);
                        //        //if (ClientsSettings.ContainsKey(client))
                        //        //    bytes = DecryptBytes(bytes, client);
                        //        json += Encoding.UTF8.GetString(bytes);
                        //    }
                        //    while (json.IndexOf("#end") != json.Length - 4);
                        //}

                        if (json.EndsWith("#end"))
                            json = json.Substring(0, json.Length - 4);
                        MethodCallInfo callInfo = ServerSerializationHelper.Deserialize<MethodCallInfo>(json, serverBase);
                        if (callInfo.PartNumber != 0)
                        {
                            SegmentManager segmentManager = new SegmentManager();
                            ISegment result = segmentManager.GenerateAndMixSegments(callInfo);
                            if (result != null)
                                callInfo = (MethodCallInfo)result;
                            else
                                continue;
                        }

#if (NET35 || NET40)
                        MethodCallbackInfo callbackResult = CallMethod(callInfo, client, json, serverBase).Result;
                        SendCallbackData(callbackResult, client, serverBase);
#else
                        Task<MethodCallbackInfo> callbackResult = CallMethod(callInfo, client, json, serverBase);
                        SendCallbackData(callbackResult, client, serverBase);
#endif
                    }

                    //reponse of client method that server called to client
                    else if (dataType == DataType.ResponseCallMethod)
                    {
                        string json = "";
                        //if (client.IsOwinClient)
                        //{
                        json = await stream.ReadLineAsync("#end");
                        //}
                        //else
                        //{
                        //    byte[] bytes = await client.StreamHelper.ReadBlockToEndAsync(stream, compressMode, serverBase.ProviderSetting.MaximumReceiveDataBlock);
                        //    //if (ClientsSettings.ContainsKey(client))
                        //    //    bytes = DecryptBytes(bytes, client);
                        //    json = Encoding.UTF8.GetString(bytes);
                        //}

                        if (json.EndsWith("#end"))
                            json = json.Substring(0, json.Length - 4);

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
                            if (callback.IsException)
                                resultTask.Value.GetType().FindMethod("SetException").Invoke(resultTask.Value, new object[] { new Exception(callback.Data) });
                            else
                                resultTask.Value.GetType().FindMethod("SetResult").Invoke(resultTask.Value, new object[] { ServerSerializationHelper.Deserialize(callback.Data, resultTask.Key, serverBase) });
                        }
                        //var geted = WaitedMethodsForResponse.TryGetValue(client, out ConcurrentDictionary<string, KeyValue<AutoResetEvent, MethodCallbackInfo>> keyValue);
                        //if (geted)
                        //{
                        //    if (keyValue.ContainsKey(callback.Guid))
                        //    {
                        //        keyValue[callback.Guid].Value = callback;
                        //        keyValue[callback.Guid].Key.Set();
                        //    }
                        //}
                    }
                    else if (dataType == DataType.GetServiceDetails)
                    {
                        byte[] bytes = await client.StreamHelper.ReadBlockToEndAsync(stream, compressMode, serverBase.ProviderSetting.MaximumReceiveDataBlock);
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
                        byte[] bytes = await client.StreamHelper.ReadBlockToEndAsync(stream, compressMode, serverBase.ProviderSetting.MaximumReceiveDataBlock);
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
                serverBase.AutoLogger.LogError(ex, $"{client.IPAddress} {client.ClientId} ServerBase SignalGoDuplexServiceProvider StartToReadingClientData");
                serverBase.DisposeClient(client, null, "SignalGoDuplexServiceProvider StartToReadingClientData exception");
            }
        }

        /// <summary>
        /// send result of calling method from client
        /// client is waiting for get response from server when calling method
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="client"></param>
        /// <param name="serverBase"></param>
#if (NET35 || NET40)
        private static void SendCallbackData(Task<MethodCallbackInfo> callback, ClientInfo client, ServerBase serverBase)
#else
        private static async void SendCallbackData(Task<MethodCallbackInfo> callback, ClientInfo client, ServerBase serverBase)
#endif
        {
            try
            {
#if (NET35 || NET40)
                MethodCallbackInfo callbackResult = callback.Result;
#else
                MethodCallbackInfo callbackResult = await callback;
#endif
                string json = ServerSerializationHelper.SerializeObject(callbackResult, serverBase);
                if (json.Length > 30000)
                {
                    List<string> listOfParts = GeneratePartsOfData(json);
                    int i = 1;
                    foreach (string item in listOfParts)
                    {
                        MethodCallbackInfo cb = callbackResult.Clone();
                        cb.PartNumber = i == listOfParts.Count ? (short)-1 : (short)i;
                        cb.Data = item;
                        json = (int)DataType.ResponseCallMethod + "," + (int)CompressMode.None + "/" + ServerSerializationHelper.SerializeObject(cb, serverBase);
                        byte[] result = Encoding.UTF8.GetBytes(json);
                        //if (ClientsSettings.ContainsKey(client))
                        //    result = EncryptBytes(result, client);
                        await client.StreamHelper.WriteToStreamAsync(client.ClientStream, result);
                        i++;
                    }
                }
                else
                {
                    json = (int)DataType.ResponseCallMethod + "," + (int)CompressMode.None + "/" + json;
                    byte[] result = Encoding.UTF8.GetBytes(json);
                    //if (ClientsSettings.ContainsKey(client))
                    //    result = EncryptBytes(result, client);
                    await client.StreamHelper.WriteToStreamAsync(client.ClientStream, result);
                }
            }
            catch (Exception ex)
            {
                serverBase.AutoLogger.LogError(ex, $"{client.IPAddress} {client.ClientId} ServerBase SendCallbackData");
                //if (!client.TcpClient.Connected)
                //    serverBase.DisposeClient(client, "SendCallbackData exception");
            }
            finally
            {
                //ClientConnectedCallingCount--;
            }
        }

        public static List<string> GeneratePartsOfData(string data)
        {
            int partCount = (int)Math.Ceiling((double)data.Length / 30000);
            List<string> partData = new List<string>();
            for (int i = 0; i < partCount; i++)
            {
                if (i != partCount - 1)
                {
                    partData.Add(data.Substring((i * 30000), 30000));
                }
                else
                {
                    partData.Add(data.Substring((i * 30000), data.Length - (i * 30000)));
                }
            }
            return partData;
        }

    }
}
