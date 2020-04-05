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

        long _Size;
        long _Position;

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

        public virtual async Task<Process> Run()
        {
            try
            {
                Status = RunStatusType.Running;
                var process = CommandRunner.Run(this);
                //if (process.Status == TaskStatus.Faulted)
                    //Status = RunStatusType.Error;
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
                        throw new NotImplementedException("this method not implemented yet.");
                        break;
                    case CompressionMethodType.Rar:
                        throw new NotImplementedException("this method not implemented yet.");
                        break;
                    case CompressionMethodType.Tar:
                        throw new NotImplementedException("this method not implemented yet.");
                        break;
                    case CompressionMethodType.bzip:
                        throw new NotImplementedException("this method not implemented yet.");
                        break;
                    case CompressionMethodType.Zip7:
                        throw new NotImplementedException("this method not implemented yet.");
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
        public virtual async Task Upload(string dataPath)
        {
            try
            {
                //string fileName = Path.Combine(AssembliesPath, "publishArchive.zip");
                Size = (new FileInfo(dataPath).Length / 1024);
                UploadInfo uploadInfo = new UploadInfo(this)
                {
                    FileName = "publishArchive.zip",
                    HasProgress = true,
                    FilePath = dataPath
                };
                await StreamManagerService.UploadAsync(uploadInfo);
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "Publisher Upload");
                Status = RunStatusType.Error;
            }
        }

        #region IDisposable Support
        /*
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~CommandBaseInfo()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            //GC.SuppressFinalize(this);
        }
        */
        #endregion
    }
}
