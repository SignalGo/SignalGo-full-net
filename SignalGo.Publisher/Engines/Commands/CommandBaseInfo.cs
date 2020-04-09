using MvvmGo.Extensions;
using MvvmGo.ViewModels;
using SignalGo.Publisher.Engines.Interfaces;
using SignalGo.Publisher.Engines.Models;
using SignalGo.Publisher.Models;
using SignalGo.Publisher.Services;
using SignalGo.Shared.Log;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SignalGo.Publisher.Models.ServerInfo;

namespace SignalGo.Publisher.Engines.Commands
{
    public abstract class CommandBaseInfo : PropertyChangedViewModel, ICommand, IPublish//, IDisposable
    {

        private RunStatusType _Status;
        public RunStatusType Status
        {
            get => _Status; set
            {
                _Status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        long _Size = 0;
        long _Position = 0;

        /// <summary>
        /// size of data/file
        /// </summary>
        public long Size
        {
            get
            {
                return _Size;// / 1024;
            }
            set
            {
                _Size = value;
                OnPropertyChanged("Size");
            }
        }

        /// <summary>
        /// current position of stream/file
        /// </summary>
        public long Position
        {
            get
            {
                return _Position;// / 1024;
            }
            set
            {
                _Position = value;
                OnPropertyChanged("Position");
            }
        }
        public string Name { get; set; }
        public string ExecutableFile { get; set; }
        public string Command { get; set; }
        public string Arguments { get; set; }
        public bool IsEnabled { get; set; }
        /// <summary>
        /// working directory path that process start on it
        /// </summary>
        public string WorkingPath { get; set; }
        /// <summary>
        /// assemblies and publish foler/files path
        /// </summary>
        public string AssembliesPath { get; set; }

        /// <summary>
        /// Base Virtual Run
        /// </summary>
        /// <returns></returns>
        public virtual async Task<Process> Run()
        {
            try
            {
                Status = RunStatusType.Running;
                var process = CommandRunner.Run(this);
                if (process.Status == TaskStatus.Faulted)
                    Status = RunStatusType.Error;
                Status = RunStatusType.Done;
                return process.Result;
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "Runner CommandBaseInfo");
                Status = RunStatusType.Error;
                return null;
            }
        }

        public virtual Task<string> Compress(CompressionMethodType compressionMethod = CompressionMethodType.Zip, bool includeParent = false, CompressionLevel compressionLevel = CompressionLevel.Fastest)
        {
            string zipFilePath = string.Empty;
            try
            {
                string[] directories = Directory.GetDirectories(AssembliesPath);
                string publishDir = directories.SingleOrDefault(x => x.Contains("publish"));

                zipFilePath = Path.Combine(AssembliesPath, "publishArchive.zip");
                if (File.Exists(zipFilePath))
                    File.Delete(zipFilePath);
                switch (compressionMethod)
                {
                    case CompressionMethodType.None:
                        break;
                    case CompressionMethodType.Zip:
                        ZipFile.CreateFromDirectory(publishDir, zipFilePath, CompressionLevel.Optimal, includeParent);
                        break;
                    case CompressionMethodType.Gzip:
                        throw new NotImplementedException("Gzip method not implemented yet.");
                        break;
                    case CompressionMethodType.Rar:
                        throw new NotImplementedException("Rar method not implemented yet.");
                        break;
                    case CompressionMethodType.Tar:
                        throw new NotImplementedException("Tar method not implemented yet.");
                        break;
                    case CompressionMethodType.bzip:
                        throw new NotImplementedException("bzip method not implemented yet.");
                        break;
                    case CompressionMethodType.Zip7:
                        throw new NotImplementedException("7Zip method not implemented yet.");
                        break;
                    default:
                        break;
                }

            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "Publisher Compression");
            }
            return Task.FromResult(zipFilePath);
        }
        public virtual Task DeCompress(CompressionMethodType compressionMethod = CompressionMethodType.Zip)
        {
            try
            {
                string zipFilePath = Path.Combine(AssembliesPath, "publishArchive.zip");
                string extractPath = Path.Combine(AssembliesPath, "extracted");
                if (compressionMethod == CompressionMethodType.Zip)
                    ZipFile.ExtractToDirectory(zipFilePath, extractPath);
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "Publisher DeCompression");
            }
            return Task.CompletedTask;
        }
        int retryCounter = 0;

