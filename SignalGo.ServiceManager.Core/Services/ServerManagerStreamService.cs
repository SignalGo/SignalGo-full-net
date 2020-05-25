using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using SignalGo.Shared.Log;
using SignalGo.Shared.Models;
using SignalGo.Shared.DataTypes;
using SignalGo.ServiceManager.Engines.Models;
using System.Linq;

namespace SignalGo.ServiceManager.Services
{
    /// <summary>
    /// Publisher/Server Manager Stream Gateway svc
    /// </summary>
    [ServiceContract("ServerStreamManager", ServiceType.HttpService, InstanceType.SingleInstance)]
    [ServiceContract("ServerStreamManager", ServiceType.StreamService, InstanceType.SingleInstance)]
    public class ServerManagerStreamService
    {
        ServiceUpdater ServiceUpdater { get; set; }
        public FileStream FileStream { get; set; }
        /// <summary>
        /// Download Data from client as stream service
        /// </summary>
        /// <param name="streamInfo"></param>
        /// <returns></returns>
        public async Task<string> UploadData(StreamInfo streamInfo, ServiceContract serviceContract)
        {
            string outFileName = $"{serviceContract.Name}{DateTime.Now.ToString("yyyyMMdd_hhmmss")}.zip";
            string outFilePath = Path.GetFullPath(outFileName, Environment.CurrentDirectory);
            try
            {
                var serviceToUpdate = Models.SettingInfo.Current.ServerInfo.SingleOrDefault(s => s.ServerKey == serviceContract.ServiceKey);
                if (serviceToUpdate == null)
                    return "failed";
                double? progress = 0;
                //string fileExtension = streamInfo.FileName.Split('.')[1];
                //string fileName = streamInfo.FileName.Split('.')[0];

                try
                {
                    FileStream = new FileStream(outFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                    var lengthWrite = 0;
                    while (lengthWrite != streamInfo.Length)
                    {
                        byte[] bufferBytes = new byte[1024 * 1024];
                        int readCount = await streamInfo.Stream.ReadAsync(bufferBytes, bufferBytes.Length);
                        if (readCount <= 0)
                            break;
                        await FileStream.WriteAsync(bufferBytes, 0, readCount);
                        lengthWrite += readCount;
                        progress += lengthWrite * 100.0 / streamInfo.Length;
                    }
                    Debug.WriteLine("Upload Data Downloaded Successfully");
                }
                catch (Exception ex)
                {
                    AutoLogger.Default.LogError(ex, "DownloadUploadData");
                }
                finally
                {
                    Dispose();
                }

                var service = new ServiceContract
                {
                    Name = serviceToUpdate.Name,
                    ServiceAssembliesPath = serviceToUpdate.AssemblyPath,
                    ServiceKey = serviceToUpdate.ServerKey,
                    IgnoreFiles = serviceContract.IgnoreFiles
                };
                using (ServiceUpdater = new ServiceUpdater(service, outFilePath))
                {
                    await ServiceUpdater.Update();
                }
                //return MessageContract.Success();
            }
            catch (Exception ex)
            {
                return "failed";
            }
            return "success";
        }
        public void Dispose()
        {
            try
            {
                FileStream.Close();
                FileStream.Dispose();
                //GC.Collect();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
