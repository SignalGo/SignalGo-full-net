using SignalGo.ServiceManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.ServiceManager.Core.Helpers
{
    public static class ServerDetailsManager
    {
        public static Dictionary<ServerInfo, ServerDetailsInfo> ServerDetails { get; set; } = new Dictionary<ServerInfo, ServerDetailsInfo>();

        public static void AddServer(ServerInfo serverInfo)
        {
            serverInfo.Details = new ServerDetailsInfo();
            serverInfo.OnPropertyChanged(nameof(serverInfo.Details));
            ServerDetails.Add(serverInfo, serverInfo.Details);
        }

        public static void Enable(ServerInfo serverInfo)
        {
            var list = ServerDetails.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Value.IsEnabled = list[i].Key == serverInfo;
            }
        }

        public static bool GetDetails(ServerInfo serverInfo, out ServerDetailsInfo result)
        {
            return ServerDetails.TryGetValue(serverInfo, out result);
        }

        public static void StartEngine()
        {

            //_ = Task.Factory.StartNew(async () =>
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    //foreach (ServerInfo item in
                    //    ServerDetails.Keys.Where
                    //    (item => item.CurrentServerBase == null))
                    //{
                    //    ServerDetails.Remove(item);
                    //}

                    var serversDetail = ServerDetails.Where(x => x.Value.IsEnabled).ToList();
                    for (int i = 0; i < serversDetail.Count; i++)
                    {
                        serversDetail[i].Value.ServiceMemoryUsage = (serversDetail[i].Key.CurrentServerBase.BaseProcess.PrivateMemorySize64 / 1000000).ToString();
                    }
                    await Task.Delay(10000);
                    //StartEngine();
                }
            });
            //}, TaskCreationOptions.LongRunning);
        }
    }
}