        public virtual async Task<TaskStatus> Upload(string dataPath, ServerInfo serverInfo = null)
        {

            try
            {
                // init upload model fields
                Size = (new FileInfo(dataPath).Length / 1024);
                UploadInfo uploadInfo = new UploadInfo(this)
                {
                    FileName = "publishArchive.zip",
                    HasProgress = true,
                    FilePath = dataPath
                };


                foreach (var server in ServerInfo.Servers.Where(x => x.IsUpdated != ServerInfo.ServerInfoStatusEnum.UpdateError).Where(y => y.IsUpdated != ServerInfoStatusEnum.Updated))
                {
                    // contact with servers agents
                    var contactToProvider = await PublisherServiceProvider.Initialize(server);
                    if (!contactToProvider)
                    {
                        if (retryCounter < 3)
                        {
                            retryCounter++;
                            // wait and retry for 3 time
                            await Task.Delay(2000);
                            // send retry signal for this server
                            await Upload(dataPath);
                        }
                        //else, we can't update this erver at this moment (Problems are Possible: Server is Offline,refuse || block || Network Mismatch, unhandled Errors...)
                        Servers.FirstOrDefault(s => s.ServerKey == server.ServerKey).ServerStatus = ServerInfoStatusEnum.UpdateError;
                        server.ServerStatus = ServerInfoStatusEnum.UpdateError;
                        server.IsUpdated = ServerInfoStatusEnum.UpdateError;
                    }
                    // contacting with Provider is OK;
                    var uploadResult = await StreamManagerService.UploadAsync(uploadInfo);
                    if (uploadResult.Status)
                    {
                        server.IsUpdated = ServerInfoStatusEnum.Updated;
                        server.ServerStatus = ServerInfoStatusEnum.Updated;
                    }
                } // end server collection foreach
            }
            catch (Exception ex)
            {

            }

            return TaskStatus.RanToCompletion;
        }
        //public virtual async Task<TaskStatus> Upload(string dataPath, ServerInfo serverInfo = null)
        //{
        //    if (string.IsNullOrEmpty(dataPath))
        //        return TaskStatus.Faulted;
        //    try
        //    {
        //        this.Size = (new FileInfo(dataPath).Length / 1024);
        //        UploadInfo uploadInfo = new UploadInfo(this)
        //        {
        //            FileName = "publishArchive.zip",
        //            HasProgress = true,
        //            FilePath = dataPath
        //        };
        //        // entry from Servers Queue, no task failed yet
        //        if (serverInfo == null)
        //        {
        //            foreach (var server in ServerInfo.Servers.Where(x => x.IsUpdated == false))
        //            {

        //                var provider = await PublisherServiceProvider.Initialize(server);
        //                if (!provider)
        //                {
        //                    if (retryCounter < 3)
        //                    {
        //                        retryCounter++;
        //                        // wait 5 sec and retry to 3 time
        //                        await Task.Delay(2000);
        //                        // send retry signal for this server
        //                        await Upload(dataPath, server);
        //                    }
        //                    //else
        //                    return TaskStatus.Faulted;
        //                }
        //                //provider = true;
        //                var uploadResult = await StreamManagerService.UploadAsync(uploadInfo);
        //                if (uploadResult.Status)
        //                    server.IsUpdated = true;
        //            }
        //        }
        //        // error occured and Entry with Retry action, just retry upload to specified server that failed on top:
        //        else
        //        {
        //            var provider = await PublisherServiceProvider.Initialize(serverInfo);
        //            if (!provider)
        //            {
        //                if (retryCounter < 3)
        //                {
        //                    retryCounter++;
        //                    // wait 5 sec and retry to 3 time
        //                    await Task.Delay(2000);
        //                    await Upload(dataPath, serverInfo);
        //                }
        //                //else
        //                serverInfo.IsUpdated = false;
        //                if (ServerInfo.Servers.Count>0 && ServerInfo.Servers.Any(k => k.ServerKey == serverInfo.ServerKey))
        //                    ServerInfo.Servers.SingleOrDefault(k => k.ServerKey == serverInfo.ServerKey).IsUpdated = false;
        //                ServerInfo.This.RemoveServerFromQueueCommand(serverInfo);
        //                serverInfo = null;
        //                retryCounter = 0;
        //                await Upload(dataPath);
        //                return TaskStatus.Faulted;
        //            }
        //            //provider = true;
        //            retryCounter = 0;
        //            await StreamManagerService.UploadAsync(uploadInfo);
        //            serverInfo.IsUpdated = true;
        //            serverInfo = null;
        //            await Upload(dataPath);
        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        AutoLogger.Default.LogError(ex, "Publisher Upload To Servers Error");
        //        Status = RunStatusType.Error;
        //    }
        //    retryCounter = 0;
        //    return TaskStatus.RanToCompletion;
        //}

        #region IDisposable Support

        //private bool disposedValue = false; // To detect redundant calls

        //protected virtual void Dispose(bool disposing)
        //{
        //    if (!disposedValue)
        //    {
        //        if (disposing)
        //        {
        //            // TODO: dispose managed state (managed objects).
        //        }

        //        // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
        //        // TODO: set large fields to null.

        //        disposedValue = true;
        //    }
        //}

        //// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        //// ~CommandBaseInfo()
        //// {
        ////   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        ////   Dispose(false);
        //// }

        //// This code added to correctly implement the disposable pattern.
        //public void Dispose()
        //{
        //    // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //    Dispose(true);
        //    // TODO: uncomment the following line if the finalizer is overridden above.
        //    //GC.SuppressFinalize(this);
        //}

        #endregion
    }
}
