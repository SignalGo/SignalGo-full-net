using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace SignalGo.Server.Helpers
{
    public static class SslTcpManager
    {
        public static async Task<Stream> GetStream(TcpClient client, X509Certificate x509Certificate)
        {
            // A client has connected. Create the 
            // SslStream using the client's network stream.
            SslStream sslStream = new SslStream(
                client.GetStream(), false);
            // Authenticate the server but don't require the client to authenticate.
            await sslStream.AuthenticateAsServerAsync(x509Certificate,
                 false, SslProtocols.Tls, true);

            return sslStream;
        }
    }
}
