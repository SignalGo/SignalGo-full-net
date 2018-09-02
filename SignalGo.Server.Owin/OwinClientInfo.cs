using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using SignalGo.Server.Models;

namespace SignalGo.Server.Owin
{
    public class OwinClientInfo : HttpClientInfo
    {
        public override bool IsOwinClient
        {
            get
            {
                return true;
            }
        }

        public IOwinContext OwinContext { get; set; }

        public override IDictionary<string, string[]> ResponseHeaders { get; set; }
        public override IDictionary<string, string[]> RequestHeaders { get; set; }

        public override string GetRequestHeaderValue(string header)
        {
            if (!RequestHeaders.ContainsKey(header))
                return null;
            return ResponseHeaders[header].FirstOrDefault();
        }
    }
}
