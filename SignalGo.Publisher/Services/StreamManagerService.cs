using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using SignalGo.Publisher.Models;
using SignalGo.Shared.Log;
using System.Threading;

namespace SignalGo.Publisher.Services
{
    public class StreamManagerService
    {

        public static async Task<UploadInfo> UploadAsync(UploadInfo uploadInfo, CancellationToken cancellationToken)
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
                ServerManagerService.StreamServices.ServerManagerStreamService service = new ServerManagerService.StreamServices.ServerManagerStreamService(PublisherServiceProvider.CurrentClientProvider);
                using Stream stream = File.OpenRead(uploadInfo.FilePath);
                var streamInfo = new Shared.Models.StreamInfo()
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
                        streamInfo.Stream.Dispose();
                        streamInfo.Dispose();
                        stream.Close();
                        stream.Dispose();

                        uploadInfo.Status = false;
                        uploadInfo.Description = "Upload Cancelled By User";
                        ServerInfo.ServerLogs.Add("Upload Cancelled By User!");
                        break;
                    }
                    await Task.FromCanceled(cancellationToken);
                };
                result = await service.UploadDataAsync(streamInfo);
                Debug.WriteLine(result);
                if (result == "success")
                {
                    uploadInfo.Status = true;
                    Debug.WriteLine("Yehh. That's Fucking Right!");
                    return uploadInfo;
                }
                else
                    Debug.WriteLine("No. That's Fucking Sucks!");
            }
            catch (Exception ex)
            {
                uploadInfo.Status = false;
                Debug.WriteLine(ex.Message);
                AutoLogger.Default.LogError(ex, "StreamManagerService(UploadAsync)");
            }
            return uploadInfo;
        }

    }
}
