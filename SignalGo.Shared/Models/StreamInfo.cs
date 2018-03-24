using Newtonsoft.Json;
using SignalGo.Shared.IO;
using SignalGo.Shared.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
#if (!PORTABLE)
using System.Net.Sockets;
#endif
using System.Reflection;
using System.Text;

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
        Func<int> GetPositionFlush { get; set; }
    }

    public class BaseStreamInfo : IStreamInfo
    {
        /// <summary>
        /// get position of flush stream
        /// </summary>
        [JsonIgnore()]
        public Func<int> GetPositionFlush { get; set; }
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

        Stream _Stream;
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
#if (NETSTANDARD1_6)
                    var property = typeof(NetworkStream).GetTypeInfo().GetProperty("Socket", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static);
#else
                    var property = typeof(NetworkStream).GetProperty("Socket", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static);
#endif
                    var socket = (Socket)property.GetValue(((NetworkStream)Stream), null);
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
            var responseType = (DataType)GoStreamReader.ReadOneByte(stream, CompressMode.None, maximumReceiveStreamHeaderBlock, false);
            var compressMode = (CompressMode)GoStreamReader.ReadOneByte(stream, CompressMode.None, maximumReceiveStreamHeaderBlock, false);
            return new KeyValue<DataType, CompressMode>(responseType, compressMode);
        }

        /// <summary>
        /// set position of flush stream
        /// </summary>
        public void SetPositionFlush(int position)
        {
            DataType dataType = DataType.FlushStream;
            CompressMode compressMode = CompressMode.None;
            GoStreamWriter.WriteToStream(Stream, new byte[] { (byte)dataType, (byte)compressMode }, false);
            byte[] data = BitConverter.GetBytes(position);
            GoStreamWriter.WriteBlockToStream(Stream, data);
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

    public class StreamInfo : IStreamInfo
    {
        public Dictionary<string, object> Headers { get; set; } = new Dictionary<string, object>();
        [JsonIgnore()]
        public Stream Stream { get; set; }
        /// <summary>
        /// wrtie manually to stream
        /// </summary>
        [JsonIgnore()]
        public Action<Stream> WriteManually { get; set; }
        /// <summary>
        /// length of stream
        /// </summary>
        public long Length { get; set; }

        public string ClientId { get; set; }

        public Stream FlushStream
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public Func<int> GetPositionFlush
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public Action<int> SetPositionFlush
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// dispose the stream
        /// </summary>
        public void Dispose()
        {
#if (!PORTABLE)
            if (Stream is NetworkStream)
            {
                try
                {
#if (NETSTANDARD1_6)
                    var property = typeof(NetworkStream).GetTypeInfo().GetProperty("Socket", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static);
#else
                    var property = typeof(NetworkStream).GetProperty("Socket", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static);
#endif
                    var socket = (Socket)property.GetValue(((NetworkStream)Stream), null);
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
    }
}
