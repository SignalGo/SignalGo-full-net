using Newtonsoft.Json;
using SignalGo.Shared.IO;
using System;
using System.IO;
using System.Net;
#if (!PORTABLE)
#endif
using System.Threading.Tasks;

namespace SignalGo.Shared.Models
{
    /// <summary>
    /// interface of stream
    /// </summary>
    public interface IStreamInfo : IDisposable
    {
        /// <summary>
        /// client id 
        /// </summary>
        string ClientId { get; set; }
        /// <summary>
        /// stream
        /// </summary>
        PipeNetworkStream Stream { get; set; }
        /// <summary>
        /// length of stream
        /// </summary>
        long Length { get; set; }
        /// <summary>
        /// wrtie manually to stream
        /// </summary>
        Action<PipeNetworkStream> WriteManually { get; set; }
        Func<PipeNetworkStream, Task> WriteManuallyAsync { get; set; }
        /// <summary>
        /// get position of flush stream
        /// </summary>
#if (NET35 || NET40)
        Func<long> GetPositionFlush { get; set; }
        KeyValue<DataType, CompressMode> ReadFirstData(PipeNetworkStream stream, int maximumReceiveStreamHeaderBlock);
#else
        Func<Task<long>> GetPositionFlush { get; set; }
        Task<KeyValue<DataType, CompressMode>> ReadFirstDataAsync(PipeNetworkStream stream, int maximumReceiveStreamHeaderBlock);
#endif
    }

    public class BaseStreamInfo : IStreamInfo
    {
        /// <summary>
        /// get position of flush stream
        /// </summary>
        [JsonIgnore()]
#if (NET35 || NET40)
        public Func<long> GetPositionFlush { get; set; }
#else
        public Func<Task<long>> GetPositionFlush { get; set; }
#endif
        /// <summary>
        /// status of request
        /// </summary>
        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;
        /// <summary>
        /// this action use client side for send one byte when client is ready to download
        /// </summary>
        [JsonIgnore()]
        internal Action GetStreamAction { get; set; }
        /// <summary>
        /// wrtie manually to stream
        /// </summary>
        [JsonIgnore()]
        public Action<PipeNetworkStream> WriteManually { get; set; }
        [JsonIgnore()]
        public Func<PipeNetworkStream, Task> WriteManuallyAsync { get; set; }

        /// <summary>
        /// client id 
        /// </summary>
        public string ClientId { get; set; }

        private PipeNetworkStream _Stream;
        /// <summary>
        /// stream for read and write
        /// </summary>
        [JsonIgnore()]
        public PipeNetworkStream Stream
        {
            get
            {
                GetStreamAction?.Invoke();
                return _Stream;
            }
            set
            {
                _Stream = value;
            }
        }

        /// <summary>
        /// length of stream
        /// </summary>
        public long Length { get; set; }
        /// <summary>
        /// content type of stream
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// close the connection
        /// </summary>
        public void Dispose()
        {
            GetStreamAction = null;
            //#if (!PORTABLE)
            //            if (Stream is NetworkStream)
            //            {
            //                try
            //                {
            //#if (NETSTANDARD)
            //                    var property = typeof(NetworkStream).GetTypeInfo().GetProperty("Socket", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static);
            //#else
            //                    PropertyInfo property = typeof(NetworkStream).GetProperty("Socket", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static);
            //#endif
            //                    Socket socket = (Socket)property.GetValue(((NetworkStream)Stream), null);
            //#if (NET35)
            //                    socket.Close();
            //#else
            //                    socket.Dispose();
            //#endif
            //                }
            //                catch
            //                {
            //                    //AutoLogger.LogError(ex, "StreamInfo Dispose");
            //                }
            //            }
            //#endif
            Stream.Dispose();
        }

#if (NET35 || NET40)
        public KeyValue<DataType, CompressMode> ReadFirstData(PipeNetworkStream stream, int maximumReceiveStreamHeaderBlock)
        {
            DataType responseType = (DataType)SignalGoStreamBase.CurrentBase.ReadOneByte(stream);
            CompressMode compressMode = (CompressMode)SignalGoStreamBase.CurrentBase.ReadOneByte(stream);
            return new KeyValue<DataType, CompressMode>(responseType, compressMode);
        }
#else
        public async Task<KeyValue<DataType, CompressMode>> ReadFirstDataAsync(PipeNetworkStream stream, int maximumReceiveStreamHeaderBlock)
        {
            DataType responseType = (DataType)await SignalGoStreamBase.CurrentBase.ReadOneByteAsync(stream);
            CompressMode compressMode = (CompressMode)await SignalGoStreamBase.CurrentBase.ReadOneByteAsync(stream);
            return new KeyValue<DataType, CompressMode>(responseType, compressMode);
        }
#endif
        /// <summary>
        /// set position of flush stream
        /// </summary>
#if (NET35 || NET40)
        public void SetPositionFlush(long position)
        {
            DataType dataType = DataType.FlushStream;
            CompressMode compressMode = CompressMode.None;
            SignalGoStreamBase.CurrentBase.WriteToStream(Stream, new byte[] { (byte)dataType, (byte)compressMode });
            byte[] data = BitConverter.GetBytes(position);
            SignalGoStreamBase.CurrentBase.WriteBlockToStream(Stream, data);
        }
#else
        public async Task SetPositionFlushAsync(long position)
        {
            DataType dataType = DataType.FlushStream;
            CompressMode compressMode = CompressMode.None;
            await SignalGoStreamBase.CurrentBase.WriteToStreamAsync(Stream, new byte[] { (byte)dataType, (byte)compressMode });
            byte[] data = BitConverter.GetBytes(position);
            await SignalGoStreamBase.CurrentBase.WriteBlockToStreamAsync(Stream, data);
        }
#endif

    }

    /// <summary>
    /// stream for upload and download
    /// </summary>
    /// <typeparam name="T">data of stream</typeparam>
    public class StreamInfo<T> : BaseStreamInfo
    {
        public StreamInfo()
        {

        }

        public StreamInfo(PipeNetworkStream stream)
        {
            Stream = stream;
        }

        /// <summary>
        /// data of stream
        /// </summary>
        public T Data { get; set; }
    }

    public class StreamInfo : BaseStreamInfo
    {
        public StreamInfo()
        {

        }

        public StreamInfo(PipeNetworkStream stream)
        {
            Stream = stream;
        }
    }
}
