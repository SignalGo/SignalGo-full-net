using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
namespace SignalGo.Client.IO
{
    public class TimeoutWebClient : WebClient
    {
        TimeSpan _timeout;
        public TimeoutWebClient(TimeSpan? timeOut)
        {
            if (timeOut.HasValue)
                timeOut = TimeSpan.FromMinutes(1);
            _timeout = timeOut.Value;
        }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest w = base.GetWebRequest(uri);
            w.Timeout = (int)_timeout.TotalMilliseconds;
            return w;
        }
    }
}
