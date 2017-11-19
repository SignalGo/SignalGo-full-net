using Newtonsoft.Json;
using SignalGo.Shared.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if (!PORTABLE)
using System.Net.Sockets;
#endif
using System.Reflection;
using System.Text;

namespace SignalGo.Shared.Models
{
    /// <summary>
    /// stream for upload and download
    /// </summary>
    /// <typeparam name="T">data of stream</typeparam>
    public class StreamInfo<T> : IDisposable
    {
        /// <summary>
        /// data of stream
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// this action use client side for send one byte when client is ready to download
        /// </summary>
        [JsonIgnore()]
        internal Action GetStreamAction { get; set; }

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
                catch (Exception ex)
                {
                    AutoLogger.LogError(ex, "StreamInfo Dispose");
                }
            }
#endif
            Stream.Dispose();
        }
    }

    public class StreamInfo : IDisposable
    {
        public Dictionary<string, object> Headers { get; set; } = new Dictionary<string, object>();
        [JsonIgnore()]
        public Stream Stream { get; set; }
        /// <summary>
        /// length of stream
        /// </summary>
        public long Length { get; set; }

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
                catch (Exception ex)
                {
                    AutoLogger.LogError(ex, "StreamInfo Dispose");
                }
            }
#endif
            Stream.Dispose();
        }
    }
}
