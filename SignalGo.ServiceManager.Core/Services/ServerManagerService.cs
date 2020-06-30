using SignalGo.Server.Models;
using SignalGo.ServiceManager.Core.ClientServices;
using SignalGo.ServiceManager.Core.Models;
using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Log;
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
        public bool StopService(Guid serviceKey)
        {
            // Current.ServerKey not set yet!
            //if (serverKey != SettingInfo.Current.ServerKey)
            //    return false;
            var find = SettingInfo.Current.ServerInfo.FirstOrDefault(x => x.ServerKey == serviceKey);
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
        public bool StartService(Guid serviceKey)
        {
            //if (serverKey != SettingInfo.Current.ServerKey)
            //    return false;
            var find = SettingInfo.Current.ServerInfo.FirstOrDefault(x => x.ServerKey == serviceKey);
            if (find == null)
                return false;
            find.Start();
            return true;
        }

        /// <summary>
        /// restart a service in server
        /// </summary>
        /// <param name="serverKey">key of service</param>
        /// <param name="force">force restart</param>
        /// <returns></returns>
        public bool RestartService(Guid serviceKey, bool force = false)
        {
            // find server
            try
            {
                ServerInfo find = SettingInfo.Current.ServerInfo.FirstOrDefault(x => x.ServerKey == serviceKey);
                if (find == null)
                    return false;
                // stop 
                find.Stop();

                // start 
                find.Start();
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "Restart Server Service");
            }

            return true;

        }

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
    }
}
