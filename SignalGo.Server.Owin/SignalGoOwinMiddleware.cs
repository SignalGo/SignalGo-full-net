using Microsoft.Owin;
using SignalGo.Server.ServiceManager;
using SignalGo.Server.ServiceManager.Providers;
using SignalGo.Shared.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace SignalGo.Server.Owin
{
    public class SignalGoOwinMiddleware : OwinMiddleware
    {
        private ServerBase CurrentServerBase { get; set; }
        public SignalGoOwinMiddleware(OwinMiddleware owinMiddleware, ServerBase serverBase)
            : base(owinMiddleware)
        {
            CurrentServerBase = serverBase;
        }

        public override Task Invoke(IOwinContext context)
        {
            //context.Response.Headers.Add("Access-Control-Allow-Origin", "*".Split(','));
            //context.Response.Headers.Add("Access-Control-Allow-Credentials", "true".Split(','));
            //// Added "Accept-Encoding" to this list
            //context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, X-CSRF-Token, X-Requested-With, Accept, Accept-Version, Accept-Encoding, Content-Length, Content-MD5, Date, X-Api-Version, X-File-Name".Split(','));
            //context.Response.Headers.Add("Access-Control-Allow-Methods", "POST,GET,PUT,PATCH,DELETE,OPTIONS".Split(','));
            //// New Code Starts here
            //if (context.Request.Method == "OPTIONS")
            //{
            //    context.Response.StatusCode = (int)HttpStatusCode.OK;
            //    return Next.Invoke(context);
            //}

            string serviceName = context.Request.Uri.PathAndQuery.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            bool isWebSocketd = context.Request.Headers.ContainsKey("Sec-WebSocket-Key");
            if (!BaseProvider.ExistService(serviceName, CurrentServerBase) && !isWebSocketd)
                return Next.Invoke(context);

            OwinClientInfo owinClientInfo = new OwinClientInfo();
            owinClientInfo.ConnectedDateTime = DateTime.Now;
            owinClientInfo.IPAddress = context.Request.RemoteIpAddress;
            owinClientInfo.ClientId = Guid.NewGuid().ToString();
            CurrentServerBase.Clients.TryAdd(owinClientInfo.ClientId, owinClientInfo);

            owinClientInfo.OwinContext = context;
            owinClientInfo.RequestHeaders = context.Request.Headers;
            owinClientInfo.ResponseHeaders = context.Response.Headers;
            if (isWebSocketd)
            {
                owinClientInfo.StreamHelper = SignalGoStreamBase.CurrentBase;
                Action<IDictionary<string, object>, Func<IDictionary<string, object>, Task>> accept = context.Get<Action<IDictionary<string, object>, Func<IDictionary<string, object>, Task>>>("websocket.Accept");
                if (accept == null)
                {
                    // Bad Request
                    context.Response.StatusCode = 400;
                    context.Response.Write("Not a valid websocket request");
                    return Task.FromResult<object>(null);
                }
                WebsocketClient websocketClient = new WebsocketClient() { ClientInfo = owinClientInfo, CurrentServerBase = CurrentServerBase };
                accept(null, websocketClient.RunWebSocket);
                return Task.FromResult<object>(null);

            }
            else
            {
                owinClientInfo.StreamHelper = SignalGoStreamBase.CurrentBase;
                owinClientInfo.ClientStream = new PipeNetworkStream(new DuplexStream(context.Request.Body, context.Response.Body));
                return HttpProvider.AddHttpClient(owinClientInfo, CurrentServerBase, context.Request.Uri.PathAndQuery, context.Request.Method, null, null);
            }
        }
    }

    internal class WebsocketClient
    {
        public OwinClientInfo ClientInfo { get; set; }
        public ServerBase CurrentServerBase { get; set; }
        public Task RunWebSocket(IDictionary<string, object> websocketContext)
        {
            if (websocketContext.TryGetValue(typeof(WebSocketContext).FullName, out object value))
            {
                WebSocket webSocket = ((WebSocketContext)value).WebSocket;
                ClientInfo.ClientStream = new PipeNetworkStream(new WebsocketStream(webSocket));
                return HttpProvider.AddWebSocketHttpClient(ClientInfo, CurrentServerBase);
            }
            else
            {
                return null;
                //mWebSocket = new OwinWebSocket(websocketContext);
            }
        }
    }
}
