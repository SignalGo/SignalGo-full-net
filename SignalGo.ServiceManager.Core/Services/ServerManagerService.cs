using SignalGo.Server.Models;
using SignalGo.ServiceManager.Core.ClientServices;
using SignalGo.ServiceManager.Core.Models;
using SignalGo.Shared.DataTypes;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SignalGo.ServiceManager.Core.Services
{
    [ServiceContract("ServerManager", ServiceType.ServerService, InstanceType.SingleInstance)]
    public class ServerManagerService
    {

        //public bool StopServer(Guid serverKey, string name)
        //{
        //    if (serverKey != SettingInfo.Current.ServerKey)
        //        return false;
        //    var find = SettingInfo.Current.ServerInfo.FirstOrDefault(x => x.Name == name);
        //    if (find == null)
        //        return false;
        //    find.Stop();
        //    return true;
        //}
        public bool StopServer(Guid serverKey)
        {
            // Current.ServerKey not set yet!
            //if (serverKey != SettingInfo.Current.ServerKey)
            //    return false;
            var find = SettingInfo.Current.ServerInfo.FirstOrDefault(x => x.ServerKey == serverKey);
            if (find == null)
                return false;
            find.Stop();
            return true;
        }
        //public bool StartServer(Guid serverKey, string name)
        //{
        //    if (serverKey != SettingInfo.Current.ServerKey)
        //        return false;
        //    var find = SettingInfo.Current.ServerInfo.FirstOrDefault(x => x.Name == name);
        //    if (find == null)
        //        return false;
        //    find.Start();
        //    return true;
        //}
        public bool StartServer(Guid serverKey)
        {
            //if (serverKey != SettingInfo.Current.ServerKey)
            //    return false;
            var find = SettingInfo.Current.ServerInfo.FirstOrDefault(x => x.ServerKey == serverKey);
            if (find == null)
                return false;
            find.Start();
            return true;
        }
        //public bool RestartServer(Guid serverKey, string name, bool force = false)
        //{
        //    // find server
        //    if (serverKey != SettingInfo.Current.ServerKey)
        //        return false;
        //    var find = SettingInfo.Current.ServerInfo.FirstOrDefault(x => x.Name == name);
        //    if (find == null)
        //        return false;
        //    // stop 
        //    find.Stop();

        //    // start 
        //    find.Start();

        //    return true;

        //}
        //public bool RestartServer(Guid serverKey, bool force = false)
        //{
        //    // find server
        //    if (serverKey != SettingInfo.Current.ServerKey)
        //        return false;
        //    var find = SettingInfo.Current.ServerInfo.FirstOrDefault(x => x.ServerKey == serverKey);
        //    if (find == null)
        //        return false;
        //    // stop 
        //    find.Stop();

        //    // start 
        //    find.Start();

        //    return true;

        //}

        public async Task<string> CallClientService(string message)
        {
            // call clients methods
            foreach (ClientContext<IServerManagerCallbackClientService> item in OperationContext.Current.GetAllClientClientContextServices<IServerManagerCallbackClientService>())
            {
                if (item.Client.ProtocolType == ClientProtocolType.WebSocket || item.Client.ProtocolType == ClientProtocolType.SignalGoDuplex)
                {
                    await item.Service.ReceivedMessageAsync(message);
                    await item.Service.ReceivedMessageBaseAsync(message);
                }
            }
            return message;
        }

        /// <summary>
        /// test hello
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string SayHello(string name = "")
        {
            return $"Hello Dear {name}";
        }
    }
}
