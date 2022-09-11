using SignalGo.Publisher.Shared.Models;
using SignalGo.ServiceManager.Core.Models;
using SignalGo.Shared.Log;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace SignalGo.ServiceManager.Core.Engines.Models
{
    public class ServiceUpdater : IDisposable
    {
        public ServiceContract ServiceInfo { get; set; }
        public string UpdateDataPath { get; set; }
        private bool IsSuccess = false;

        public ServiceUpdater(ServiceContract service)
        {
            ServiceInfo = service;
        }
        public ServiceUpdater(ServiceContract service, string updateDataPath)
        {
            ServiceInfo = service;
            UpdateDataPath = updateDataPath;
        }
        /// <summary>
        /// Update Service
        /// </summary>
        /// <returns>Task Status</returns>
        public async Task<TaskStatus> Update()
        {
            try
            {
                var serviceToUpdate = SettingInfo.Current.ServerInfo.SingleOrDefault(s => s.ServerKey == ServiceInfo.ServiceKey);
                serviceToUpdate.Stop();
                if (await BackupService())
                {
                    await DeCompressUpdates(UpdateDataPath);
                }
                else
                {
                    AutoLogger.Default.LogText("Update operation Failed ");
                    return TaskStatus.Faulted;
                }
                serviceToUpdate.Start();
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "Update operation Failed");
                return TaskStatus.Faulted;
            }
            return TaskStatus.RanToCompletion;
        }

        public async Task<bool> BackupService()
        {
            IsSuccess = false;
            try
            {
                IsSuccess = await CompressBackup();
                Debug.WriteLine("service backup has been completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"an error occured in service backup{ex.Message}");
                AutoLogger.Default.LogError(ex, "an error occured in service backup");
            }
            return IsSuccess;
        }
        public async Task<bool> DeCompressUpdates(string archive, CompressionMethodType compressionMethod = CompressionMethodType.Zip)
        {
            IsSuccess = false;
            string ExtractPath = Path.Combine(Directory.GetParent(ServiceInfo.ServiceAssembliesPath).FullName);
            try
            {
                string zipFilePath = Path.Combine(archive);
                if (Directory.Exists(ExtractPath))
                {
                    ////Directory.Delete(ExtractPath, true);
                    //var allFiles = Directory.GetFiles(ExtractPath).ToList();
                    //var ignoredFiles = allFiles.Where(f => f.Contains(".zip")).ToList();
                    //ignoredFiles.AddRange(ServiceInfo.IgnoreFiles.Where(e => e.IsEnabled).Select(x => x.FileName));
                    //foreach (var item in ignoredFiles)
                    //{
                    //    allFiles.RemoveAll(x => x.Contains(item));
                    //}
                    //if (Directory.Exists(Path.Combine(ExtractPath, "publish")))
                    //    Directory.Delete(Path.Combine(ExtractPath, "publish"), true);
                    //if (Directory.Exists(Path.Combine(ExtractPath, "runtimes")))
                    //    Directory.Delete(Path.Combine(ExtractPath, "runtimes"), true);
                    //foreach (var item in allFiles)
                    //{
                    //    File.Delete(item);
                    //}

                    //deletes all files marked as FileStatus.Deleted from server's extract directory
                    ServiceInfo.CompressArchive.FileHashes.Where(x => x.FileStatus == Shared.DataTypes.FileStatusType.Deleted).ToList()
                        .ForEach(x =>
                        {
                            File.Delete(Path.Combine(ExtractPath, x.FileName));
                        });
                }
                if (compressionMethod == CompressionMethodType.Zip)
                    ZipFile.ExtractToDirectory(zipFilePath, ExtractPath, true);
                //Debug.WriteLine("archive extracted successfully");
                Console.WriteLine("archive extracted successfully");
                IsSuccess = true;
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "ServiceUpdater DeCompression Failed");
                Console.WriteLine("archive extraction failed");
            }
            return IsSuccess;
        }

        /// <summary>
        /// Compress Service Files Backup.
        /// </summary>
        /// <param name="compressionMethod"></param>
        /// <param name="includeParent"></param>
        /// <param name="compressionLevel"></param>
        /// <returns></returns>
        public async Task<bool> CompressBackup(CompressionMethodType compressionMethod = CompressionMethodType.Zip, bool includeParent = false, CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            IsSuccess = false;
            string backupPath = Path.Combine(UserSettingInfo.Current.UserSettings.BackupPath);

            string GetServicePathParent = Directory.GetParent(ServiceInfo.ServiceAssembliesPath).FullName;
            // if backup path is'nt valid or exist break operation and display a warn message to user
            if (!Directory.Exists(backupPath))
            {
                Directory.CreateDirectory(backupPath);
                //MessageBox.Show("Backup path not found. please set Valid Backup Path in Application Settings", "No valid path", MessageBoxButton.OK);
                //return IsSuccess;
            }
            string zipFilePath = string.Empty;
            string backupPostFixName = DateTime.Now.ToString("yyyyMMdd_hhmmss");
            string tempServiceBackupPath = Path.Combine(backupPath, ServiceInfo.Name,
                $"{ServiceInfo.Name}_backup{backupPostFixName}");
            // get executable service directory
            string servicePath = Path.Combine(GetServicePathParent);
            try
            {
                // get all files in service path
                string[] allDires = Directory.GetDirectories(GetServicePathParent, "*", SearchOption.AllDirectories);
                string[] allFiles = Directory.GetFiles(GetServicePathParent, "*.*", SearchOption.AllDirectories);
                var filesList = allFiles.ToList();
                //find backup files that stored earlier to ignore in backup
                List<string> ignoreFiles = filesList.Where(x => x.Contains(".zip")).ToList();
                // remove ignored files from backup files
                foreach (var file in ignoreFiles)
                {
                    filesList.Remove(file);
                }
                // make sure backups folder already exist, else create one
                if (!Directory.Exists(Path.Combine(backupPath, ServiceInfo.Name)))
                    Directory.CreateDirectory(Path.Combine(backupPath, ServiceInfo.Name));
                Directory.CreateDirectory(tempServiceBackupPath);
                // dynamic user defined backup path

                allFiles = filesList.ToArray();

                foreach (string dirPath in allDires)
                {
                    Directory.CreateDirectory(dirPath.Replace(servicePath, tempServiceBackupPath));
                }
                foreach (string newPath in allFiles)
                {
                    File.Copy(newPath, newPath.Replace(servicePath, tempServiceBackupPath), true);
                }
                // Store a backup archive with current ServiceName backup_Date&Time Info
                zipFilePath = $"{tempServiceBackupPath}.zip";
                switch (compressionMethod)
                {
                    case CompressionMethodType.None:
                        break;
                    case CompressionMethodType.Zip:
                        ZipFile.CreateFromDirectory(Path.Combine(backupPath, ServiceInfo.Name, $"{ServiceInfo.Name}_backup{backupPostFixName}"), zipFilePath, CompressionLevel.Optimal, includeParent);
                        IsSuccess = true;
                        Directory.Delete(Path.Combine(backupPath, ServiceInfo.Name, $"{ServiceInfo.Name}_backup{backupPostFixName}"), true);
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
            return IsSuccess;
        }

        public void DisposeCache()
        {
            File.Delete(UpdateDataPath);
            Debug.WriteLine("Cache Disposed");
        }
        public void Dispose()
        {
            DisposeCache();
            GC.Collect();
        }
    }
}
