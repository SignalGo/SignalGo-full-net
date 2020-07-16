using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using SignalGo.Shared.Log;
using System.Threading.Tasks;
using SignalGo.Shared.Models;
using System.Drawing.Imaging;
using SignalGo.Shared.DataTypes;
using SignalGo.Publisher.Shared.Models;
using SignalGo.ServiceManager.Core.Models;
using SignalGo.ServiceManager.Core.Engines.Models;
using System.Text;

namespace SignalGo.ServiceManager.Core.Services
{
    /// <summary>
    /// Publisher/Server Manager Stream Gateway svc
    /// </summary>
    [ServiceContract("ServerStreamManager", ServiceType.StreamService, InstanceType.SingleInstance)]
    public class ServerManagerStreamService
    {
        /// <summary>
        /// Download Data from client as stream service
        /// </summary>
        /// <param name="streamInfo"></param>
        /// <returns></returns>
        public async Task<string> UploadData(StreamInfo streamInfo, ServiceContract serviceContract)
        {
            // formmat output file name with current datetime
            string outFileName = $"{serviceContract.Name}{DateTime.Now:yyyyMMdd_hhmmss}.zip";
            // set output file path for write later
            string outFilePath = Path.GetFullPath(outFileName, Environment.CurrentDirectory);
            try
            {
                var serviceToUpdate = SettingInfo.Current.ServerInfo
                    .SingleOrDefault(s => s.ServerKey == serviceContract.ServiceKey);
                if (serviceToUpdate == null)
                    return "failed";
                double? progress = 0;
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
                    Console.WriteLine("Upload Data, Downloaded Successfully");
                }
                catch (Exception ex)
                {
                    AutoLogger.Default.LogError(ex, "DownloadUploadData");
                }
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
                AutoLogger.Default.LogError(ex, "UploadData");
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
            ServerInfo.CheckServerPath(filePath, serviceKey);
            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return new StreamInfo(stream)
            {
                Length = stream.Length,
                FileName = Path.GetFileName(filePath)
            };
        }

        /// <summary>
        /// save data to service
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="serviceKey"></param>
        /// <returns></returns>
        public async Task<bool> SaveFileData(StreamInfo<string> stream, Guid serviceKey)
        {
            ServerInfo.CheckServerPath(stream.Data, serviceKey);
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

        /// <summary>
        /// using to focus specified service tab, in server manager
        /// </summary>
        public static Func<ServerInfo, Task> FocusTabFunc;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceKey"></param>
        /// <returns></returns>
        public async Task<StreamInfo> CaptureApplicationProcess(Guid serviceKey)
        {
            //BufferedStream bufferedStream;
            MemoryStream memoryStream = new MemoryStream();
            //bufferedStream = new BufferedStream(memoryStream);
            try
            {
                ServerInfo find = SettingInfo.Current.ServerInfo.FirstOrDefault(x => x.ServerKey == serviceKey);
                if (find == null)
                {
                    string message = $"Service {serviceKey} not found!";
                    AutoLogger.Default.LogText(message);
                    await memoryStream
                        .WriteAsync(Encoding.UTF8.GetBytes(message), 0, message.Length);
                    return new StreamInfo(memoryStream) { Length = memoryStream.Length, };
                    //throw new Exception($"Service {serviceKey} not found!");
                }
                using (Process proc = find.CurrentServerBase.BaseProcess)
                {
                    await FocusTabFunc(find);
                    WindowRectangleInfo.GetWindowRect(proc.MainWindowHandle, out WindowRectangleInfo.WindowRectangleStruct windowRectangle);
                    var bmp = WindowRectangleInfo.CaptureWindowImage(proc.MainWindowHandle, windowRectangle);
                    bmp.Save(memoryStream, ImageFormat.Png);
                }
                memoryStream.Seek(0, SeekOrigin.Begin);
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "CaptureApplicationProcess");
                return null;
            }
            return new StreamInfo(memoryStream) { Length = memoryStream.Length, };
        }
    }

}
