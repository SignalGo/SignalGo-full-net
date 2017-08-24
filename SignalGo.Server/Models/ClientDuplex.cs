using SignalGo.Server.ServiceManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Server.Models
{
    //public class ServerCalls: OprationCalls
    //{
    //    public ClientInfo CurrentClient { get; set; }

    //    public ServerBase ServerBase { get; set; }

    //    public virtual void OnInitialized()
    //    {

    //    }
    //}

    //public class CallbackContract : OprationCalls
    //{
    //    public ClientInfo CurrentClient { get; set; }

    //    public ServerBase ServerBase { get; set; }

    //    public virtual void OnInitialized()
    //    {

    //    }
    //}

    public interface OperationCalls
    {
        ClientInfo CurrentClient { get; set; }
        ServerBase ServerBase { get; set; }
    }

    //public class ClientDuplexCallback<T>
    //{
    //    public ServerBase ServerBase { get; set; }

    //    public T AllClients
    //    {
    //        get
    //        {
    //            return Clients(ServerBase.Clients);
    //        }
    //    }

    //    public T Client(string sessionId)
    //    {
    //        return Clients(from x in ServerBase.Clients where x.SessionId == sessionId);
    //    }

    //    public T Clients(List<string> sessionIds)
    //    {
    //        return Clients(from x in ServerBase.Clients where sessionIds.Contains(x.SessionId) select x);
    //    }

    //    public T ClientsWithout(List<string> sessionIds)
    //    {
    //        return Clients(from x in ServerBase.Clients where !sessionIds.Contains(x.SessionId) select x);
    //    }

    //    public T Clients(IEnumerable<ClientInfo> clients)
    //    {
    //        var attribName = ServerBase.GetServiceAttributeName(typeof(T));
    //        var callBack = ServerBase.Callbacks[attribName];
    //    }
    //}
}
