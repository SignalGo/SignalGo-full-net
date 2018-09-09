using SignalGo.ServerManager.Models;
using SignalGo.Shared.DataTypes;
using System;
using System.Linq;

namespace SignalGo.ServerManager.Services
{
    [ServiceContract("ServerManager", ServiceType.OneWayService, InstanceType.SingleInstance)]
    public class ServerManagerService
    {
        public bool StopServer(Guid serverKey, string name)
        {
            if (serverKey != SettingInfo.Current.ServerKey)
                return false;
            var find =  SettingInfo.Current.ServerInfoes.FirstOrDefault(x => x.Name == name);
            if (find == null)
                return false;
            find.Stop();
            return true;
        }

        public bool StartServer(Guid serverKey, string name)
        {
            if (serverKey != SettingInfo.Current.ServerKey)
                return false;
            var find = SettingInfo.Current.ServerInfoes.FirstOrDefault(x => x.Name == name);
            if (find == null)
                return false;
            find.Start();
            return true;
        }
    }
}
