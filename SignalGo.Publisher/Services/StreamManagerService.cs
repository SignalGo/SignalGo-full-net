using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using SignalGo.Publisher.Models;

namespace SignalGo.Publisher.Services
{
    public static class StreamManagerService
    {

        public static async Task<UploadInfo> UploadAsync(UploadInfo uploadInfo)
        {
            var watch = new Stopwatch();
            watch.Start();
            ServerManagerService.StreamServices.ServerManagerStreamService service = new ServerManagerService.StreamServices.ServerManagerStreamService(PublisherServiceProvider.CurrentClientProvider);

            Debug.WriteLine(PublisherServiceProvider.CurrentClientProvider.ClientId);
            using Stream stream = File.OpenRead(uploadInfo.FilePath);
            var streamInfo = new Shared.Models.StreamInfo()
            {
                FileName = uploadInfo.FileName,
                Length = stream.Length,
                Stream = stream,
                ClientId = PublisherServiceProvider.CurrentClientProvider.ClientId, // null


                //GetPositionFlush = new Func<Task<long>>(() =>
                //{
                //    return Task.FromResult(stream.Position);
                //})
            };
            
            //its c# shit ke zard nashe
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
                    uploadInfo.Command.Position = writed;
                    //var pos = await streamInfo.GetPositionFlush();
                }
            };
            string result = await service.UploadDataAsync(streamInfo);

            Debug.WriteLine(result);

            watch.Stop();
            Debug.WriteLine($"stop watch ended in , {watch.Elapsed}, Timestamp {Stopwatch.GetTimestamp()}");
            if (result == "success")
            {
                uploadInfo.Status = true;
                Debug.WriteLine("Yehh. That's Fucking Right!");
                return uploadInfo;
            }
            else
                Debug.WriteLine("No. That's Fucking Sucks!");
            uploadInfo.Status = false;
            return uploadInfo;
        }

    }
}
