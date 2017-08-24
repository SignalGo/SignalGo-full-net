using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using SignalGo.Shared.Helpers;
using System;

namespace SignalGo.Server.ServiceManager
{

    public class ServerProvider : UdpServiceBase
    {
        static ServerProvider()
        {
            JsonSettingHelper.Initialize();
        }

        public void Start(string url)
        {
            Uri uri = null;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                throw new Exception("url is not valid");
            }
            else if (uri.Port <= 0)
            {
                throw new Exception("port is not valid");
            }

            //IPHostEntry Host = Dns.GetHostEntry(uri.Host);
            //IPHostEntry server = Dns.Resolve(uri.Host);
            Connect(uri.Port, new string[] { uri.AbsolutePath });
        }

        //public void StartWebSocket(string url)
        //{
        //    Uri uri = null;
        //    if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
        //    {
        //        throw new Exception("url is not valid");
        //    }
        //    else if (uri.Port <= 0)
        //    {
        //        throw new Exception("port is not valid");
        //    }

        //    //IPHostEntry Host = Dns.GetHostEntry(uri.Host);
        //    //IPHostEntry server = Dns.Resolve(uri.Host);
        //    ConnectWebSocket(uri.Port, new string[] { uri.AbsolutePath });
        //}
    }
}
