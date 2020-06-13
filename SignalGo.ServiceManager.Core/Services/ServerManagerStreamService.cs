using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using SignalGo.Shared.Log;
using SignalGo.Shared.Models;
using System.Threading.Tasks;
using SignalGo.Shared.DataTypes;
using SignalGo.ServiceManager.Core.Engines.Models;
using SignalGo.Publisher.Shared.Models;
using SignalGo.ServiceManager.Core.Models;

namespace SignalGo.ServiceManager.Core.Services
{
    /// <summary>
    /// Publisher/Server Manager Stream Gateway svc
    /// </summary>
    [ServiceContract("ServerStreamManager", ServiceType.StreamService, InstanceType.SingleInstance)]
    public class ServerManagerStreamService
    {
        //ServiceUpdater ServiceUpdater { get; set; }
        //public FileStream FileStream { get; set; }
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
                    using var fileStream = new FileStream(outFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                    var lengthWrite = 0;
                    while (lengthWrite != streamInfo.Length)
                    {
                        byte[] bufferBytes = new byte[1024 * 1024];
                        int readCount = await streamInfo.Stream.ReadAsync(bufferBytes, bufferBytes.Length);
                        if (readCount <= 0)
                            break;
                        await fileStream.WriteAsync(bufferBytes, 0, readCount);
                        lengthWrite += readCount;
                        progress += lengthWrite * 100.0 / streamInfo.Length;
                    }
                    Debug.WriteLine("Upload Data Downloaded Successfully");
                }
                catch (Exception ex)
                {
                    AutoLogger.Default.LogError(ex, "DownloadUploadData");
                }
                //finally
                //{
                //    Dispose();
                //}

                var service = new ServiceContract
                {
                    Name = serviceToUpdate.Name,
                    ServiceAssembliesPath = serviceToUpdate.AssemblyPath,
                    ServiceKey = serviceToUpdate.ServerKey,
                    IgnoreFiles = serviceContract.IgnoreFiles
                };
                using (var serviceUpdater = new ServiceUpdater(service, outFilePath))
                {
                    await serviceUpdater.Update();
                }
                //return MessageContract.Success();
            }
            catch (Exception ex)
            {
                return "failed";
            }
            return "success";
        }

        /// <summary>
        /// downlaod file from service
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="serviceKey"></param>
        /// <returns></returns>
        public StreamInfo DownloadFileData(string filePath, Guid serviceKey)
        {
            CheckServerPath(filePath, serviceKey);
            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return new StreamInfo(stream) { Length = stream.Length, FileName = Path.GetFileName(filePath) };
        }

        /// <summary>
        /// save data to service
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="serviceKey"></param>
        /// <returns></returns>
        public async Task<bool> SaveFileData(StreamInfo<string> stream, Guid serviceKey)
        {
            CheckServerPath(stream.Data, serviceKey);
            using FileStream fileStream = new FileStream(stream.Data, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            fileStream.SetLength(0);
            var lengthWrite = 0;
            while (lengthWrite < stream.Length)
            {
                byte[] bufferBytes = new byte[1024 * 1024];
                int readCount = await stream.Stream.ReadAsync(bufferBytes, bufferBytes.Length);
                if (readCount <= 0)
                    break;
                await fileStream.WriteAsync(bufferBytes, 0, readCount);
                lengthWrite += readCount;
            }
            return true;
        }

        private ServerInfo CheckServerPath(string filePath, Guid serviceKey)
        {
            var find = SettingInfo.Current.ServerInfo.FirstOrDefault(x => x.ServerKey == serviceKey);
            if (find == null)
                throw new Exception($"Service {serviceKey} not found!");
            else if (Path.GetDirectoryName(find.AssemblyPath) != Path.GetDirectoryName(filePath))
                throw new Exception($"Access to the path denied!");
            return find;
        }

        //private void Dispose()
        //{
        //    try
        //    {
        //        FileStream.Close();
        //        FileStream.Dispose();
        //        //GC.Collect();
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex.Message);
        //    }
        //}
    }
}
