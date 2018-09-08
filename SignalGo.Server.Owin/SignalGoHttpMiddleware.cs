using Microsoft.Owin;
using SignalGo.Server.ServiceManager;
using SignalGo.Server.ServiceManager.Providers;
using SignalGo.Shared.IO;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SignalGo.Server.Owin
{
    public class SignalGoHttpMiddleware : OwinMiddleware
    {
        private ServerBase CurrentServerBase { get; set; }
        public SignalGoHttpMiddleware(OwinMiddleware owinMiddleware, ServerBase serverBase)
            : base(owinMiddleware)
        {
            CurrentServerBase = serverBase;
        }

        public override Task Invoke(IOwinContext context)
        {
            string serviceName = context.Request.Uri.PathAndQuery.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (!BaseProvider.ExistService(serviceName, CurrentServerBase))
                return Next.Invoke(context);

            OwinClientInfo owinClientInfo = new OwinClientInfo();
            owinClientInfo.ConnectedDateTime = DateTime.Now;
            owinClientInfo.IPAddress = context.Request.RemoteIpAddress;
            owinClientInfo.ClientId = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString();
            CurrentServerBase.Clients.TryAdd(owinClientInfo.ClientId, owinClientInfo);

            owinClientInfo.ClientStream = new PipeNetworkStream(new DuplexStream(context.Request.Body, context.Response.Body));
            owinClientInfo.OwinContext = context;
            owinClientInfo.RequestHeaders = context.Request.Headers;
            owinClientInfo.ResponseHeaders = context.Response.Headers;
            owinClientInfo.StreamHelper = SignalGoStreamBase.CurrentBase;

            return HttpProvider.AddHttpClient(owinClientInfo, CurrentServerBase, context.Request.Uri.PathAndQuery, context.Request.Method, null, null);
        }
    }
}
