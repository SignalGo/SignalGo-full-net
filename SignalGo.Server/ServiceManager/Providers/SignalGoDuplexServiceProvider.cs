using SignalGo.Server.Models;
using SignalGo.Shared.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace SignalGo.Server.ServiceManager.Providers
{
    /// <summary>
    /// manage data providing of duplex services
    /// </summary>
    public static class SignalGoDuplexServiceProvider
    {
        public static void StartToReadingClientData(ClientInfo client, ServerBase serverBase)
        {
            try
            {
                RegisterClientServices(client);
                var stream = client.ClientStream;
                while (client.TcpClient.Connected)
                {
                    var oneByteOfDataType = GoStreamReader.ReadOneByte(stream, CompressMode.None, serverBase.ProviderSetting.MaximumReceiveDataBlock, client.IsWebSocket);
                    //type of data
                    var dataType = (DataType)oneByteOfDataType;
                    if (dataType == DataType.PingPong)
                    {
                        //AutoLogger.LogText($"PingPong {client.IsWebSocket} {client.SessionId} {client.IPAddress}");
                        GoStreamWriter.WriteToStream(client.ClientStream, new byte[] { 5 }, client.IsWebSocket);
                        continue;
                    }
                    //compress mode of data
                    var compressMode = (CompressMode)GoStreamReader.ReadOneByte(stream, CompressMode.None, serverBase.ProviderSetting.MaximumReceiveDataBlock, client.IsWebSocket);
                    //یکی از متد های سرور توسط این کلاینت صدا زده شده
                    if (dataType == DataType.CallMethod)
                    {
                        string json = "";
                        do
                        {
                            var bytes = GoStreamReader.ReadBlockToEnd(stream, compressMode, serverBase.ProviderSetting.MaximumReceiveDataBlock, client.IsWebSocket);
                            if (ClientsSettings.ContainsKey(client))
                                bytes = DecryptBytes(bytes, client);
                            json += Encoding.UTF8.GetString(bytes);
                        }
                        while (client.IsWebSocket && json.IndexOf("#end") != json.Length - 4);

                        if (client.IsWebSocket)
                        {
                            if (json.IndexOf("#end") == json.Length - 4)
                                json = json.Substring(0, json.Length - 4);
                        }
                        MethodCallInfo callInfo = ServerSerializationHelper.Deserialize<MethodCallInfo>(json, this);
                        if (callInfo.PartNumber != 0)
                        {
                            var result = CurrentSegmentManager.GenerateAndMixSegments(callInfo);
                            if (result != null)
                                callInfo = (MethodCallInfo)result;
                            else
                                continue;
                        }
                        if (callInfo == null)
                            AutoLogger.LogText($"{client.IPAddress} {client.ClientId} callinfo is null:" + json);
                        //else
                        //    AutoLogger.LogText(callInfo.MethodName);
                        //ست کردن تنظیمات
                        if (callInfo.ServiceName == "/SetSettings")
                            SetSettings(ServerSerializationHelper.Deserialize<SecuritySettingsInfo>(callInfo.Data.ToString(), this), callInfo, client, json);
                        //بررسی آدرس اتصال
                        else if (callInfo.ServiceName == "/CheckConnection")
                            SendCallbackData(new MethodCallbackInfo() { Guid = callInfo.Guid, Data = ServerSerializationHelper.SerializeObject(true, this) }, client);
                        //کلاسی کالبکی که سمت سرور جدید میشه
                        else if (callInfo.MethodName == "/RegisterService")
                            CalculateRegisterServerServiceForClient(callInfo, client);
                        //متد هایی که لازمه برای کلاینت کال بشه
                        else if (callInfo.MethodName == "/RegisterClientMethods")
                        {
                            RegisterMethodsForClient(callInfo, client);
                        }
                        //حذف متد هایی که قبلا رجیستر شده بود
                        else if (callInfo.MethodName == "/UnRegisterClientMethods")
                        {
                            UnRegisterMethodsForClient(callInfo, client);
                        }
                        else
                            CallMethod(callInfo, client, json);
                    }
                    //پاسخ دریافت شده از صدا زدن یک متد از کلاینت توسط سرور
                    else if (dataType == DataType.ResponseCallMethod)
                    {
                        var bytes = GoStreamReader.ReadBlockToEnd(stream, compressMode, ProviderSetting.MaximumReceiveDataBlock, client.IsWebSocket);
                        if (ClientsSettings.ContainsKey(client))
                            bytes = DecryptBytes(bytes, client);
                        var json = Encoding.UTF8.GetString(bytes);
                        MethodCallbackInfo callback = ServerSerializationHelper.Deserialize<MethodCallbackInfo>(json, this);
                        if (callback == null)
                            AutoLogger.LogText($"{client.IPAddress} {client.ClientId} callback is null:" + json);
                        if (callback.PartNumber != 0)
                        {
                            var result = CurrentSegmentManager.GenerateAndMixSegments(callback);
                            if (result != null)
                                callback = (MethodCallbackInfo)result;
                            else
                                continue;
                        }
                        var geted = WaitedMethodsForResponse.TryGetValue(client, out ConcurrentDictionary<string, KeyValue<AutoResetEvent, MethodCallbackInfo>> keyValue);
                        if (geted)
                        {
                            if (keyValue.ContainsKey(callback.Guid))
                            {
                                keyValue[callback.Guid].Value = callback;
                                keyValue[callback.Guid].Key.Set();
                            }
                        }
                    }
                    else if (dataType == DataType.RegisterFileDownload)
                    {
                        using (var writeToClientStrem = RegisterFileToDownload(stream, compressMode, client, client.IsWebSocket))
                        {
                            WriteStreamToClient(writeToClientStrem, stream, client.IsWebSocket);
                        }
                        break;
                    }
                    else if (dataType == DataType.RegisterFileUpload)
                    {
                        RegisterFileToUpload(stream, compressMode, client, client.IsWebSocket);
                        break;
                    }
                    else if (dataType == DataType.GetServiceDetails)
                    {
                        var bytes = GoStreamReader.ReadBlockToEnd(stream, compressMode, ProviderSetting.MaximumReceiveDataBlock, client.IsWebSocket);
                        if (ClientsSettings.ContainsKey(client))
                            bytes = DecryptBytes(bytes, client);
                        var json = Encoding.UTF8.GetString(bytes);
                        var hostUrl = ServerSerializationHelper.Deserialize<string>(json, this);
                        SendServiceDetail(client, hostUrl);
                    }
                    else if (dataType == DataType.GetMethodParameterDetails)
                    {
                        var bytes = GoStreamReader.ReadBlockToEnd(stream, compressMode, ProviderSetting.MaximumReceiveDataBlock, client.IsWebSocket);
                        if (ClientsSettings.ContainsKey(client))
                            bytes = DecryptBytes(bytes, client);
                        var json = Encoding.UTF8.GetString(bytes);
                        var detail = ServerSerializationHelper.Deserialize<MethodParameterDetails>(json, this);
                        SendMethodParameterDetail(client, detail);
                    }
                    else if (dataType == DataType.GetClientId)
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(client.ClientId);
                        if (ClientsSettings.ContainsKey(client))
                            bytes = EncryptBytes(bytes, client);
                        byte[] len = BitConverter.GetBytes(bytes.Length);
                        List<byte> data = new List<byte>
                            {
                                (byte)DataType.GetClientId,
                                (byte)CompressMode.None
                            };
                        data.AddRange(len);
                        data.AddRange(bytes);
                        if (data.Count > ProviderSetting.MaximumSendDataBlock)
                            throw new Exception($"{client.IPAddress} {client.ClientId} GetClientId data length exceeds MaximumSendDataBlock");

                        GoStreamWriter.WriteToStream(client.ClientStream, data.ToArray(), client.IsWebSocket);
                    }
                    else
                    {
                        //throw new Exception($"Correct DataType Data {dataType}");
                        AutoLogger.LogText($"Correct DataType Data {oneByteOfDataType} {client.ClientId} {client.IPAddress}");
                        break;
                    }
                }
                DisposeClient(client, "StartToReadingClientData while break");
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex);
                AutoLogger.LogError(ex, $"{client.IPAddress} {client.ClientId} ServerBase StartToReadingClientData");
                DisposeClient(client, "StartToReadingClientData exception");
            }
        }
    }
}
