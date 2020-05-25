using SignalGo.Server.ServiceManager;
using SignalGo.ServiceManager.Models;
using SignalGo.ServiceManager.Services;
using SignalGo.Shared;
using SignalGo.Shared.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SignalGo.ServiceManager.BaseViewModels.Core
{
    public static class StartUp
    {
        public static void Initialize()
        {
            try
            {
                AsyncActions.InitializeUIThread();
                Load();
                ServerProvider serverProvider = new ServerProvider();
                serverProvider.RegisterServerService<ServerManagerService>();
                serverProvider.RegisterServerService<ServerManagerStreamService>();
                serverProvider.ProviderSetting.HttpSetting.HandleCrossOriginAccess = true;
                Console.WriteLine($"Listening on port {UserSettingInfo.Current.UserSettings.ListeningPort}");
                serverProvider.Start($"http://{UserSettingInfo.Current.UserSettings.ListeningAddress}:{UserSettingInfo.Current.UserSettings.ListeningPort}/ServerManager/SignalGo");
                Debug.WriteLine("server is started");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static void Exit()
        {
            AutoLogger.Default.LogText("Manualy user closed the server manager.");
            foreach (var server in SettingInfo.Current.ServerInfo)
            {
                try
                {
                    if (server.CurrentServerBase != null)
                        server.CurrentServerBase.Dispose();
                }
                catch (Exception ex)
                {

                }
            }
            Process.GetCurrentProcess().Kill();
        }

        public static void Load()
        {
            try
            {
                foreach (ServerInfo server in SettingInfo.Current.ServerInfo)
                {
                    Console.WriteLine($"Your server key is : {server.ServerKey}");
                    server.Status = ServerInfoStatus.Stopped;
                    ServerInfoBaseViewModel.StartServer(server);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                AutoLogger.Default.LogError(ex, "Load");
            }
        }
    }
}
