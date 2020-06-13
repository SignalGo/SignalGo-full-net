using System;
using SignalGo.Client;
using System.Diagnostics;
using ServerManagerService.Interfaces;
using System.Threading.Tasks;
using SignalGo.Publisher.Models;
using SignalGo.Shared.Log;
using System.Collections.Generic;

namespace SignalGo.Publisher.Services
{
    /// <summary>
    /// init instance of publisher client that connect to signalGo server manager service
    /// simply check the connection state and reliability
    /// </summary>
    public class PublisherServiceProvider //: IDisposable
    {

        public static Dictionary<string, PublisherServiceProvider> Providers { get; set; } = new Dictionary<string, PublisherServiceProvider>();
        /// <summary>
        /// instance of server manager service
        /// </summary>
        public IServerManagerService ServerManagerService { get; set; }
        public IFileManagerService FileManagerService { get; set; }

        /// <summary>
        /// instance of client(publisher)
        /// </summary>
        public ClientProvider CurrentClientProvider { get; set; }
        public string RemoteServer { get; set; }


        /// <summary>
        /// init client connection to server manager service
        /// </summary>
        public static PublisherServiceProvider Initialize(ServerInfo serverInfo)
        {
            PublisherServiceProvider publisherServiceProvider = null;
            try
            {
                var serverAddress = string.Concat("http://", serverInfo.ServerAddress, ":", serverInfo.ServerPort);
                if (Providers.TryGetValue(serverAddress, out publisherServiceProvider))
                {
                }
                else
                {
                    publisherServiceProvider = new PublisherServiceProvider();
                    publisherServiceProvider.CurrentClientProvider = new ClientProvider();
                    //CurrentClientProvider.ProtocolType = Client.ClientManager.ClientProtocolType.WebSocket; // = asp core

                    publisherServiceProvider.ServerManagerService = publisherServiceProvider.CurrentClientProvider
                        .RegisterServerService<ServerManagerService.ServerServices.ServerManagerService>(publisherServiceProvider.CurrentClientProvider);
                    publisherServiceProvider.FileManagerService = publisherServiceProvider.CurrentClientProvider
                        .RegisterServerService<ServerManagerService.ServerServices.FileManagerService>(publisherServiceProvider.CurrentClientProvider);
                    TaskCompletionSource<bool> completeConnection = new TaskCompletionSource<bool>();
                    publisherServiceProvider.CurrentClientProvider.ConnectAsyncAutoReconnect(serverAddress, x =>
                    {
                        try
                        {
                            if (x)
                                publisherServiceProvider.CheckConnectionQuality();
                        }
                        catch (Exception ex) { }
                        finally
                        {
                            completeConnection.SetResult(true);
                        }
                    });
                    completeConnection.Task.Wait();
                    Providers.Add(serverAddress, publisherServiceProvider);
                }
            }
            catch (Exception ex)
            {
                //isSuccess = false;
                AutoLogger.Default.LogError(ex, "initialize client provider error");
                ServerInfo.ServerLogs.Add("error while contacting to server");
            }
            return publisherServiceProvider;
        }
        #region Utility Methods For Connection
        /// <summary>
        /// call server hello method to get simple response
        /// </summary>
        public bool CheckConnectionQuality()
        {
            var watch = new Stopwatch();
            watch.Start();
            bool isServerAvailaible = false;
            try
            {
                isServerAvailaible = CurrentClientProvider.SendPingAndWaitToReceive();
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
