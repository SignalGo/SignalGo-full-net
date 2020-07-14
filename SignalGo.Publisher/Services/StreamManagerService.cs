using System;
using System.IO;
using System.Threading.Tasks;
using SignalGo.Publisher.Models;
using SignalGo.Shared.Log;
using System.Threading;
using SignalGo.Shared.Models;
using SignalGo.Publisher.Shared.Models;
using SignalGo.Publisher.Models.Extra;
using System.Diagnostics;

namespace SignalGo.Publisher.Services
{
    public class StreamManagerService
    {
        public static async Task<UploadInfo> UploadAsync(UploadInfo uploadInfo, CancellationToken cancellationToken, ServiceContract serviceContract, Client.ClientProvider clientProvider)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                uploadInfo.Status = false;
                uploadInfo.Description = "Upload Cancelled By User";
                return uploadInfo;
            }
            string result = string.Empty;
            try
            {
                using Stream stream = File.OpenRead(uploadInfo.FilePath);
                using var streamInfo = new StreamInfo()
                {
                    FileName = uploadInfo.FileName,
                    Length = stream.Length,
                    Stream = stream,
                };
                streamInfo.WriteManuallyAsync = async (streamWriter) =>
                {
                    long len = streamInfo.Length.Value;
                    long writed = 0;
                    while (writed < len)
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            byte[] bytes = new byte[1024];
                            var readCount = await streamInfo.Stream.ReadAsync(bytes, bytes.Length);

                            await streamWriter.WriteAsync(bytes, 0, readCount);
                            writed += readCount;
                            uploadInfo.Command.Position = (writed / 1024);
                        }
                        // cancellation occured, Release All Resources and report back
                        uploadInfo.Status = false;
                        uploadInfo.Description = "Upload Cancelled By User";
                        ServerInfo.ServerLogs.Add("Upload Cancelled By User!");
                        break;
                    }
                    await Task.FromCanceled(cancellationToken);
                };

                //var provider = PublisherServiceProvider.Initialize(serverInfo, serviceContract.Name);
                var service = new ServerManagerService.StreamServices.ServerManagerStreamService(clientProvider);
                result = await service.UploadDataAsync(streamInfo, serviceContract);
                if (result == "success")
                {
                    uploadInfo.Status = true;
                    Debug.WriteLine("Stream Completed.");
                    LogModule.AddLog(serviceContract.Name, SectorType.Server, "Stream Completed.", LogTypeEnum.System);
                    return uploadInfo;
                }
                else
                    uploadInfo.Status = false;
                LogModule.AddLog(serviceContract.Name, SectorType.Server, "Stream Completed.", LogTypeEnum.System);
                Debug.WriteLine("Problem Occured In Stream");
            }
            catch (Exception ex)
            {
                uploadInfo.Status = false;
                Console.WriteLine(ex.Message);
                LogModule.AddLog(serviceContract.Name, SectorType.Server, ex.Message, LogTypeEnum.Error);
                AutoLogger.Default.LogError(ex, "StreamManagerService(UploadAsync)");
            }
            return uploadInfo;
        }
    }
}
