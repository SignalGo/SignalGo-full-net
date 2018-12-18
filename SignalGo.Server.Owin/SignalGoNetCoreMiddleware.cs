#if (NETSTANDARD)
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using SignalGo.Server.ServiceManager;
using SignalGo.Server.ServiceManager.Providers;
using SignalGo.Shared.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalGo.Server.Owin
{
    public class SignalGoNetCoreMiddleware
    {
        private ServerBase CurrentServerBase { get; set; }
        private readonly RequestDelegate _next;

        public SignalGoNetCoreMiddleware(ServerBase serverBase, RequestDelegate next)
        {
            CurrentServerBase = serverBase;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            //context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            //context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            //// Added "Accept-Encoding" to this list
            //context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, X-CSRF-Token, X-Requested-With, Accept, Accept-Version, Accept-Encoding, Content-Length, Content-MD5, Date, X-Api-Version, X-File-Name");
            //context.Response.Headers.Add("Access-Control-Allow-Methods", "POST,GET,PUT,PATCH,DELETE,OPTIONS");
            //// New Code Starts here
            //if (context.Request.Method == "OPTIONS")
            //{
            //    context.Response.StatusCode = (int)HttpStatusCode.OK;
            //    return _next.Invoke(context);
            //}

            string uri = context.Request.Path.Value + context.Request.QueryString.ToString();
            string serviceName = uri.Contains('/') ? uri.Substring(0, uri.LastIndexOf('/')).Trim('/') : "";
            bool isWebSocketd = context.Request.Headers.ContainsKey("Sec-WebSocket-Key");
            if (!BaseProvider.ExistService(serviceName, CurrentServerBase) && !isWebSocketd && !context.Request.Headers.ContainsKey("signalgo-servicedetail") && context.Request.Headers["content-type"] != "SignalGo Service Reference")
            {
                await _next.Invoke(context);
                return;
            }

            OwinClientInfo owinClientInfo = new OwinClientInfo(CurrentServerBase);
            owinClientInfo.ChangeStatusAction = (code) =>
            {
                context.Response.StatusCode = code;
            };

            owinClientInfo.ConnectedDateTime = DateTime.Now;
            owinClientInfo.IPAddress = context.Connection.RemoteIpAddress.ToString();
            owinClientInfo.ClientId = Guid.NewGuid().ToString();
            CurrentServerBase.Clients.TryAdd(owinClientInfo.ClientId, owinClientInfo);

            //owinClientInfo.OwinContext = context;
            owinClientInfo.RequestHeaders = new HttpHeaderCollection(context.Request.Headers);
            owinClientInfo.ResponseHeaders = new HttpHeaderCollection(context.Response.Headers);
            if (isWebSocketd)
            {
                owinClientInfo.StreamHelper = SignalGoStreamBase.CurrentBase;
                //Action<IDictionary<string, object>, Func<IDictionary<string, object>, Task>> accept = context.Get<Action<IDictionary<string, object>, Func<IDictionary<string, object>, Task>>>("websocket.Accept");
                //if (accept == null)
                //{
                //    // Bad Request
                //    context.Response.StatusCode = 400;
                //    context.Response.WriteAsync("Not a valid websocket request");
                //    return Task.FromResult<object>(null);
                //}
                //WebsocketClient websocketClient = new WebsocketClient() { ClientInfo = owinClientInfo, CurrentServerBase = CurrentServerBase };
                //accept(null, websocketClient.RunWebSocket);
                await Task.FromResult<object>(null);

            }
            else
            {
                owinClientInfo.StreamHelper = SignalGoStreamBase.CurrentBase;
                owinClientInfo.ClientStream = new PipeNetworkStream(new DuplexStream(context.Request.Body, context.Response.Body));
                await HttpProvider.AddHttpClient(owinClientInfo, CurrentServerBase, uri, context.Request.Method, null, null);
            }
        }


    }

    public class HttpHeaderCollection : IDictionary<string, string[]>
    {
        private IHeaderDictionary _headerDictionary;
        public HttpHeaderCollection(IHeaderDictionary headerDictionary)
        {
            _headerDictionary = headerDictionary;
        }

        public string[] this[string key]
        {
            get
            {
                return _headerDictionary[key];
            }
            set
            {
                _headerDictionary[key] = value;
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                return _headerDictionary.Keys;
            }
        }

        public ICollection<string[]> Values
        {
            get
            {
                return _headerDictionary.Values.Select(x => (string[])x).ToList();
            }
        }

        public int Count
        {
            get
            {
                return _headerDictionary.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return _headerDictionary.IsReadOnly;
            }
        }

        public void Add(string key, string[] value)
        {
            _headerDictionary.Add(key, value);
        }

        public void Add(KeyValuePair<string, string[]> item)
        {
            _headerDictionary.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _headerDictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, string[]> item)
        {
            return _headerDictionary.Contains(new KeyValuePair<string, StringValues>(item.Key, item.Value));
        }

        public bool ContainsKey(string key)
        {
            return _headerDictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, string[]>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator()
        {
            return _headerDictionary.Select(x => new KeyValuePair<string, string[]>(x.Key, x.Value)).GetEnumerator();
        }

        public bool Remove(string key)
        {
            return _headerDictionary.Remove(key);
        }

        public bool Remove(KeyValuePair<string, string[]> item)
        {
            return _headerDictionary.Remove(new KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>(item.Key, item.Value));
        }

        public bool TryGetValue(string key, out string[] value)
        {
            bool result = _headerDictionary.TryGetValue(key, out StringValues data);
            value = data;
            return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _headerDictionary.GetEnumerator();
        }
    }
}
#endif