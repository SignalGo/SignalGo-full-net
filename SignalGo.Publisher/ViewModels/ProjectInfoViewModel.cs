using System;
using System.IO;
using System.Linq;
using System.Text;
using MvvmGo.Commands;
using System.Threading;
using MvvmGo.ViewModels;
using System.Diagnostics;
using SignalGo.Shared.Log;
using System.Windows.Forms;
using System.Threading.Tasks;
using SignalGo.Publisher.Views;
using SignalGo.Publisher.Models;
using SignalGo.Publisher.Engines.Commands;
using SignalGo.Publisher.Engines.Interfaces;
using System.Collections.ObjectModel;
using SignalGo.Publisher.Shared.Models;
using SignalGo.Publisher.Services;
using ServerManagerService.StreamServices;
using System.Windows.Media.Imaging;
using SignalGo.Publisher.Models.Extra;
using System.Collections.Generic;
using SignalGo.Publisher.Extensions;

namespace SignalGo.Publisher.ViewModels
{
    /// <summary>
    /// View Model Logics For ProjectInfo Page
    /// </summary>
    public class ProjectInfoViewModel : BaseViewModel
    {
        /// <summary>
        /// viewmodel information of project
        /// </summary>
        public ProjectInfoViewModel()
        {
            //IsChangeBusyWhenCommandExecute = true;
            SaveIgnoreFileListCommand = new Command(SaveIgnoreFileList);
            SaveChangeCommand = new Command(SaveChanges);
            BuildCommand = new Command(Build);
            BrowseProjectPathCommand = new Command(BrowseProjectPath);
            BrowseAssemblyPathCommand = new Command(BrowseAssemblyPath);
            ClearServerFileListCommand = new EventCommand(ClearFileList);
            GetServerScreenShotCommand = new TaskCommand(CaptureApplicationProcess, () => !GetServerScreenShotCommand.IsBusy);
            CancellationCommand = new Command(CancelCommands, () => !IsBusy);
            DeleteCommand = new Command(DeleteProject);
            OpenProjectFolderCommand = new Command(OpenProjectFolder);
            RunTestsCommand = new Command(RunTests);
            RunCommandsCommand = new TaskCommand(RunCommands,
                () => !RunCommandsCommand.IsBusy);
            RestorePackagesCommand = new Command(RestorePackages);
            RemoveIgnoredServerFileCommand = new Command<IgnoreFileInfo>(RemoveIgnoredServerFile);
            RemoveIgnoredFileCommand = new Command<IgnoreFileInfo>(RemoveIgnoredFile);
            AddIgnoreClientFileCommand = new Command(AddIgnoreClientFile);
            AddIgnoreServerFileCommand = new Command(AddIgnoreServerFile);
            RemoveCommand = new Command<ICommand>((x) =>
            {
                ProjectInfo.Commands.Remove(x);
            });

            RetryCommand = new TaskCommand<ICommand>(async (x) =>
            {
                await x.Run(CancellationToken, ProjectInfo.Name);
            }, (x) => !RetryCommand.IsBusy);

            ToDownCommand = new Command<ICommand>(MoveCommandLower);
            ToUpCommand = new Command<ICommand>(MoveCommandUpper);
            GitPullCommand = new Command(GitPull);
            ClearLogsCommand = new TaskCommand(ClearLogs, () => !ClearLogsCommand.IsBusy);
            PublishCommand = new Command(PublishToServers);
            FetchFilesCommand = new TaskCommand(FetchFiles, () => !FetchFilesCommand.IsBusy);

            LoadFileCommmand = new TaskCommand<string>(LoadFileDataFromServer);

            UploadFileCommmand = new TaskCommand(UploadFileDataFromServer, () => !UploadFileCommmand.IsBusy);
            RestartServiceCommand = new TaskCommand(RestartService, () => !RestartServiceCommand.IsBusy);
            StopServiceCommand = new TaskCommand(StopService, () => !StopServiceCommand.IsBusy);
            StartServiceCommand = new TaskCommand(StartService, () => !StartServiceCommand.IsBusy);
            //InitializeCommands();
        }

