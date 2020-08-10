using SignalGo.Publisher.Models;
using SignalGo.Server.ServiceManager;
using SignalGo.ServiceManager.Core.BaseViewModels;
using SignalGo.ServiceManager.Core.Models;
using SignalGo.ServiceManager.Core.Services;
using SignalGo.Shared;
using SignalGo.Shared.Log;
using System;
using System.Diagnostics;
using System.Linq;

namespace SignalGo.ServiceManager.BaseViewModels.Core
{
    public static class StartUp
    {
        /// <summary>
        /// init the server
        /// </summary>
        public static void Initialize()
        {
            try
            {
                AsyncActions.InitializeUIThread();
                Load();
                ServerProvider serverProvider = new ServerProvider();
                // register server services
                serverProvider.RegisterServerService<ServerManagerService>();
                serverProvider.RegisterServerService<ServerManagerStreamService>();
                serverProvider.RegisterServerService<FileManagerService>();
                serverProvider.AddAssemblyToSkipServiceReferences(typeof(IgnoreFileInfo).Assembly);
                //serverProvider.ProviderSetting.HttpSetting.HandleCrossOriginAccess = true;
                // show server manager information
                Console.WriteLine($"Listening {UserSettingInfo.Current.UserSettings.ListeningAddress} on port {UserSettingInfo.Current.UserSettings.ListeningPort}");
                // start server and listener's
                serverProvider.Start($"http://{UserSettingInfo.Current.UserSettings.ListeningAddress}:{UserSettingInfo.Current.UserSettings.ListeningPort}/ServerManager/SignalGo");
                // inform server start
                ;
                Console.WriteLine("server is started", Console.ForegroundColor = ConsoleColor.Cyan);
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                AutoLogger.Default.LogError(ex, "Startup Initialize");
            }
        }

        /// <summary>
        /// Quit Application and kill all services
        /// </summary>
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
                    AutoLogger.Default.LogError(ex, "Exit App");
                }
            }
            Process.GetCurrentProcess().Kill();
        }

        /// <summary>
        /// load all configured service's
        /// </summary>
        public static void Load()
        {
            try
            {
                foreach (var server in SettingInfo.Current.ServerInfo.Where(x => x.AutoStartEnabled))
                {
                    Console.WriteLine($"Your {server.Name} service key is : {server.ServerKey}", Console.ForegroundColor = ConsoleColor.Yellow);
                    Console.ResetColor();
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