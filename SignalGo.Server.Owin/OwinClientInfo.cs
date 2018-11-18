using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
#if (!NETSTANDARD)
using Microsoft.Owin;
#endif
using SignalGo.Server.Models;

namespace SignalGo.Server.Owin
{
    public class OwinClientInfo : HttpClientInfo
    {
        public Action<int> ChangeStatusAction { get; set; }

        public override bool IsOwinClient
        {
            get
            {
                return true;
            }
        }
#if (!NETSTANDARD)
        public IOwinContext OwinContext { get; set; }
#endif
        public override IDictionary<string, string[]> ResponseHeaders { get; set; }
        public override IDictionary<string, string[]> RequestHeaders { get; set; }

        public override string GetRequestHeaderValue(string header)
        {
            if (!RequestHeaders.ContainsKey(header))
                return null;
            return RequestHeaders[header].FirstOrDefault();
        }

        public override void ChangeStatusCode(HttpStatusCode statusCode)
        {
            ChangeStatusAction?.Invoke((int)statusCode);
        }
    }
}
