using Newtonsoft.Json;
using SignalGo.Client.ClientManager;
using SignalGo.Shared.IO;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
#if (!PORTABLE)
using System.Net.Sockets;
#endif
using System.Text;

namespace SignalGo.Client
{
    public abstract class ConnectorStreamBase : ConnectorBase
    {
        internal override StreamInfo RegisterFileStreamToDownload(MethodCallInfo Data)
        {
#if (PORTABLE)
            if (IsDisposed || !_client.ReadStream.CanRead)
                return null;
#else
              if (IsDisposed || !_client.Connected)
                return null;
#endif
            //connect to tcp
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var downloadFileSocket = new TcpClient();
            var waitable = downloadFileSocket.ConnectAsync(_address, _port);
            waitable.Wait();
#elif (PORTABLE)
            var downloadFileSocket = new Sockets.Plugin.TcpSocketClient();
            downloadFileSocket.ConnectAsync(_address, _port).Wait();

#else
            var downloadFileSocket = new TcpClient(_address, _port);
#endif

#if (PORTABLE)
            var firstBytes = Encoding.UTF8.GetBytes("SignalGo/1.0");
            var wrtiteStream = downloadFileSocket.WriteStream;
            var readStream = downloadFileSocket.ReadStream;
            downloadFileSocket.WriteStream.Write(firstBytes, 0, firstBytes.Length);
#else
            var wrtiteStream = downloadFileSocket.GetStream();
            var readStream = downloadFileSocket.GetStream();
            downloadFileSocket.Client.Send(Encoding.UTF8.GetBytes("SignalGo/1.0"));
#endif
            //get OK SignalGo/1.0
            int o = readStream.ReadByte();
            int k = readStream.ReadByte();

            //register client
            var json = JsonConvert.SerializeObject(new List<string>() { "/DownloadFile" });
            List<byte> bytes = new List<byte>();
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            byte[] dataLen = BitConverter.GetBytes(jsonBytes.Length);
            bytes.AddRange(dataLen);
            bytes.AddRange(jsonBytes);
            StreamHelper.WriteToStream(wrtiteStream, bytes.ToArray());

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

            StreamHelper.WriteToStream(wrtiteStream, bytes.ToArray());

            //get OK SignalGo/1.0
            //int o = socketStream.ReadByte();
            //int k = socketStream.ReadByte();

            //get DataType
            var dataType = (DataType)readStream.ReadByte();
            //secound byte is compress mode
            var compresssMode = (CompressMode)readStream.ReadByte();

            // server is called client method
            if (dataType == DataType.ResponseCallMethod)
            {
                var bytesArray = StreamHelper.ReadBlockToEnd(readStream, compresssMode, ProviderSetting.MaximumReceiveDataBlock);
                json = Encoding.UTF8.GetString(bytesArray, 0, bytesArray.Length);
                MethodCallInfo callInfo = JsonConvert.DeserializeObject<MethodCallInfo>(json);
                var data = JsonConvert.DeserializeObject<StreamInfo>(callInfo.Data.ToString());
                data.Stream = readStream;
                return data;
            }
            return null;
        }

        internal override void RegisterFileStreamToUpload(StreamInfo streamInfo, MethodCallInfo Data)
        {
#if (PORTABLE)
            if (IsDisposed || !_client.ReadStream.CanRead)
                return ;
#else
              if (IsDisposed || !_client.Connected)
                return;
#endif
            //connect to tcp
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var downloadFileSocket = new TcpClient();
            var waitable = downloadFileSocket.ConnectAsync(_address, _port);
            waitable.Wait();
#elif (PORTABLE)
            var downloadFileSocket = new Sockets.Plugin.TcpSocketClient();
#else
            var downloadFileSocket = new TcpClient(_address, _port);
#endif
#if (PORTABLE)
            streamInfo.Stream = downloadFileSocket.WriteStream;
            var firstBytes = Encoding.UTF8.GetBytes("SignalGo/1.0");
            downloadFileSocket.WriteStream.Write(firstBytes, 0, firstBytes.Length);
#else
            streamInfo.Stream = downloadFileSocket.GetStream();
            downloadFileSocket.Client.Send(Encoding.UTF8.GetBytes("SignalGo/1.0"));
#endif
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

            StreamHelper.WriteToStream(downloadFileSocket.GetStream(), bytes.ToArray());
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

            StreamHelper.WriteToStream(downloadFileSocket.GetStream(), bytes.ToArray());
        }
    }
}