        /// <summary>
        /// browse directory for path
        /// </summary>
        private void BrowseAssemblyPath()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = folderBrowserDialog.SelectedPath;
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                this.ProjectInfo.ProjectAssembliesPath = folderBrowserDialog.SelectedPath;
                SettingInfo.SaveSettingInfo();
            }
        }

        private void BrowseProjectPath()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = folderBrowserDialog.SelectedPath;
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                this.ProjectInfo.ProjectPath = folderBrowserDialog.SelectedPath;
                SettingInfo.SaveSettingInfo();
            }
        }

        /// <summary>
        /// save changes/edit's in project info
        /// </summary>
        private void SaveChanges()
        {
            var gu = Guid.Empty;
            if (Guid.TryParse(ProjectInfo.ProjectKey.ToString(), out gu))
                ProjectInfo.ProjectKey = gu;
            SettingInfo.SaveSettingInfo();
            MessageBox.Show("Change's Saved Successfully!", "Edit Project", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        public static CancellationToken CancellationToken = CancellationTokenSource.Token;

        /// <summary>
        /// move a command lower/down in Commands Queue List
        /// </summary>
        /// <param name="x"></param>
        public void MoveCommandLower(ICommand x)
        {
            var index = ProjectInfo.Commands.IndexOf(x);
            if (index + 1 != ProjectInfo.Commands.Count())
                ProjectInfo.Commands.Move(index + 1, index);
        }

        /// <summary>
        /// move a command top/upper in Commands Queue List
        /// </summary>
        /// <param name="x"></param>
        public void MoveCommandUpper(ICommand x)
        {
            var index = ProjectInfo.Commands.IndexOf(x);
            if (index != 0)
                ProjectInfo.Commands.Move(index - 1, index);
        }

        #region |MVVM Commands Used In ProjectInfo Page|
        public Command SaveChangeCommand { get; set; }
        public Command BrowseProjectPathCommand { get; set; }
        public Command BrowseAssemblyPathCommand { get; set; }
        public Command OpenProjectFolderCommand { get; set; }
        public Command BuildCommand { get; set; }
        public Command CancellationCommand { get; set; }
        public Command SaveIgnoreFileListCommand { get; set; }
        public Command<ICommand> RemoveCommand { get; set; }
        public Command<IgnoreFileInfo> RemoveIgnoredFileCommand { get; set; }
        public Command<IgnoreFileInfo> RemoveIgnoredServerFileCommand { get; set; }
        public Command AddIgnoreClientFileCommand { get; set; }
        public Command AddIgnoreServerFileCommand { get; set; }
        public TaskCommand<ICommand> RetryCommand { get; set; }
        public Command<ICommand> ToDownCommand { get; set; }
        public Command<ICommand> ToUpCommand { get; set; }
        public TaskCommand<string> LoadFileCommmand { get; set; }
        public TaskCommand UploadFileCommmand { get; set; }
        public TaskCommand StartServiceCommand { get; set; }
        public TaskCommand StopServiceCommand { get; set; }
        public TaskCommand RestartServiceCommand { get; set; }

        public EventCommand ClearServerFileListCommand { get; set; }
        public TaskCommand FetchFilesCommand { get; set; }
        public TaskCommand GetServerScreenShotCommand { get; set; }
        /// <summary>
        /// run a custome command/expression
        /// </summary>
        public TaskCommand RunCommandsCommand { get; set; }
        public Command RunTestsCommand { get; set; }
        //public string CmdLogs { get; set; }
        /// <summary>
        /// restore/update nuget packages
        /// </summary>
        public Command RestorePackagesCommand { get; set; }
        public Command GitPullCommand { get; set; }
        public Command PublishCommand { get; set; }
        #endregion

        public ObservableCollection<ServerInfo> Servers
        {
            get
            {
                return CurrentServerSettingInfo.ServerInfo;
            }
        }
        ServerInfo _SelectedServerInfo;
        public ServerInfo SelectedServerInfo
        {
            get
            {
                return _SelectedServerInfo;
            }
            set
            {
                _SelectedServerInfo = value;
                OnPropertyChanged(nameof(SelectedServerInfo));
            }
        }
        private string _fileContent;
        public string SelectedServerFile { get; set; }
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
        private ProjectInfo _ProjectInfo;

        /// <summary>
        /// a property instance of ProjectInfo Model
        /// </summary>
        public ProjectInfo ProjectInfo
        {
            get
            {
                return _ProjectInfo;
            }
            set
            {
                _ProjectInfo = value;
                OnPropertyChanged(nameof(ProjectInfo));
                OnPropertyChanged(nameof(ServerLogs));
                OnPropertyChanged(nameof(ManagementLogs));
                OnPropertyChanged(nameof(BuilderLogs));
            }
        }

        BitmapSource _ServerScreenCapture;
        public BitmapSource ServerScreenCapture
        {
            get
            {
                return _ServerScreenCapture;
            }
            set
            {
                _ServerScreenCapture = value;
                OnPropertyChanged(nameof(ServerScreenCapture));
            }
        }

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

        public ObservableCollection<LogInfo> ManagementLogs
        {
            get
            {
                if (!string.IsNullOrEmpty(ProjectInfo?.Name))
                {
                    LogModule.TryGetLogs(ProjectInfo.Name, SectorType.Management, out ObservableCollection<LogInfo> _ProjectInfoLogs);
                    return _ProjectInfoLogs;
                }
                return null;
            }
        }

        public ObservableCollection<LogInfo> BuilderLogs
        {
            get
            {
                if (!string.IsNullOrEmpty(ProjectInfo?.Name))
                {
                    LogModule.TryGetLogs(ProjectInfo.Name, SectorType.Builder, out ObservableCollection<LogInfo> _ProjectInfoLogs);
                    return _ProjectInfoLogs;
                }
                return null;
            }
        }

        public ObservableCollection<LogInfo> ServerLogs
        {
            get
            {
                if (!string.IsNullOrEmpty(ProjectInfo?.Name))
                {
                    LogModule.TryGetLogs(ProjectInfo.Name, SectorType.Server, out ObservableCollection<LogInfo> _ProjectInfoLogs);
                    return _ProjectInfoLogs;
                }
                return null;
            }
        }

        private ProjectInfo _SelectedCommandInfo;
        public ProjectInfo SelectedCommandInfo
        {
            get
            {
                return _SelectedCommandInfo;
            }
            set
            {
                _SelectedCommandInfo = value;
                OnPropertyChanged(nameof(SelectedCommandInfo));
                ProjectInfoPage page = new ProjectInfoPage();
                ProjectInfoViewModel vm = page.DataContext as ProjectInfoViewModel;
                vm.ProjectInfo = value;
                //MainFrame.Navigate(page);
            }
        }
        public Command DeleteCommand { get; set; }
        public TaskCommand ClearLogsCommand { get; set; }

        /// <summary>
        /// Delete Project From publisher if user confirm it
        /// </summary>
        private void DeleteProject()
        {
            if (MessageBox.Show("Are you srue you want to delete this project?", "Delete Project", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            SettingInfo.Current.ProjectInfo.Remove(ProjectInfo);
            SettingInfo.SaveSettingInfo();
            ProjectManagerWindowViewModel.MainFrame.GoBack();
        }
        /// <summary>
        /// Get Compile and output for Publish
        /// push/update projects and related assemblies to Servers
        /// </summary>
        private void PublishToServers()
        {

            // add git command/commands to commands queue list
            if (!ProjectInfo.Commands.Any(x => x is GitCommandInfo))
                ProjectInfo.AddCommand(new GitCommandInfo());
            // add compiler command to commands list
            if (!ProjectInfo.Commands.Any(x => x is BuildCommandInfo))
                ProjectInfo.AddCommand(new BuildCommandInfo());
            // add TestsRunner Command if not exist in commands list
            if (!ProjectInfo.Commands.Any(x => x is TestsCommandInfo))
                ProjectInfo.AddCommand(new TestsCommandInfo());
            // add publish command with peroject data|key
            if (!ProjectInfo.Commands.Any(x => x is PublishCommandInfo))
            {
                ProjectInfo.AddCommand(new PublishCommandInfo(new ServiceContract
                {
                    Name = ProjectInfo.Name,
                    ServiceKey = ProjectInfo.ProjectKey
                }));
            }
        }

        private async Task ClearLogs()
        {
            await LogModule.ClearLogs(ProjectInfo.Name, SectorType.Builder, true);
            await LogModule.ClearLogs(ProjectInfo.Name, SectorType.Server, true);
            await LogModule.ClearLogs(ProjectInfo.Name, SectorType.Management, true);

        }

        /// <summary>
        /// Cancel/Break All Commands and Queued Commands
        /// </summary>
        private void GitPull()
        {
            if (!ProjectInfo.Commands.Any(x => x is GitCommandInfo))
                ProjectInfo.AddCommand(new GitCommandInfo());
        }
        /// <summary>
        /// request cancellation for all queued and running command's
        /// </summary>
        private void CancelCommands()
        {
            IsBusy = true;
            try
            {
                CancellationTokenSource.Cancel();
                CancellationToken.ThrowIfCancellationRequested();
            }
            catch (Exception ex)
            {
                LogModule.AddLog(ProjectInfo.Name, SectorType.Builder, "Task has been cancelled By Cancellation Command", LogTypeEnum.System);
                AutoLogger.Default.LogError(ex, "Cancel Commands");
            }
            finally
            {
                // Generate new token
                CancellationTokenSource = new CancellationTokenSource();
                CancellationToken = new CancellationToken(false);
                CancellationToken = CancellationTokenSource.Token;
                IsBusy = false;
            }
        }

        /// <summary>
        /// Add Specified File to upload ignore list settings
        /// </summary>
        private void AddIgnoreClientFile()
        {
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.Multiselect = true;
            if (fileDialog.ShowDialog().GetValueOrDefault())
            {
                if (!ProjectInfo.IgnoredFiles.Any(x => x.FileName == fileDialog.SafeFileName) && fileDialog.CheckFileExists)
                {
                    ProjectInfo.IgnoredFiles.Add(new IgnoreFileInfo() { FileName = fileDialog.SafeFileName, IsEnabled = true });
                }
            }
        }
        /// <summary>
        /// add specified file to Server Ignore Update List settings
        /// </summary>
        public void AddIgnoreServerFile()
        {
            if (!ProjectInfo.ServerIgnoredFiles.Any(x => x.FileName == IgnoreServerFileName) && !string.IsNullOrEmpty(IgnoreServerFileName))
            {
                ProjectInfo.ServerIgnoredFiles.Add(new IgnoreFileInfo() { FileName = IgnoreServerFileName, IsEnabled = true });
                IgnoreServerFileName = string.Empty;
            }
            else
                System.Windows.MessageBox.Show("Invalid Input Or Already Exist!", "Data Validation Error", System.Windows.MessageBoxButton.OK);
        }
        /// <summary>
        /// Save all ignore file settings to user settings
        /// </summary>
        private void ClearFileList()
        {
            FileContent = string.Empty;
            ProjectInfo.ServerFiles.Clear();
        }

        /// <summary>
        /// open the project folder path
        /// </summary>
        private void OpenProjectFolder()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = ProjectInfo.ProjectPath
                });
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "open project folder");
                MessageBox.Show("error", ex.Message);
            }
        }
        /// <summary>
        /// save files that defined as ignore
        /// </summary>
        private void SaveIgnoreFileList()
        {
            try
            {
                var clientIgnoreList = CurrentProjectSettingInfo.ProjectInfo.Select(x => x.IgnoredFiles).ToList();
                var serverIgnoreList = CurrentProjectSettingInfo.ProjectInfo.Select(x => x.ServerIgnoredFiles).ToList();
                clientIgnoreList.Add(ProjectInfo.IgnoredFiles);
                serverIgnoreList.Add(ProjectInfo.ServerIgnoredFiles);
                clientIgnoreList.Clear();
                serverIgnoreList.Clear();
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "save ignore file list");
            }
            finally
            {
                SettingInfo.SaveSettingInfo();
            }
        }
        /// <summary>
        /// remove specified file from server ignore list
        /// </summary>
        /// <param name="name"></param>
        private void RemoveIgnoredServerFile(IgnoreFileInfo ignoreFileInfo)
        {
            ProjectInfo.ServerIgnoredFiles.Remove(ProjectInfo.ServerIgnoredFiles.FirstOrDefault(x => x == ignoreFileInfo));
            //SaveIgnoreFileList();
        }

        /// remove specified file from client(publisher) ignore list
        private void RemoveIgnoredFile(IgnoreFileInfo ignoreFileInfo)
        {
            ProjectInfo.IgnoredFiles.Remove(ProjectInfo.IgnoredFiles.FirstOrDefault(x => x == ignoreFileInfo));
            //SaveIgnoreFileList();

        }
        /// <summary>
        /// Add Compile Command To Commands List
        /// </summary>
        private void Build()
        {
            if (!ProjectInfo.Commands.Any(x => x is BuildCommandInfo))
                ProjectInfo.AddCommand(new BuildCommandInfo());
        }
        /// <summary>
        /// Init Commands to Run in Queue
        /// </summary>
        private async Task RunCommands()
        {
            if (!ProjectInfo.Commands.HasValue())
            {
                MessageBox.Show("No Command Available To Run!");
                return;
            }
            bool locked = false;
            ServerInfo.Servers.Clear();
            try
            {
                // remove old command runner logs (build's, test's, ...)
                File.Delete(CurrentUserSettingInfo.UserSettings.CommandRunnerLogsPath);

                bool hasPublishCommand = ProjectInfo.Commands.Any(x => x is PublishCommandInfo);

                if (hasPublishCommand && !CurrentServerSettingInfo.ServerInfo.Any(x => x.IsChecked))
                {
                    System.Windows.MessageBox.Show("No Server Selected", "Specify Remote Target", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
                else
                {
                    if (ProjectManagerWindowViewModel.This.IsAccessControlUnlocked)
                    {
                        ProjectManagerWindowViewModel.This.IsAccessControlUnlocked = false;
                        locked = true;
                    }
                    if (CurrentUserSettingInfo.UserSettings.RunAuthenticateAtFirst)
                    {
                        foreach (ServerInfo item in CurrentServerSettingInfo.ServerInfo.Where(s => s.IsChecked))
                        {
                            item.HasAccess();
                        }
                    }
                    await Task.Run(async () =>
                    {
                        await RunCustomCommand(ProjectInfo, CancellationToken);
                    }, CancellationToken);
                }
            }
            catch (Exception ex)
            {
                LogModule.AddLog(ProjectInfo.Name, SectorType.Builder, $"{ex.Message} - RunCommands ProjectInfo", LogTypeEnum.Error);
                AutoLogger.Default.LogError(ex, "RunCommands ProjectInfo");
            }
            finally
            {
                if (locked)
                    ProjectManagerWindowViewModel.This.IsAccessControlUnlocked = true;
            }
        }

        /// <summary>
        /// restore (install/fix) nuget packages
        /// </summary>
        private void RestorePackages()
        {
            if (!ProjectInfo.Commands.Any(x => x is RestoreCommandInfo))
                ProjectInfo.AddCommand(new RestoreCommandInfo());
        }

        /// <summary>
        /// Run Project Test Cases
        /// </summary>
        private void RunTests()
        {
            if (!ProjectInfo.Commands.Any(x => x is BuildCommandInfo))
                ProjectInfo.AddCommand(new BuildCommandInfo());
            if (!ProjectInfo.Commands.Any(x => x is TestsCommandInfo))
                ProjectInfo.AddCommand(new TestsCommandInfo());
        }

        public static async Task<bool> RunCustomCommand(ProjectInfo ProjectInfo, CancellationToken cancellationToken)
        {
            try
            {
                if (CancellationToken.IsCancellationRequested)
                {
                    return false;
                }
                await ProjectInfo.RunCommands(CancellationToken);
            }
            catch
            {

            }
            return true;
        }

        /// <summary>
        /// stop a service process on server
        /// </summary>
        public async Task StopService()
        {
            try
            {
                ServerInfo.Servers.Clear();
                List<ServerInfo> servers = CurrentServerSettingInfo.ServerInfo.Where(x => x.IsChecked).ToList();

                if (servers.HasValue())
                {
                    for (int i = 0; i < servers.Count; i++)
                    {
                        try
                        {
                            var provider = await PublisherServiceProvider
                                .Initialize(servers[i], ProjectInfo.Name);
                            if (provider.HasValue())
                            {
                                if (await provider.ServerManagerService
                                                                .StopServiceAsync(ProjectInfo.ProjectKey))
                                {
                                    LogModule.AddLog(ProjectInfo.Name, SectorType.Management, $"Service {ProjectInfo.Name} Stopped Successfully", DateTime.Now.ToLongTimeString(), LogTypeEnum.Info);
                                }
                                else
                                {
                                    LogModule.AddLog(ProjectInfo.Name, SectorType.Management, $"Couldn't Stop Service { ProjectInfo.Name}", DateTime.Now.ToLongTimeString(), LogTypeEnum.Warning);
                                }
                            }
                            else { break; }
                        }
                        catch (Exception ex)
                        {
                            LogModule.AddLog(ProjectInfo.Name, SectorType.Management, $" Operation Time Out On Server { servers[i].ServerName}", DateTime.Now.ToLongTimeString(), LogTypeEnum.Error);
                            AutoLogger.Default.LogError(ex, "ProjectInfo- StopService");
                        }
                    } // end for
                } // end if
                else
                {

                    System.Windows.MessageBox.Show("No Server Selected", "Specify Remote Target", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                LogModule.AddLog(ProjectInfo.Name, SectorType.Management, $"Couldn't Get Service { ProjectInfo.Name}", DateTime.Now.ToLongTimeString(), LogTypeEnum.Error);
                AutoLogger.Default.LogError(ex, "StopService, PublisherServiceProviderInitialize");
            }
        }

        /// <summary>
        /// start a service process on server
        /// </summary>
        public async Task StartService()
        {
            try
            {
                ServerInfo.Servers.Clear();
                List<ServerInfo> servers = CurrentServerSettingInfo.ServerInfo.Where(x => x.IsChecked).ToList();

                if (servers.HasValue())
                {
                    for (int i = 0; i < servers.Count; i++)
                    {
                        try
                        {
                            var provider = await PublisherServiceProvider
                                .Initialize(servers[i], ProjectInfo.Name);
                            if (provider.HasValue())
                            {
                                if (await provider.ServerManagerService
                                                                .StartServiceAsync(ProjectInfo.ProjectKey))
                                {
                                    LogModule.AddLog(ProjectInfo.Name, SectorType.Management, $"Service { ProjectInfo.Name} Started Successfully", DateTime.Now.ToLongTimeString(), LogTypeEnum.Info);
                                }
                                else
                                {
                                    LogModule.AddLog(ProjectInfo.Name, SectorType.Management, $"Couldn't Start Service { ProjectInfo.Name}", DateTime.Now.ToLongTimeString(), LogTypeEnum.Warning);
                                }
                            }
                            else { break; }
                        }
                        catch (Exception ex)
                        {
                            LogModule.AddLog(ProjectInfo.Name, SectorType.Management, $" Operation Time Out On Server { servers[i].ServerName}", DateTime.Now.ToLongTimeString(), LogTypeEnum.Error);
                            AutoLogger.Default.LogError(ex, "ProjectInfo-RestartService");
                        }
                    } // end for
                } // end if
                else
                {

                    System.Windows.MessageBox.Show("No Server Selected", "Specify Remote Target", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                LogModule.AddLog(ProjectInfo.Name, SectorType.Management, $"Couldn't Get Service { ProjectInfo.Name}", DateTime.Now.ToLongTimeString(), LogTypeEnum.Error);
                AutoLogger.Default.LogError(ex, "StartService, PublisherServiceProviderInitialize");
            }
        }

        /// <summary>
        /// restart a service on server
        /// </summary>
        public async Task RestartService()
        {
            try
            {
                ServerInfo.Servers.Clear();
                List<ServerInfo> servers = CurrentServerSettingInfo.ServerInfo.Where(x => x.IsChecked).ToList();

                if (servers.HasValue())
                {
                    for (int i = 0; i < servers.Count; i++)
                    {
                        try
                        {
                            var provider = await PublisherServiceProvider
                                .Initialize(servers[i], ProjectInfo.Name);
                            if (provider.HasValue())
                            {
                                if (await provider.ServerManagerService
                                                                .RestartServiceAsync(ProjectInfo.ProjectKey, false))
                                {
                                    LogModule.AddLog(ProjectInfo.Name, SectorType.Management, $"Service { ProjectInfo.Name} Restarted Successfully", DateTime.Now.ToLongTimeString(), LogTypeEnum.Info);
                                }
                                else
                                {
                                    LogModule.AddLog(ProjectInfo.Name, SectorType.Management, $"Couldn't Restart Service { ProjectInfo.Name}", DateTime.Now.ToLongTimeString(), LogTypeEnum.Warning);
                                }
                            }
                            else { break; }
                        }
                        catch (Exception ex)
                        {
                            LogModule.AddLog(ProjectInfo.Name, SectorType.Management, $" Operation Time Out On Server { servers[i].ServerName}", DateTime.Now.ToLongTimeString(), LogTypeEnum.Error);
                            AutoLogger.Default.LogError(ex, "ProjectInfo-RestartService");
                        }
                    } // end for
                } // end if
                else
                {

                    System.Windows.MessageBox.Show("No Server Selected", "Specify Remote Target", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                LogModule.AddLog(ProjectInfo.Name, SectorType.Management, $"Couldn't Get Service { ProjectInfo.Name}", DateTime.Now.ToLongTimeString(), LogTypeEnum.Error);
                AutoLogger.Default.LogError(ex, "RestartService, PublisherServiceProviderInitialize");
            }
        }

        /// <summary>
        /// Fetch service files list from server
        /// </summary>
        /// <returns></returns>
        public async Task FetchFiles()
        {
            IsFetch = true;
            ServerScreenCapture = null;
            List<string> files = new List<string>();
            try
            {
                var provider = await PublisherServiceProvider.Initialize(SelectedServerInfo, ProjectInfo.Name);
                if (provider.HasValue())
                {
                    files = await provider.FileManagerService.GetTextFilesAsync(ProjectInfo.ProjectKey);
                    RunOnUIAction(() =>
                    {
                        ProjectInfo.ServerFiles.Clear();
                        for (int i = 0; i < files.Count; i++)
                        {
                            ProjectInfo.ServerFiles.Add(files[i]);
                        }
                    });
                }
                else
                {
                    System.Windows.MessageBox.Show("Could'nt Find Provider!");
                    return;
                }

            }
            catch (Exception ex)
            {
                LogModule.AddLog(ProjectInfo.Name, SectorType.Management, "Can't Fetch Files From server!", DateTime.Now.ToLongTimeString(), LogTypeEnum.Error);
                System.Windows.MessageBox.Show("Can't Fetch Files From server!");
                AutoLogger.Default.LogError(ex, "Fetch Files From server");
            }
        }
        /// <summary>
        /// Capture Service Process Screenshot from server manager (by service key)
        /// </summary>
        /// <returns></returns>
        private async Task CaptureApplicationProcess()
        {
            IsFetch = false;
            MemoryStream memoryStream = new MemoryStream();
            try
            {
                var provider = await PublisherServiceProvider.Initialize(SelectedServerInfo, ProjectInfo.Name);
                if (provider.HasValue())
                {
                    ServerManagerStreamService serverManagerStreamService = new ServerManagerStreamService(provider.CurrentClientProvider);
                    // Call CaptureApplicationProcess Service from server manager and using porojectKey(serviceKey)
                    using var stream = await serverManagerStreamService.CaptureApplicationProcessAsync(ProjectInfo.ProjectKey);
                    var lengthWrite = 0;
                    while (lengthWrite != stream.Length)
                    {
                        byte[] bufferBytes = new byte[1024];
                        int readCount = await stream.Stream.ReadAsync(bufferBytes, bufferBytes.Length);
                        if (readCount <= 0)
                            break;
                        await memoryStream.WriteAsync(bufferBytes, 0, readCount);
                        lengthWrite += readCount;
                    }

                    RunOnUIAction(() =>
                    {
                        try
                        {
                            ProjectInfo.ServerFiles.Clear();
                            PngBitmapDecoder decoder = new PngBitmapDecoder(memoryStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                            ServerScreenCapture = decoder.Frames[0];
                        }
                        catch (Exception ex)
                        {
                            AutoLogger.Default.LogError(ex, "CaptureApplicationProcess, RunOnUIAction");
                        }
                    });
                }
                else
                {
                    LogModule.AddLog(ProjectInfo.Name, SectorType.Management, "Sorry, Couldn't connect to server!", DateTime.Now.ToLongTimeString(), LogTypeEnum.Error);
                    System.Windows.MessageBox.Show("Sorry, Couldn't connect to server and Get ScreenShot");
                    return;
                }
            }
            catch (Exception ex)
            {
                //IsBusy = false;
                System.Windows.MessageBox.Show("Can't connect to server to Get ScreenShot");
                LogModule.AddLog(ProjectInfo.Name, SectorType.Management, "Couldn't connect to server for Get ScreenShot", DateTime.Now.ToLongTimeString(), LogTypeEnum.Error);
                AutoLogger.Default.LogError(ex, "CaptureApplicationProcess");
            }
            finally
            {
                await Task.Delay(100);
                await memoryStream.DisposeAsync();
            }
        }

        /// <summary>
        /// Load Selected service file from server
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private async Task LoadFileDataFromServer(string filePath)
        {
            try
            {
                SelectedServerFile = filePath;
                var provider = await PublisherServiceProvider.Initialize(SelectedServerInfo, ProjectInfo.Name);
                if (provider == null)
                {
                    System.Windows.MessageBox.Show("can't load the file!");
                    return;
                }
                ServerManagerStreamService serverManagerStreamService = new ServerManagerStreamService(provider.CurrentClientProvider);
                var stream = await serverManagerStreamService.DownloadFileDataAsync(filePath, ProjectInfo.ProjectKey);
                var lengthWrite = 0;
                using var memoryStream = new MemoryStream();
                while (lengthWrite != stream.Length)
                {
                    byte[] bufferBytes = new byte[1024];
                    int readCount = await stream.Stream.ReadAsync(bufferBytes, bufferBytes.Length);
                    if (readCount <= 0)
                        break;
                    await memoryStream.WriteAsync(bufferBytes, 0, readCount);
                    lengthWrite += readCount;
                }

                FileContent = Encoding.UTF8.GetString(memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                LogModule.AddLog(ProjectInfo.Name, SectorType.Management, "can't LoadFile Data FromServer!", DateTime.Now.ToLongTimeString(), LogTypeEnum.Error);
                AutoLogger.Default.LogError(ex, "LoadFileDataFromServer");
            }
        }

        /// <summary>
        /// upload/save selected file to server
        /// </summary>
        /// <returns></returns>
        private async Task UploadFileDataFromServer()
        {
            try
            {
                var provider = await PublisherServiceProvider.Initialize(SelectedServerInfo, ProjectInfo.Name);
                if (provider.HasValue())
                {
                    ServerManagerStreamService serverManagerStreamService = new ServerManagerStreamService(provider.CurrentClientProvider);
                    using MemoryStream memoryStream = new MemoryStream();
                    if (string.IsNullOrEmpty(FileContent))
                        return;
                    await memoryStream.WriteAsync(Encoding.UTF8.GetBytes(FileContent));
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    bool result = await serverManagerStreamService.SaveFileDataAsync(new SignalGo.Shared.Models.StreamInfo<string>(memoryStream)
                    {
                        Length = memoryStream.Length,
                        Data = SelectedServerFile
                    }, ProjectInfo.ProjectKey);
                }
                else
                {
                    LogModule.AddLog(ProjectInfo.Name, SectorType.Management, "Couldn't connect to server for Upload the file", DateTime.Now.ToLongTimeString(), LogTypeEnum.Error);
                    MessageBox.Show("Could'nt Upload!");
                    return;
                }
            }
            catch (Exception ex)
            {
                LogModule.AddLog(ProjectInfo.Name, SectorType.Management, "can't Upload File Data!", DateTime.Now.ToLongTimeString(), LogTypeEnum.Error);
                AutoLogger.Default.LogError(ex, "UploadFileDataFromServer");
            }
        }


        public UserSettingInfo CurrentUserSettingInfo
        {
            get
            {
                return UserSettingInfo.Current;
            }
        }
        /// <summary>
        /// setting's and all data defiend/stored in project db
        /// </summary>
        public SettingInfo CurrentProjectSettingInfo
        {
            get
            {
                return SettingInfo.Current;
            }
        }

        public ServerSettingInfo CurrentServerSettingInfo
        {
            get
            {
                return ServerSettingInfo.CurrentServer;
            }
        }
        public CommandSettingInfo CurrentCommandSettingInfo
        {
            get
            {
                return CommandSettingInfo.Current;
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
                OnPropertyChanged(nameof(ServerInfo));
            }
        }

        private string _IgnoreServerFileName;
        public string IgnoreServerFileName
        {
            get { return _IgnoreServerFileName; }
            set
            {
                _IgnoreServerFileName = value;
                OnPropertyChanged(nameof(IgnoreServerFileName));
            }
        }


    }
}
