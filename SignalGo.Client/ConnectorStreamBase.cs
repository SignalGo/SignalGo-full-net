using Newtonsoft.Json;
using SignalGo.Client.ClientManager;
using SignalGo.Shared.IO;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
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
            throw new NotSupportedException();
//#if (PORTABLE)
//            if (IsDisposed || !_client.ReadStream.CanRead)
//                return null;
//#else
//            if (IsDisposed || !_client.Connected)
//                return null;
//#endif
//            //connect to tcp
//#if (NETSTANDARD1_6 || NETCOREAPP1_1)
//            var downloadFileSocket = new TcpClient();
//            var waitable = downloadFileSocket.ConnectAsync(_address, _port);
//            waitable.Wait();
//#elif (PORTABLE)
//            var downloadFileSocket = new Sockets.Plugin.TcpSocketClient();
//            downloadFileSocket.ConnectAsync(_address, _port).Wait();

//#else
//            TcpClient downloadFileSocket = new TcpClient(_address, _port);
//#endif

//            var stream = new PipeNetworkStream(new NormalStream(downloadFileSocket.GetStream()));
//            downloadFileSocket.Client.Send(Encoding.UTF8.GetBytes("SignalGo/1.0"));

//            //get OK SignalGo/1.0
//            int o = stream.ReadOneByte();
//            int k = stream.ReadOneByte();

//            //register client
//            string json = JsonConvert.SerializeObject(new List<string>() { "/DownloadFile" });
//            List<byte> bytes = new List<byte>();
//            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
//            byte[] dataLen = BitConverter.GetBytes(jsonBytes.Length);
//            bytes.AddRange(dataLen);
//            bytes.AddRange(jsonBytes);
//            StreamHelper.WriteToStream(stream, bytes.ToArray());

//            ///send method data
//            json = JsonConvert.SerializeObject(Data);
//            bytes = new List<byte>();
//            bytes.Add((byte)DataType.RegisterFileDownload);
//            bytes.Add((byte)CompressMode.None);
//            jsonBytes = Encoding.UTF8.GetBytes(json);
//            dataLen = BitConverter.GetBytes(jsonBytes.Length);
//            bytes.AddRange(dataLen);
//            bytes.AddRange(jsonBytes);
//            if (bytes.Count > ProviderSetting.MaximumSendDataBlock)
//                throw new Exception("SendData data length is upper than MaximumSendDataBlock");

//            StreamHelper.WriteToStream(stream, bytes.ToArray());

//            //get OK SignalGo/1.0
//            //int o = socketStream.ReadByte();
//            //int k = socketStream.ReadByte();

//            //get DataType
//            DataType dataType = (DataType)stream.ReadOneByte();
//            //secound byte is compress mode
//            CompressMode compresssMode = (CompressMode)stream.ReadOneByte();

//            // server is called client method
//            if (dataType == DataType.ResponseCallMethod)
//            {
//                byte[] bytesArray = StreamHelper.ReadBlockToEnd(stream, compresssMode, ProviderSetting.MaximumReceiveDataBlock);
//                json = Encoding.UTF8.GetString(bytesArray, 0, bytesArray.Length);
//                MethodCallInfo callInfo = JsonConvert.DeserializeObject<MethodCallInfo>(json);
//                StreamInfo data = JsonConvert.DeserializeObject<StreamInfo>(callInfo.Data.ToString());
//                data.Stream = stream;
//                return data;
//            }
//            return null;
        }

        internal override void RegisterFileStreamToUpload(StreamInfo streamInfo, MethodCallInfo Data)
        {
            throw new NotSupportedException();

//#if (PORTABLE)
//            if (IsDisposed || !_client.ReadStream.CanRead)
//                return ;
//#else
//            if (IsDisposed || !_client.Connected)
//                return;
//#endif
//            //connect to tcp
//#if (NETSTANDARD1_6 || NETCOREAPP1_1)
//            var downloadFileSocket = new TcpClient();
//            var waitable = downloadFileSocket.ConnectAsync(_address, _port);
//            waitable.Wait();
//#elif (PORTABLE)
//            var downloadFileSocket = new Sockets.Plugin.TcpSocketClient();
//#else
//            TcpClient downloadFileSocket = new TcpClient(_address, _port);
//#endif
//            var stream = new PipeNetworkStream(new NormalStream(downloadFileSocket.GetStream()));
//            streamInfo.Stream = stream;
//            downloadFileSocket.Client.Send(Encoding.UTF8.GetBytes("SignalGo/1.0"));
//            //get OK SignalGo/1.0
//            int o =await stream.ReadOneByteAcync();
//            int k = await stream.ReadOneByteAcync();

//            //register client
//            string json = JsonConvert.SerializeObject(new List<string>() { "/UploadFile" });
//            List<byte> bytes = new List<byte>();
//            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
//            byte[] dataLen = BitConverter.GetBytes(jsonBytes.Length);
//            bytes.AddRange(dataLen);
//            bytes.AddRange(jsonBytes);

//            await StreamHelper.WriteToStreamAsync(stream, bytes.ToArray());
//            ///send method data
//            json = JsonConvert.SerializeObject(Data);
//            bytes = new List<byte>();
//            bytes.Add((byte)DataType.RegisterFileUpload);
//            bytes.Add((byte)CompressMode.None);
//            jsonBytes = Encoding.UTF8.GetBytes(json);
//            dataLen = BitConverter.GetBytes(jsonBytes.Length);
//            bytes.AddRange(dataLen);
//            bytes.AddRange(jsonBytes);
//            if (bytes.Count > ProviderSetting.MaximumSendDataBlock)
//                throw new Exception("SendData data length is upper than MaximumSendDataBlock");

//            await StreamHelper.WriteToStreamAsync(stream, bytes.ToArray());
        }
    }
}
