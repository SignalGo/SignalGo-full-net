using System;
using System.IO;
using System.Linq;
using System.Threading;
using MvvmGo.ViewModels;
using System.Diagnostics;
using SignalGo.Shared.Log;
using System.IO.Compression;
using System.Threading.Tasks;
using SignalGo.Publisher.Models;
using SignalGo.Publisher.Services;
using SignalGo.Publisher.Engines.Models;
using SignalGo.Publisher.Engines.Interfaces;
using SignalGo.Shared.Models;
using SignalGo.Publisher.ViewModels;


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

        private long _Size = 0;
        private long _Position = 0;
        private int retryCounter = 0;

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
        public string ServiceName { get; set; }
        public Guid ServiceKey { get; set; }

        /// <summary>
        /// Base Virtual Run
        /// </summary>
        /// <returns></returns>
        public virtual async Task<Process> Run(CancellationToken cancellationToken)
        {
            try
            {
                Status = RunStatusType.Running;
                if (cancellationToken.IsCancellationRequested)
                {
                    Debug.WriteLine("Runner Cancellation Requested");
                    Status = RunStatusType.Running;
                    return null;
                }
                var process = CommandRunner.Run(this, cancellationToken);
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

                zipFilePath = Path.Combine(AssembliesPath, $"{ServiceName}.zip");
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

        public virtual async Task<TaskStatus> Upload(string dataPath, CancellationToken cancellationToken, ServerInfo serverInfo = null, bool forceUpdate = false)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                Debug.WriteLine("Cancellation Requested In Upload Command");
                return TaskStatus.Canceled;
            }
            try
            {
                ServerInfo.ServerLogs.Add($"----(Manually)Started at [{DateTime.Now}] ------");
                // Generate Upload Model based on

                Size = (new FileInfo(dataPath).Length / 1024);
                UploadInfo uploadInfo = new UploadInfo(this)
                {
                    FileName = "publishArchive.zip",
                    HasProgress = true,
                    FilePath = dataPath
                };
                var serviceContract = new ServiceContract
                {
                    Name = ServiceName,
                    ServiceKey = ServiceKey
                };
                foreach (var server in ServerInfo.Servers.Where(x => x.IsUpdated != ServerInfo.ServerInfoStatusEnum.UpdateError).Where(y => y.IsUpdated != ServerInfo.ServerInfoStatusEnum.Updated))
                {
                    // Contact with Server Agents and Make the connection if it possible
                    var contactToProvider = await PublisherServiceProvider.Initialize(server);
                    // contacting with Provider is Not Availaible;
                    if (!contactToProvider)
                    {
                        if (retryCounter < 3)
                        {
                            // increase try counter
                            retryCounter++;
                            // wait and retry up to 3 time
                            await Task.Delay(2000);
                            // Try to update current server again
                            await Upload(dataPath, cancellationToken);
                        }
                        //else, we can't update this erver at this moment (Problems are Possible: Server is Offline,refuse || block || Network Mismatch, unhandled Errors...)
                        ServerInfo.Servers.FirstOrDefault(s => s.ServerKey == server.ServerKey).ServerStatus = ServerInfo.ServerInfoStatusEnum.UpdateError;
                        server.ServerStatus = ServerInfo.ServerInfoStatusEnum.UpdateError;
                        server.IsUpdated = ServerInfo.ServerInfoStatusEnum.UpdateError;
                    }
                    //if (forceUpdate)
                    //    await PublisherServiceProvider.StopServices();
                    // contacting with Provider is OK and server is available.
                    var uploadResult = await StreamManagerService.UploadAsync(uploadInfo, cancellationToken, serviceContract);
                    if (uploadResult.Status)
                    {
                        //if (forceUpdate)
                        //    await PublisherServiceProvider.StartServices();
                        ServerInfo.ServerLogs.Add($"------ Ended at [{DateTime.Now}] ------");
                        server.IsUpdated = ServerInfo.ServerInfoStatusEnum.Updated;
                        server.ServerStatus = ServerInfo.ServerInfoStatusEnum.Updated;
                    } // end server collection foreach
                }
                ServerInfo.ServerLogs.Add($"------ Cancelled at [{DateTime.Now}] ------");
            }
            catch (Exception ex)
            {

            }
            ServerInfo.ServerLogs.Add($"------ Exited at [{DateTime.Now}] ------");
            return TaskStatus.RanToCompletion;
        }


    }
}
