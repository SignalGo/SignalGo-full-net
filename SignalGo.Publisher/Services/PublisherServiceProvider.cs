using System;
using SignalGo.Client;
using System.Diagnostics;
using ServerManagerService.Interfaces;
using System.Threading.Tasks;
using SignalGo.Publisher.Models;
using System.IO;

namespace SignalGo.Publisher.Services
{
    /// <summary>
    /// init instance of publisher client that connect to signalGo server manager service
    /// simply check the connection state and reliability
    /// </summary>
    public static class PublisherServiceProvider
    {
        //static PublisherServiceProvider()
        //{

        //}
        /// <summary>
        /// instance of client(publisher)
        /// </summary>
        public static ClientProvider CurrentClientProvider { get; set; }

        /// <summary>
        /// instance of server manager service
        /// </summary>
        public static IServerManagerService ServerManagerService { get; set; }

        public static Func<Task<long>> taskProgress;
        /// <summary>
        /// init client connection to server manager service
        /// </summary>
        public static void Initialize()
        {
            CurrentClientProvider = new ClientProvider();
            CurrentClientProvider.ProtocolType = Client.ClientManager.ClientProtocolType.WebSocket;
            ServerManagerService = CurrentClientProvider.RegisterServerService<ServerManagerService.ServerServices.ServerManagerService>(CurrentClientProvider);

            CurrentClientProvider.ConnectAsyncAutoReconnect("http://localhost:5468", async (isConnected) =>
            {
                if (isConnected)
                {
                    try
                    {
                        
                        CheckConnectionQuality();
                        //var uploadResult = new UploadInfo();
                        //string fileName = @"uploadme.zip";
                        //string simpleFilePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

                        //var uploadInfo = new UploadInfo
                        //{
                        //    FileName = fileName,
                        //    //FileExtension = "zip",
                        //    Size = new FileInfo(simpleFilePath).Length,
                        //    HasProgress = true,
                        //    FilePath = simpleFilePath
                        //};
                        //uploadResult = await StreamManagerService.UploadAsync(uploadInfo);
                    }
                    catch (Exception ex)
                    {

                    }
                }
                //isConnected connection state changed
            });
        }

        /// <summary>
        /// call server hello method to get simple response
        /// </summary>
        public static void CheckConnectionQuality()
        {
            Debug.WriteLine($"{ServerManagerService.SayHello("saeed")} ,connection is ok");

        }
    }
}
