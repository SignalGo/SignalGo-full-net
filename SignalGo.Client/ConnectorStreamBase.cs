using Newtonsoft.Json;
using SignalGo.Shared.IO;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SignalGo.Client
{
    public abstract class ConnectorStreamBase : ConnectorBase
    {
        internal override StreamInfo RegisterFileStreamToDownload(MethodCallInfo Data)
        {
            if (IsDisposed || !_client.Connected)
                return null;
            //connect to tcp
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var downloadFileSocket = new TcpClient();
            var waitable = downloadFileSocket.ConnectAsync(_address, _port);
            waitable.Wait();
#else
            var downloadFileSocket = new TcpClient(_address, _port);
#endif
            var socketStream = downloadFileSocket.GetStream();

            downloadFileSocket.Client.Send(Encoding.UTF8.GetBytes("SignalGo/1.0"));
            //get OK SignalGo/1.0
            int o = socketStream.ReadByte();
            int k = socketStream.ReadByte();

            //register client
            var json = JsonConvert.SerializeObject(new List<string>() { "/DownloadFile" });
            List<byte> bytes = new List<byte>();
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            byte[] dataLen = BitConverter.GetBytes(jsonBytes.Length);
            bytes.AddRange(dataLen);
            bytes.AddRange(jsonBytes);
            GoStreamWriter.WriteToStream(socketStream, bytes.ToArray(), IsWebSocket);

            ///send method data
            json = JsonConvert.SerializeObject(Data);
            bytes = new List<byte>();
            bytes.Add((byte)DataType.RegisterFileDownload);
            bytes.Add((byte)CompressMode.None);
            jsonBytes = Encoding.UTF8.GetBytes(json);
            dataLen = BitConverter.GetBytes(jsonBytes.Length);
            bytes.AddRange(dataLen);
            bytes.AddRange(jsonBytes);
            if (bytes.Count > ProviderSetting.MaximumSendDataBlock)
                throw new Exception("SendData data length is upper than MaximumSendDataBlock");

            GoStreamWriter.WriteToStream(socketStream, bytes.ToArray(), IsWebSocket);

            //get OK SignalGo/1.0
            //int o = socketStream.ReadByte();
            //int k = socketStream.ReadByte();

            //get DataType
            var dataType = (DataType)socketStream.ReadByte();
            //secound byte is compress mode
            var compresssMode = (CompressMode)socketStream.ReadByte();

            // server is called client method
            if (dataType == DataType.ResponseCallMethod)
            {
                var bytesArray = GoStreamReader.ReadBlockToEnd(socketStream, compresssMode, ProviderSetting.MaximumReceiveDataBlock, IsWebSocket);
                json = Encoding.UTF8.GetString(bytesArray);
                MethodCallInfo callInfo = JsonConvert.DeserializeObject<MethodCallInfo>(json);
                var data = JsonConvert.DeserializeObject<StreamInfo>(callInfo.Data.ToString());
                data.Stream = socketStream;
                return data;
            }
            return null;
        }

        internal override void RegisterFileStreamToUpload(StreamInfo streamInfo, MethodCallInfo Data)
        {
            if (IsDisposed || !_client.Connected)
                return;
            //connect to tcp
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var downloadFileSocket = new TcpClient();
            var waitable = downloadFileSocket.ConnectAsync(_address, _port);
            waitable.Wait();
#else
            var downloadFileSocket = new TcpClient(_address, _port);
#endif
            streamInfo.Stream = downloadFileSocket.GetStream();
            downloadFileSocket.Client.Send(Encoding.UTF8.GetBytes("SignalGo/1.0"));
            //get OK SignalGo/1.0
            int o = streamInfo.Stream.ReadByte();
            int k = streamInfo.Stream.ReadByte();

            //register client
            var json = JsonConvert.SerializeObject(new List<string>() { "/UploadFile" });
            List<byte> bytes = new List<byte>();
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            byte[] dataLen = BitConverter.GetBytes(jsonBytes.Length);
            bytes.AddRange(dataLen);
            bytes.AddRange(jsonBytes);
            GoStreamWriter.WriteToStream(downloadFileSocket.GetStream(), bytes.ToArray(), IsWebSocket);

            ///send method data
            json = JsonConvert.SerializeObject(Data);
            bytes = new List<byte>();
            bytes.Add((byte)DataType.RegisterFileUpload);
            bytes.Add((byte)CompressMode.None);
            jsonBytes = Encoding.UTF8.GetBytes(json);
            dataLen = BitConverter.GetBytes(jsonBytes.Length);
            bytes.AddRange(dataLen);
            bytes.AddRange(jsonBytes);
            if (bytes.Count > ProviderSetting.MaximumSendDataBlock)
                throw new Exception("SendData data length is upper than MaximumSendDataBlock");

            GoStreamWriter.WriteToStream(downloadFileSocket.GetStream(), bytes.ToArray(), IsWebSocket);
        }
    }
}
