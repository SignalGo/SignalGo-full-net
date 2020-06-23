using MvvmGo.Commands;
using MvvmGo.ViewModels;
using SignalGo.ServiceManager.Core.Helpers;
using SignalGo.ServiceManager.Core.Models;
using SignalGo.Shared.Log;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SignalGo.ServiceManager.Core.BaseViewModels
{
    public class ServerInfoBaseViewModel : BaseViewModel
    {
        public ServerInfoBaseViewModel()
        {
            StartCommand = new Command(Start);
            StopCommand = new Command(Stop);
            BrowsePathCommand = new Command(BrowsePath);
            ChangeCommand = new Command(Change);
            DeleteCommand = new Command(Delete);
            ClearLogCommand = new Command(ClearLog);
            CopyCommand = new Command<TextLogInfo>(Copy);
            UploadFileCommand = new Command(() =>
            {
                IsBusy = true;
                UploadFileCommand.ValidateCanExecute();
                _ = EditFileData();
            }, () => !IsBusy);
            FetchFilesCommand = new Command(() =>
            {
                IsFetch = true;
                IsBusy = true;
                FetchFilesCommand.ValidateCanExecute();
                _ = FetchFiles();
            }, () => !IsBusy);
            LoadFileCommmand = new Command<string>((filePath) =>
            {
                IsBusy = true;
                LoadFileCommmand.ValidateCanExecute(filePath);
                _ = LoadFileDataFromServer(filePath);
            }, (x) => !IsBusy);
        }
        public static CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        public static CancellationToken CancellationToken = CancellationTokenSource.Token;
        public Command StartCommand { get; set; }
        public Command StopCommand { get; set; }
        public Command ChangeCommand { get; set; }
        public Command BrowsePathCommand { get; set; }
        public Command DeleteCommand { get; set; }
        public Command ClearLogCommand { get; set; }
        public Command UploadFileCommand { get; set; }
        public Command FetchFilesCommand { get; set; }
        public Command<string> LoadFileCommmand { get; set; }
        public Command<TextLogInfo> CopyCommand { get; set; }

        public string _ServiceMemoryUsage;
        public string ServiceMemoryUsage
        {
            get
            {
                return $"{_ServiceMemoryUsage}";
            }
            set
            {
                _ServiceMemoryUsage = value;
                OnPropertyChanged(nameof(ServiceMemoryUsage));
            }
        }

        ServerInfo _ServerInfo;
        public ServerInfo ServerInfo
        {
            get
            {
                return _ServerInfo;
            }

            set
            {
                _ServerInfo = value;
                ServerDetailsManager.Enable(value);
                OnPropertyChanged(nameof(ServerInfo));
            }
        }

        protected virtual void Delete()
        {
            SettingInfo.Current.ServerInfo.Remove(ServerInfo);
            SettingInfo.SaveSettingInfo();
        }

        private void Stop()
        {
            try
            {
                //CancellationTokenSource.Cancel();
                //CancellationToken.ThrowIfCancellationRequested();
            }
            catch
            {
                Debug.WriteLine($"Task in Server ({ServerInfo.Name}) has been cancelled With Cancellation Token.");
            }
            StopServer(ServerInfo);
        }

        private void Start()
        {
            StartServer(ServerInfo);
        }
        private void Change()
        {
            var gu = Guid.Empty;
            if (Guid.TryParse(ServerInfo.ServerKey.ToString(), out gu))
            {
                ServerInfo.ServerKey = gu;
            }
            SettingInfo.SaveSettingInfo();
        }
        protected virtual void BrowsePath()
        {

        }
        public static void Delete(ServerInfo serverInfo)
        {
            StopServer(serverInfo);
            SettingInfo.Current.ServerInfo.Remove(serverInfo);
            SettingInfo.SaveSettingInfo();
        }
        public static void Delete(string serviceName)
        {
            try
            {
                var server = SettingInfo.Current.ServerInfo.FirstOrDefault(x => x.Name == serviceName);
                StopServer(server);
                SettingInfo.Current.ServerInfo.Remove(server);
                SettingInfo.SaveSettingInfo();
            }
            catch (Exception ex)
            {

            }
        }
        public static void StartServer(ServerInfo serverInfo)
        {
            serverInfo.Start();
        }
        public static void StopServer(ServerInfo serverInfo)
        {
            serverInfo.Stop();
        }

        private void ClearLog()
        {
            //ServerInfo.Logs.Clear();
        }
        protected virtual void Copy(TextLogInfo textLogInfo)
        {
        }


        public ObservableCollection<string> ServerFiles { get; set; } = new ObservableCollection<string>();
        private string _fileContent;
        public string FileContent
        {
            get
            {
                return _fileContent;
            }
            set
            {
                _fileContent = value;
                OnPropertyChanged(nameof(FileContent));
            }
        }
        public string SelectedServerFile { get; set; }
        bool _IsFetch = false;
        public bool IsFetch
        {
            get
            {
                return _IsFetch;
            }
            set
            {
                _IsFetch = value;
                OnPropertyChanged(nameof(IsFetch));
            }
        }

        public async Task<List<string>> GetTextFiles(Guid serviceKey)
        {
            var find = SettingInfo.Current.ServerInfo.FirstOrDefault(x => x.ServerKey == serviceKey);
            if (find == null)
                throw new Exception($"Service {serviceKey} not found!");
            var directory = Path.GetDirectoryName(find.AssemblyPath);
            return Directory.GetFiles(directory).Where(x =>
            {
                var extension = Path.GetExtension(x).ToLower();
                if (extension == ".txt" || extension == ".json")
                    return true;
                return false;
            }).ToList();
        }

        private async Task LoadFileDataFromServer(string filePath)
        {
            try
            {
                SelectedServerFile = filePath;
                var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var lengthWrite = 0;
                using var memoryStream = new MemoryStream();
                while (lengthWrite != stream.Length)
                {
                    byte[] bufferBytes = new byte[1024];
                    int readCount = await stream.ReadAsync(bufferBytes, 0, bufferBytes.Length);
                    if (readCount <= 0)
                        break;
                    await memoryStream.WriteAsync(bufferBytes, 0, readCount);
                    lengthWrite += readCount;
                }

                FileContent = Encoding.UTF8.GetString(memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "LoadFileDataFromServer");
            }
            finally
            {
                IsBusy = false;
                LoadFileCommmand.ValidateCanExecute(filePath);
            }
        }
        private async Task EditFileData()
        {
            try
            {
                using MemoryStream memoryStream = new MemoryStream();
                if (string.IsNullOrEmpty(FileContent))
                    return;
                await memoryStream.WriteAsync(Encoding.UTF8.GetBytes(FileContent));
                _ = memoryStream.Seek(0, SeekOrigin.Begin);
                using FileStream fileStream = new FileStream(SelectedServerFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                fileStream.SetLength(0);
                int lengthWrite = 0;
                while (lengthWrite < memoryStream.Length)
                {
                    byte[] bufferBytes = new byte[1024 * 1024];
                    int readCount = await memoryStream.ReadAsync(bufferBytes, 0, bufferBytes.Length);
                    if (readCount <= 0)
                        break;
                    await fileStream.WriteAsync(bufferBytes, 0, readCount);
                    lengthWrite += readCount;
                }
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "EditFileData");
            }
            finally
            {
                IsBusy = false;
                UploadFileCommand.ValidateCanExecute();
            }
        }
        public async Task FetchFiles()
        {
            try
            {
                var files = await GetTextFiles(ServerInfo.ServerKey);

                //RunOnUIAction(() =>
                //{
                ServerFiles.Clear();
                foreach (var item in files)
                {
                    ServerFiles.Add(item);
                }
                //});
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "Fetch Files From server");
            }
            finally
            {
                IsBusy = false;
                FetchFilesCommand.ValidateCanExecute();
            }
        }

    }
}
