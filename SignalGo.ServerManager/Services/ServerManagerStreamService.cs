using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Log;

namespace SignalGo.ServerManager.Services
{
    /// <summary>
    /// Publisher/Server Manager Stream Gateway svc
    /// </summary>
    [ServiceContract("ServerStreamManager", ServiceType.HttpService, InstanceType.SingleInstance)]
    [ServiceContract("ServerStreamManager", ServiceType.StreamService, InstanceType.SingleInstance)]
    public class ServerManagerStreamService
    {
        public async Task<string> UploadData(Shared.Models.StreamInfo streamInfo)
        {
            double? progress = 0;
            string fileExtension = streamInfo.FileName.Split('.')[1];
            string fileName = streamInfo.FileName.Split('.')[0];
            string outFileName = $"{fileName}{DateTime.Now.ToString("yyyyMMdd_hhmm")}.{fileExtension}";
            //you can use OperationContext<T> to get your client setting with client id if you dont have client plan ignore it (you can read about OperationContext in wiki too)
            //var currentUserSetting = OperationContext<YourSettingClass>.GetCurrent(streamInfo.ClientId);
            string outFilePath = Path.GetFullPath(outFileName, Environment.CurrentDirectory);
            try
            {
                //this is an example to read stream and save to file
                using var fileStream = new FileStream(outFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                var lengthWrite = 0;
                while (lengthWrite != streamInfo.Length)
                {
                    byte[] bufferBytes = new byte[1024];
                    int readCount = await streamInfo.Stream.ReadAsync(bufferBytes, bufferBytes.Length);
                    if (readCount <= 0)
                        break;
                    await fileStream.WriteAsync(bufferBytes, 0, readCount);
                    lengthWrite += readCount;
                    progress += lengthWrite * 100.0 / streamInfo.Length;
                    Debug.WriteLine("progress writed value: " + progress);
                    //if you have a progress bar in client side this code will send your server position to client and client can position it if you don't have progressbar just pervent this line
                    //try
                    //{
                    //    await streamInfo.SetPositionFlushAsync(lengthWrite); // callback error null 
                    //}
                    //catch (Exception ex)
                    //{

                    //}

                }
                Debug.WriteLine("Upload Data Downloaded Successfully");
                //ExtractArchive(filePath);
            }
            catch (Exception ex)
            {
                SignalGo.Shared.Log.AutoLogger.Default.LogError(ex, "DownloadUploadData");
            }

            //make your custom result
            //return MessageContract.Success();
            return "success";
        }

        public bool ExtractArchive(string archive)
        {
            bool isExtracted = false;
            // archive extension:
            switch (archive.Split('.')[1])
            {
                case "zip":
                    ZipFile.ExtractToDirectory(archive, Path.GetFullPath(Directory.GetCurrentDirectory()));
                    isExtracted = true;
                    break;
                case "rar":
                    isExtracted = false;
                    break;
                default:
                    break;
            }
            return isExtracted;
        }
        //public virtual Task DeCompress(CompressionMethodType compressionMethod = CompressionMethodType.Zip)
        //{
        //    try
        //    {
        //        string zipFilePath = Path.Combine(AssembliesPath, "publishArchive.zip");
        //        string extractPath = Path.Combine(AssembliesPath, "extracted");
        //        if (compressionMethod == CompressionMethodType.Zip)
        //            ZipFile.ExtractToDirectory(zipFilePath, extractPath);
        //    }
        //    catch (Exception ex)
        //    {
        //        AutoLogger.Default.LogError(ex, "Publish DeCompression");
        //    }
        //    return Task.CompletedTask;
        //}
    }
}
