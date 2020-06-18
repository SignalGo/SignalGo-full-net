using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using SignalGo.Publisher.Models;
using SignalGo.Shared.Log;
using System.Threading;
using SignalGo.Shared.Models;
using SignalGo.Publisher.Shared.Models;

namespace SignalGo.Publisher.Services
{
    public class StreamManagerService
    {
        public static async Task<UploadInfo> UploadAsync(UploadInfo uploadInfo, CancellationToken cancellationToken, ServiceContract serviceContract, ServerInfo serverInfo)
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
                        //await DisposeAsync();
                        uploadInfo.Description = "Upload Cancelled By User";
                        ServerInfo.ServerLogs.Add("Upload Cancelled By User!");
                        break;
                    }
                    await Task.FromCanceled(cancellationToken);
                };

                var provider = PublisherServiceProvider.Initialize(serverInfo);
                var service = new ServerManagerService.StreamServices.ServerManagerStreamService(provider.CurrentClientProvider);
                result = await service.UploadDataAsync(streamInfo, serviceContract);
                Debug.WriteLine(result);
                if (result == "success")
                {
                    uploadInfo.Status = true;
                    Debug.WriteLine("Streaming Completed.");
                    return uploadInfo;
                }
                else
                    uploadInfo.Status = false;
                Debug.WriteLine("Problem Occured In Stream");
            }
            catch (Exception ex)
            {
                uploadInfo.Status = false;
                Debug.WriteLine(ex.Message);
                AutoLogger.Default.LogError(ex, "StreamManagerService(UploadAsync)");
            }
            return uploadInfo;
        }

        //public async Task<bool> CaptureApplicationProcessAsync(StreamInfo<string> stream)
        //{
        //    using FileStream fileStream = new FileStream(stream.Data, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        //    fileStream.SetLength(0);
        //    var lengthWrite = 0;
        //    while (lengthWrite < stream.Length)
        //    {
        //        byte[] bufferBytes = new byte[1024 * 1024];
        //        int readCount = await stream.Stream.ReadAsync(bufferBytes, bufferBytes.Length);
        //        if (readCount <= 0)
        //            break;
        //        await fileStream.WriteAsync(bufferBytes, 0, readCount);
        //        lengthWrite += readCount;
        //    }
        //    return true;
        //}

        //public static async Task DisposeAsync()
        //{
        //    //streamInfo.Stream.Dispose();
        //    //streamInfo.Dispose();
        //    //stream.Close();
        //    //stream.Dispose();
        //    await service.DisposeStreamAsync();
        //}

    }
}
