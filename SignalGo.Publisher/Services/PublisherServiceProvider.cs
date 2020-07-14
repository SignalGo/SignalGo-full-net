using System;
using SignalGo.Client;
using SignalGo.Shared;
using System.Diagnostics;
using SignalGo.Shared.Log;
using System.Threading.Tasks;
using SignalGo.Publisher.Models;
using System.Collections.Generic;
using SignalGo.Publisher.Extensions;
using SignalGo.Publisher.Models.Extra;
using ServerManagerService.Interfaces;

namespace SignalGo.Publisher.Services
{
    /// <summary>
    /// init instance of publisher client that connect to signalGo server manager service
    /// simply check the connection state and reliability
    /// </summary>
    public class PublisherServiceProvider
    {
        //public static Dictionary<string, PublisherServiceProvider> Providers { get; set; } = new Dictionary<string, PublisherServiceProvider>();

        /// <summary>
        /// instance of server manager service
        /// </summary>
        public IServerManagerService ServerManagerService { get; set; }
        public IFileManagerService FileManagerService { get; set; }

        /// <summary>
        /// instance of client(publisher)
        /// </summary>
        public ClientProvider CurrentClientProvider { get; set; }
        //public string RemoteServer { get; set; }

        /// <summary>
        /// Connect Publisher Client to ServerManager/Service Providers.
        /// </summary>
        /// <param name="serverInfo">remote server</param>
        /// <param name="caller">who requesting (project, client's). need for logging in ui</param>
        /// <returns></returns>
        public async static Task<PublisherServiceProvider> Initialize(ServerInfo serverInfo, string caller)
        {
            bool isAllowed = false;
            await AsyncActions.RunOnUIAsync(() =>
            {
                //isAllowed = Helpers.CommandAuthenticator.Authorize(ref serverInfo);
                isAllowed = serverInfo.HasAccess();
                //if (!isAllowed)
                //{
                //    LogModule.AddLog(caller, SectorType.Management, "Access Denied! Secret does't match.", DateTime.Now.ToLongTimeString(), LogTypeEnum.Warning);
                //    //isAllowed = true;
                //}
            });
            if (!isAllowed)
            {
                LogModule.AddLog(caller, SectorType.Management, "Access Denied! Secret does't match.", DateTime.Now.ToLongTimeString(), LogTypeEnum.Warning);
                return null;
            }
            //if (!isAllowed) 
            //    return null;
            string serverAddress = string.Empty;
            PublisherServiceProvider publisherServiceProvider = null;
            LogModule.AddLog(caller, SectorType.Management, $"Initializing Server {serverInfo.ServerName}", DateTime.Now.ToLongTimeString(), LogTypeEnum.System);

            try
            {
                serverAddress = string.Concat("http://", serverInfo.ServerAddress, ":", serverInfo.ServerPort);
                //if (Providers.TryGetValue(serverAddress, out publisherServiceProvider))
                //{

                //}
                //else
                //{

                publisherServiceProvider = new PublisherServiceProvider();
                publisherServiceProvider.CurrentClientProvider = new ClientProvider();

                #region |Register SignalGo Server Manager Services|

                publisherServiceProvider.ServerManagerService = publisherServiceProvider.CurrentClientProvider
                    .RegisterServerService<ServerManagerService.ServerServices.ServerManagerService>(publisherServiceProvider.CurrentClientProvider);
                publisherServiceProvider.FileManagerService = publisherServiceProvider.CurrentClientProvider
                    .RegisterServerService<ServerManagerService.ServerServices.FileManagerService>(publisherServiceProvider.CurrentClientProvider);

                #endregion

                #region | ConnectAsync & AutoReconnect |

                //TaskCompletionSource<bool> completeConnection = new TaskCompletionSource<bool>();
                //publisherServiceProvider.CurrentClientProvider
                //   .ConnectAsyncAutoReconnect(serverAddress, isConnected =>
                //   {
                //       try
                //       {
                //           if (isConnected)
                //           {
                //               publisherServiceProvider.CheckConnectionQuality(ref caller, ref serverInfo);

                //           }
                //       }
                //       catch (Exception ex)
                //       {
                //           AutoLogger.Default.LogError(ex, "PublisherServiceProvider Initialize");
                //       }
                //       finally
                //       {
                //           completeConnection.SetResult(true);
                //       }
                //   },3);
                //completeConnection.Task.Wait();

                //Providers.Add(serverAddress, publisherServiceProvider);

                #endregion

                //}
                #region Connect Async Once
                await publisherServiceProvider.CurrentClientProvider.ConnectAsync(serverAddress);
                #endregion
                publisherServiceProvider.CheckConnectionQuality(ref caller, ref serverInfo);
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "initialize client provider error");
                ServerInfo.ServerLogs.Add("error while contacting to server");
            }
            return publisherServiceProvider;
        }

        #region Utility Methods For Connection
        /// <summary>
        /// call server hello method to get simple response
        /// </summary>
        public bool CheckConnectionQuality(ref string caller, ref ServerInfo serverInfo)
        {
            Stopwatch pingWatch = new Stopwatch();
            pingWatch.Start();
            bool isServerAvailaible = false;
            try
            {
                isServerAvailaible = CurrentClientProvider.SendPingAndWaitToReceiveAsync().Result;
                pingWatch.Stop();

                ServerInfo.ServerLogs.Add($"-> ping is {isServerAvailaible} in {pingWatch.Elapsed}");
                LogModule.AddLog(caller, SectorType.Management, $"Server {serverInfo.ServerName} Connection is {isServerAvailaible} in {pingWatch.Elapsed}", DateTime.Now.ToLongTimeString(), LogTypeEnum.System);
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "CheckConnectionQuality Error");
            }
            return isServerAvailaible;
        }
        #endregion
    }
}
