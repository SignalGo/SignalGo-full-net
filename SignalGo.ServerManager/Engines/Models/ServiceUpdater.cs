using SignalGo.ServerManager.Models;
using SignalGo.Shared.Log;
using SignalGo.Shared.Models;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SignalGo.ServerManager.Engines.Models
{
    public class ServiceUpdater : IDisposable
    {
        //public string ExtractPath { get; set; }
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
            //ExtractPath = Path.Combine(Directory.GetParent(service.AssemblyPath).FullName);
        }

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
                    throw new InvalidOperationException();
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
                    //Directory.Delete(ExtractPath, true);
                    var allFiles = Directory.GetFiles(ExtractPath);
                    var filesList = allFiles.ToList();
                    var ignoredFiles = filesList.Where(f => f.Contains(".zip")).ToList();
                    foreach (var item in ignoredFiles)
                    {
                        filesList.Remove(item);
                    }
                    if (Directory.Exists(Path.Combine(ExtractPath, "publish")))
                        Directory.Delete(Path.Combine(ExtractPath, "publish"), true);
                    if (Directory.Exists(Path.Combine(ExtractPath, "runtimes")))
                        Directory.Delete(Path.Combine(ExtractPath, "runtimes"), true);
                    allFiles = filesList.ToArray();
                    foreach (var item in allFiles)
                    {
                        File.Delete(item);
                    }
                }
                if (compressionMethod == CompressionMethodType.Zip)
                    ZipFile.ExtractToDirectory(zipFilePath, ExtractPath);
                Debug.WriteLine("archive extracted successfully");
                IsSuccess = true;
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "Publisher DeCompression Failed");
            }
            return IsSuccess;
        }

        public async Task<bool> CompressBackup(CompressionMethodType compressionMethod = CompressionMethodType.Zip, bool includeParent = false, CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            IsSuccess = false;
            string backupPath = Path.Combine($"{ConfigurationManager.AppSettings["BackupPath"]}");
            // if backup path is'nt valid or exist break operation and display a warn message to user
            if (!Directory.Exists(backupPath))
            {
                MessageBox.Show("Backup path not found. please set Valid Backup Path in Application Settings", "No valid path", MessageBoxButton.OK);
                return IsSuccess;
            }
            string zipFilePath = string.Empty;
            string backupArchivePath = DateTime.Now.ToString("yyyyMMdd_hhmm");
            string tempServiceBackupPath = Path.Combine(backupPath, ServiceInfo.Name, $"{ServiceInfo.Name}_backup{backupArchivePath}");
            // get executable service directory
            string servicePath = Path.Combine(Directory.GetParent(ServiceInfo.ServiceAssembliesPath).FullName);
            try
            {
                // get all files in service path
                var allDires = Directory.GetDirectories(Directory.GetParent(ServiceInfo.ServiceAssembliesPath).FullName, "*", SearchOption.AllDirectories);
                var allFiles = Directory.GetFiles(Directory.GetParent(ServiceInfo.ServiceAssembliesPath).FullName, "*.*", SearchOption.AllDirectories);
                var filesList = allFiles.ToList();
                //find backup files that stored earlier to ignore in backup
                var ignoreFiles = filesList.Where(x => x.Contains(".zip")).ToList();
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
                        ZipFile.CreateFromDirectory(Path.Combine(backupPath, ServiceInfo.Name, $"{ServiceInfo.Name}_backup{backupArchivePath}"), zipFilePath, CompressionLevel.Optimal, includeParent);
                        IsSuccess = true;
                        Directory.Delete(Path.Combine(backupPath, ServiceInfo.Name, $"{ServiceInfo.Name}_backup{backupArchivePath}"), true);
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

        public void Dispose()
        {

        }
    }
}
