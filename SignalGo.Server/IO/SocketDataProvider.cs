using SignalGo.Shared.IO;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SignalGo.Server.IO
{
    public class SocketDataProvider
    {
        public SocketDataProvider(Socket socket)
        {
            var reader = new CustomStreamReader(socket);
            var headerResponse = reader.ReadLine();
            if (headerResponse.Contains("SignalGo-Stream/2.0"))
            {

            }

        }


        void ReadLine(Socket _connecter, int size = 4)
        {
            var buffer = new byte[size];
            var recieveArgs = new SocketAsyncEventArgs()
            {
                UserToken = Guid.NewGuid()
            };
            recieveArgs.SetBuffer(buffer, 0, size);
            recieveArgs.Completed += recieveArgs_Completed;
            _connecter.ReceiveAsync(recieveArgs);
            return buffer;
        }
    }
}
