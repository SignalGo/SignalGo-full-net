using SignalGo.Shared.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace SignalGo.Shared.Models
{
    public class StreamInfo : IDisposable
    {
        public Dictionary<string, object> Headers { get; set; } = new Dictionary<string, object>();
        public Stream Stream { get; set; }

        public void Dispose()
        {
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
            Stream.Dispose();
        }
    }
}
