using Newtonsoft.Json;
using SignalGo.Shared.IO;
using System;
using System.IO;
using System.Net;
#if (!PORTABLE)
using System.Net.Sockets;
#endif
using System.Reflection;

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
        Stream Stream { get; set; }
        /// <summary>
        /// length of stream
        /// </summary>
        long Length { get; set; }
        /// <summary>
        /// wrtie manually to stream
        /// </summary>
        Action<Stream> WriteManually { get; set; }
        /// <summary>
        /// get position of flush stream
        /// </summary>
        Func<long> GetPositionFlush { get; set; }
        KeyValue<DataType, CompressMode> ReadFirstData(Stream stream, uint maximumReceiveStreamHeaderBlock);
    }

    public class BaseStreamInfo : IStreamInfo
    {
        /// <summary>
        /// get position of flush stream
        /// </summary>
        [JsonIgnore()]
        public Func<long> GetPositionFlush { get; set; }
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
        public Action<Stream> WriteManually { get; set; }

        /// <summary>
        /// client id 
        /// </summary>
        public string ClientId { get; set; }

        private Stream _Stream;
        /// <summary>
        /// stream for read and write
        /// </summary>
        [JsonIgnore()]
        public Stream Stream
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
        /// close the connection
        /// </summary>
        public void Dispose()
        {
            GetStreamAction = null;
#if (!PORTABLE)
            if (Stream is NetworkStream)
            {
                try
                {
#if (NETSTANDARD)
                    var property = typeof(NetworkStream).GetTypeInfo().GetProperty("Socket", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static);
#else
                    PropertyInfo property = typeof(NetworkStream).GetProperty("Socket", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static);
#endif
                    Socket socket = (Socket)property.GetValue(((NetworkStream)Stream), null);
#if (NET35)
                    socket.Close();
#else
                    socket.Dispose();
#endif
                }
                catch
                {
                    //AutoLogger.LogError(ex, "StreamInfo Dispose");
                }
            }
#endif
            Stream.Dispose();
        }

        public KeyValue<DataType, CompressMode> ReadFirstData(Stream stream, uint maximumReceiveStreamHeaderBlock)
        {
            DataType responseType = (DataType)SignalGoStreamBase.CurrentBase.ReadOneByte(stream);
            CompressMode compressMode = (CompressMode)SignalGoStreamBase.CurrentBase.ReadOneByte(stream);
            return new KeyValue<DataType, CompressMode>(responseType, compressMode);
        }

        /// <summary>
        /// set position of flush stream
        /// </summary>
        public void SetPositionFlush(long position)
        {
            DataType dataType = DataType.FlushStream;
            CompressMode compressMode = CompressMode.None;
            SignalGoStreamBase.CurrentBase.WriteToStream(Stream, new byte[] { (byte)dataType, (byte)compressMode });
            byte[] data = BitConverter.GetBytes(position);
            SignalGoStreamBase.CurrentBase.WriteBlockToStream(Stream, data);
        }
    }

    /// <summary>
    /// stream for upload and download
    /// </summary>
    /// <typeparam name="T">data of stream</typeparam>
    public class StreamInfo<T> : BaseStreamInfo
    {
        /// <summary>
        /// data of stream
        /// </summary>
        public T Data { get; set; }
    }

    public class StreamInfo : BaseStreamInfo
    {

    }
}
