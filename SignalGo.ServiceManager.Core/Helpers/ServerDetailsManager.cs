using System;
using System.Linq;
using SignalGo.Shared.Log;
using System.Threading.Tasks;
using System.Collections.Generic;
using SignalGo.ServiceManager.Core.Models;

namespace SignalGo.ServiceManager.Core.Helpers
{
    public static class ServerDetailsManager
    {
        /// <summary>
        /// service's detail used in monitoring engine
        /// </summary>
        private static Dictionary<ServerInfo, ServerDetailsInfo> ServerDetails { get; set; } = new Dictionary<ServerInfo, ServerDetailsInfo>();

        /// <summary>
        /// add an service to server monitoring dic 
        /// </summary>
        /// <param name="serverInfo"></param>
        public static void AddServer(ServerInfo serverInfo)
        {
            serverInfo.Details = new ServerDetailsInfo();
            serverInfo.OnPropertyChanged(nameof(serverInfo.Details));
            ServerDetails.Add(serverInfo, serverInfo.Details);
        }
        /// <summary>
        /// enable monitoring for an service
        /// </summary>
        /// <param name="serverInfo"></param>
        public static void Enable(ServerInfo serverInfo)
        {
            try
            {
                //AddServer(serverInfo);
                var list = ServerDetails.ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].Value.IsEnabled = list[i].Key == serverInfo;
                }
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "ServerDetailsManager AddServer");
            }
        }

        public static void StopEngine(ServerInfo serverInfo, bool forceAll = false)
        {
            if (forceAll)
            {
                ServerDetails.Clear();
            }
            else
            {
                ServerDetails.Remove(serverInfo);
            }
            AutoLogger.Default.LogText($"Server {serverInfo.Name}, Monitoring Engine Stopped");
        }
        public static async void StartEngine()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    var serversDetail = ServerDetails.Where(x => x.Value.IsEnabled).ToList();
                    for (int i = 0; i < serversDetail.Count; i++)
                    {
                        try
                        {
                            serversDetail[i].Value.ServiceMemoryUsage = (serversDetail[i].Key.CurrentServerBase.BaseProcess.PrivateMemorySize64 / 1000000).ToString();
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    await Task.Delay(20000);
                }
            });
        }
    }
}
