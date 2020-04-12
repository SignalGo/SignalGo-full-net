using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using SignalGo.Publisher.Models;
using SignalGo.Shared.Log;

namespace SignalGo.Publisher.Services
{
    public class StreamManagerService
    {

        public static async Task<UploadInfo> UploadAsync(UploadInfo uploadInfo)
        {
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
                        byte[] bytes = new byte[1024];
                        var readCount = await streamInfo.Stream.ReadAsync(bytes, bytes.Length);

                        await streamWriter.WriteAsync(bytes, 0, readCount);
                        writed += readCount;
                        uploadInfo.Command.Position = (writed / 1024);
                    }
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
