using System;
using SignalGo.Client;
using System.Diagnostics;
using ServerManagerService.Interfaces;
using System.Threading.Tasks;
using SignalGo.Publisher.Models;
using SignalGo.Shared.Log;

namespace SignalGo.Publisher.Services
{
    /// <summary>
    /// init instance of publisher client that connect to signalGo server manager service
    /// simply check the connection state and reliability
    /// </summary>
    public class PublisherServiceProvider //: IDisposable
    {
        //static PublisherServiceProvider()
        //{

        //}
        /// <summary>
        /// instance of client(publisher)
        /// </summary>
        public static ClientProvider CurrentClientProvider { get; set; }
        public static string RemoteServer { get; set; }
        /// <summary>
        /// instance of server manager service
        /// </summary>
        public static IServerManagerService ServerManagerService { get; set; }

        /// <summary>
        /// init client connection to server manager service
        /// </summary>
        public static async Task<bool> Initialize(ServerInfo serverInfo)
        {
            // simple "http://localhost:5468"
            bool isSuccess = false;
            try
            {
                RemoteServer = string.Concat("http://", serverInfo.ServerAddress, ":", serverInfo.ServerPort);
                if (CurrentClientProvider != null)
                {
                    //Debug.WriteLine($"CurrentClientProvider not null");
                    //Debug.WriteLineIf(CurrentClientProvider.IsConnected, "CurrentClientProvider is Connected");
                    CurrentClientProvider.Dispose();
                }
                CurrentClientProvider = new ClientProvider();
                //CurrentClientProvider.ProtocolType = Client.ClientManager.ClientProtocolType.WebSocket; // = asp core
                ServerManagerService = CurrentClientProvider
                    .RegisterServerService<ServerManagerService.ServerServices.ServerManagerService>(CurrentClientProvider);
                //await CheckConnectionQuality();
                CurrentClientProvider.Connect(RemoteServer);
                //, async (isConnected) =>
                //{
                //if (isConnected)
                //{
                //try
                //{
                isSuccess = await CheckConnectionQuality();
                //}
                //catch (Exception ex)
                //{
                //    AutoLogger.Default.LogError(ex, "Bad Connection Quality");
                //}
                //}
                //else
                //{
                //    isSuccess = false;
                //    ServerInfo.This.ServerLogs.Add($"Bad Connection, isConnected:{isConnected}");
                //}
                //isConnected connection state changed
                //});
            }
            catch (Exception ex)
            {
                isSuccess = false;
                AutoLogger.Default.LogError(ex, "initialize client provider error");
                ServerInfo.ServerLogs.Add("error while contacting to server");
            }
            return isSuccess;
        }
        public static async Task RestartServices()
        {
            Guid serviceGuid = Guid.Parse("ae58b3ba-b6ca-45dc-a432-569a2fbd3a88");
            await ServerManagerService.RestartServerAsync(serviceGuid, true);
            ServerInfo.ServerLogs.Add($"-> from ({RemoteServer}): Service Restarted During Update...");
        }
        public static async Task StartServices()
        {
            Guid serviceGuid = Guid.Parse("ae58b3ba-b6ca-45dc-a432-569a2fbd3a88");
            await ServerManagerService.StartServerAsync(serviceGuid);
            ServerInfo.ServerLogs.Add($"-> from ({RemoteServer}): Service Started After Update...");
        }
        public static async Task StopServices()
        {
            Guid serviceGuid = Guid.Parse("ae58b3ba-b6ca-45dc-a432-569a2fbd3a88");
            await ServerManagerService.StopServerAsync(serviceGuid);
            ServerInfo.ServerLogs.Add($"-> from ({RemoteServer}): Service Stopped Before Update...");
        }
        #region Utility Methods For Connection
        /// <summary>
        /// call server hello method to get simple response
        /// </summary>
        public static async Task<bool> CheckConnectionQuality()
        {
            var watch = new Stopwatch();
            watch.Start();
            bool isServerAvailaible = false;
            try
            {
                await Task.Run(async () =>
                {
                    isServerAvailaible = await CurrentClientProvider.SendPingAndWaitToReceiveAsync();
                });
#if Debug
                    Debug.WriteLine($"-> {ServerManagerService.SayHello("saeed")} ,connection is ok.");
                    ServerInfo.This.ServerLogs.Add($"-> {ServerManagerService.SayHello("saeed")} ,connection is ok.");
                    //Debug.WriteLine($"-> ping is {isServerAvailaible}");
                    ServerInfo.This.ServerLogs.Add($"-> ping is {isServerAvailaible} in {watch.Elapsed}");
                    Debug.WriteLine($-> "time elapsed: {watch.Elapsed}");
#else
                Debug.WriteLine($"-> connection is {isServerAvailaible}");
                //ServerInfo.ServerLogs.Add($"-> from ({RemoteServer}): {ServerManagerService.SayHello("saeed")} ,connection is ok.");
                ServerInfo.ServerLogs.Add($"-> ping is {isServerAvailaible} in {watch.Elapsed}");
#endif
                watch.Stop();
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "CheckConnectionQuality Error");
            }
            return isServerAvailaible;
        }
        #endregion

        #region IDisposable Support
        //private bool disposedValue = false; // To detect redundant calls

        //protected virtual void Dispose(bool disposing)
        //{
        //    if (!disposedValue)
        //    {
        //        if (disposing)
        //        {
        //            // TODO: dispose managed state (managed objects).
        //        }

        //        // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
        //        // TODO: set large fields to null.
        //        //CurrentClientProvider.Dispose();

        //        disposedValue = true;
        //    }
        //}

        //// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        //// ~PublisherServiceProvider()
        //// {
        ////   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        ////   Dispose(false);
        //// }

        //// This code added to correctly implement the disposable pattern.
        //public void Dispose()
        //{
        //    // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //    Dispose(true);
        //    // TODO: uncomment the following line if the finalizer is overridden above.
        //    // GC.SuppressFinalize(this);
        //}
        #endregion
    }
}
