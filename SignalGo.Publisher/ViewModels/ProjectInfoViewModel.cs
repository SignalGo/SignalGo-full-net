using System;
using System.IO;
using System.Linq;
using MvvmGo.Commands;
using MvvmGo.ViewModels;
using System.Windows.Forms;
using System.Threading.Tasks;
using SignalGo.Publisher.Views;
using SignalGo.Publisher.Models;
using SignalGo.Publisher.Engines.Commands;
using SignalGo.Publisher.Engines.Interfaces;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections.ObjectModel;
using SignalGo.Shared.Log;
using SignalGo.Publisher.Shared.Models;
using SignalGo.Publisher.Services;
using SignalGo.Shared;
using ServerManagerService.StreamServices;
using SignalGo.Publisher.Views.Extra;
using SignalGo.Publisher.Engines.Models;
using System.Security.Cryptography;

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
            //ServerInfo.Servers.Clear();
            SaveIgnoreFileListCommand = new Command(SaveIgnoreFileList);
            SaveChangeCommand = new Command(SaveChanges);
            ApplyMigrationsCommand = new Command(ApplyMigrations);
            BuildCommand = new Command(Build);
            BrowseProjectPathCommand = new Command(BrowseProjectPath);
            BrowseAssemblyPathCommand = new Command(BrowseAssemblyPath);
            ClearServerFileListCommand = new EventCommand(ClearFileList);
            CancellationCommand = new Command(CancelCommands);
            DeleteCommand = new Command(Delete);
            RunTestsCommand = new Command(RunTests);
            RunCommand = new Command(RunCMD);
            RestorePackagesCommand = new Command(RestorePackages);
            RemoveIgnoredServerFileCommand = new Command<IgnoreFileInfo>(RemoveIgnoredServerFile);
            RemoveIgnoredFileCommand = new Command<IgnoreFileInfo>(RemoveIgnoredFile);
            AddIgnoreClientFileCommand = new Command(AddIgnoreClientFile);
            AddIgnoreServerFileCommand = new Command(AddIgnoreServerFile);
            RemoveCommand = new Command<ICommand>((x) =>
            {
                ProjectInfo.Commands.Remove(x);
            });

            RetryCommand = new Command<ICommand>((x) =>
            {
                Task.Run(async () =>
                {
                    await x.Run(CancellationToken);
                    await ReadCommandLog();
                });
            });
            ToDownCommand = new Command<ICommand>(MoveCommandLower);
            ToUpCommand = new Command<ICommand>(MoveCommandUpper);
            GitPullCommand = new Command(GitPull);
            PublishCommand = new Command(PublishToServers);

            FetchFilesCommand = new Command(() =>
            {
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

            UploadFileCommmand = new Command(() =>
            {
                IsBusy = true;
                UploadFileCommmand.ValidateCanExecute();
                _ = UploadFileDataFromServer();
            }, () => !IsBusy);
        }


        /// <summary>
        /// read log of excecuted commands
        /// </summary>
        /// <returns></returns>
        public async Task ReadCommandLog()
        {
            string standardOutputResult;
            StringBuilder sb = new StringBuilder();
            var logFile = await File.ReadAllTextAsync(UserSettingInfo.Current.UserSettings.CommandRunnerLogsPath);
            standardOutputResult = logFile;
            CmdLogs += standardOutputResult;
            foreach (var item in ServerInfo.ServerLogs)
            {
                sb.AppendLine(item);
            }
            ServerLogs = sb.ToString();
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
        /// save changes in project info
        /// </summary>
        private void SaveChanges()
        {
            var gu = Guid.Empty;
            if (Guid.TryParse(ProjectInfo.ProjectKey.ToString(), out gu))
                ProjectInfo.ProjectKey = gu;
            SaveIgnoreFileList();
            SettingInfo.SaveSettingInfo();
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

        /// <summary>
        /// compile and check project assemblies for build
        /// </summary>
        public Command SaveChangeCommand { get; set; }
        public Command BrowseProjectPathCommand { get; set; }
        public Command BrowseAssemblyPathCommand { get; set; }
        public Command BuildCommand { get; set; }
        public Command CancellationCommand { get; set; }
        public Command SaveIgnoreFileListCommand { get; set; }
        public Command<ICommand> RemoveCommand { get; set; }
        public Command<IgnoreFileInfo> RemoveIgnoredFileCommand { get; set; }
        public Command<IgnoreFileInfo> RemoveIgnoredServerFileCommand { get; set; }
        public Command AddIgnoreClientFileCommand { get; set; }
        public Command AddIgnoreServerFileCommand { get; set; }
        public Command<ICommand> RetryCommand { get; set; }
        public Command<ICommand> ToDownCommand { get; set; }
        public Command<ICommand> ToUpCommand { get; set; }
        public Command<string> LoadFileCommmand { get; set; }
        public Command UploadFileCommmand { get; set; }
        public string SelectedServerFile { get; set; }

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

        public EventCommand ClearServerFileListCommand { get; set; }
        public Command FetchFilesCommand { get; set; }
        /// <summary>
        /// run a custome command/expression
        /// </summary>
        public Command RunCommand { get; set; }
        public Command RunTestsCommand { get; set; }
        //public string CmdLogs { get; set; }
        /// <summary>
        /// restore/update nuget packages
        /// </summary>
        public Command RestorePackagesCommand { get; set; }

        public Command GitPullCommand { get; set; }
        public Command PublishCommand { get; set; }
        /// <summary>
        /// Execute Test Cases of Project
        /// </summary>

        string _ServerLogs;

        /// <summary>
        /// instance of ProjectInfo Model
        /// </summary>
        public string ServerLogs
        {
            get
            {
                return _ServerLogs;
            }
            set
            {
                _ServerLogs = value;
                OnPropertyChanged(nameof(ServerLogs));
            }
        }

        string _CmdLogs;

        /// <summary>
        /// instance of ProjectInfo Model
        /// </summary>
        public string CmdLogs
        {
            get
            {
                return _CmdLogs;
            }
            set
            {
                _CmdLogs = value;
                OnPropertyChanged(nameof(CmdLogs));
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

        /// <summary>
        /// Check/Add New Migrations of project models
        /// </summary>
        public Command ApplyMigrationsCommand { get; set; }
        public Command UpdateDatabaseCommand { get; set; }
        public Command DeleteCommand { get; set; }
        public Command ClearLogCommand { get; set; }

        private void Delete()
        {
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
            // add compiler command to commands list
            if (!ProjectInfo.Commands.Any(x => x is GitCommandInfo))
                ProjectInfo.AddCommand(new GitCommandInfo());
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

        /// <summary>
        /// Cancel/Break All Commands and Queued Commands
        /// </summary>
        private void GitPull()
        {
            if (!ProjectInfo.Commands.Any(x => x is GitCommandInfo))
                ProjectInfo.AddCommand(new GitCommandInfo());
        }
        private void CancelCommands()
        {
            try
            {
                CancellationTokenSource.Cancel();
                CancellationToken.ThrowIfCancellationRequested();
                Debug.WriteLine("Task has been cancelled from Cancellation Command");
                ServerInfo.ServerLogs.Add("Task has been cancelled from Cancellation Command");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                // Generate new token
                CancellationTokenSource = new CancellationTokenSource();
                CancellationToken = new CancellationToken(false);
                CancellationToken = CancellationTokenSource.Token;
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
                System.Windows.MessageBox.Show("Invalid Input Or exist", "validation error", System.Windows.MessageBoxButton.OK);
        }
        /// <summary>
        /// Save all ignore file settings to user settings
        /// </summary>
        private void ClearFileList()
        {
            FileContent = string.Empty;
            ProjectInfo.ServerFiles.Clear();
        }
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
        private async void RunCMD()
        {
            try
            {
                CanRunCommands = false;
                ServerInfo.Servers.Clear();
                bool hasPublishCommand = ProjectInfo.Commands.Any(x => x is PublishCommandInfo);

                foreach (var item in CurrentServerSettingInfo.ServerInfo.Where(x => x.IsChecked))
                {
                    if (hasPublishCommand & item.ProtectionPassword != null)
                    {
                    GetThePass:
                        InputDialogWindow inputDialog = new InputDialogWindow("Please enter your password:");
                        if (inputDialog.ShowDialog() == true)
                        {
                            if (item.ProtectionPassword != PasswordEncoder.ComputeHash(inputDialog.Answer, new SHA256CryptoServiceProvider()))
                            {
                                MessageBox.Show("password does't match!");
                                goto GetThePass;
                            }
                        }
                        else continue;
                    }
                    ServerInfo.Servers.Add(item.Clone());
                }
                if (hasPublishCommand && ServerInfo.Servers.Count <= 0)
                {
                    System.Windows.MessageBox.Show("No Server Selected", "Specify Remote Target", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
                else
                {
                    await Task.Run(async () =>
                   {
                       await RunCustomCommand(ProjectInfo, CancellationToken);
                       await ReadCommandLog();
                   }, CancellationToken);
                }
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "Run CMD");
            }
            finally
            {
                CanRunCommands = true;
                File.Delete(UserSettingInfo.Current.UserSettings.CommandRunnerLogsPath);
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

        /// <summary>
        /// Check Project For Entities/Models Change And Prepair to add migrations
        /// </summary>
        private void ApplyMigrations()
        {
            if (!ProjectInfo.Commands.Any(x => x is RestoreCommandInfo))
                ProjectInfo.AddCommand(new RestoreCommandInfo());
        }

        public static async Task<bool> RunCustomCommand(ProjectInfo ProjectInfo, CancellationToken cancellationToken)
        {
            try
            {
                if (CancellationToken.IsCancellationRequested)
                {
                    Debug.WriteLine("Cancellation Is Requested, breaking RunCustomCommand");
                    return false;
                }
                await ProjectInfo.RunCommands(CancellationToken);
            }
            catch (ArgumentNullException ex)
            {

            }
            return true;
        }

        /// <summary>
        /// field of ProjectInfo instance
        /// </summary>
        ProjectInfo _ProjectInfo;

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
            }
        }

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
        private bool _CanRunCommands = true;

        public bool CanRunCommands
        {
            get { return _CanRunCommands; }
            set
            {
                _CanRunCommands = value;
                OnPropertyChanged(nameof(CanRunCommands));
            }
        }
        public string IgnoreServerFileName
        {
            get { return _IgnoreServerFileName; }
            set
            {
                _IgnoreServerFileName = value;
                OnPropertyChanged(nameof(IgnoreServerFileName));
            }
        }

        public async Task FetchFiles()
        {
            try
            {
                //ClearServerFileListCommand.Execute();
                var result = PublisherServiceProvider.Initialize(SelectedServerInfo);
                var files = await result.FileManagerService.GetTextFilesAsync(ProjectInfo.ProjectKey);

                RunOnUIAction(() =>
                {
                    ProjectInfo.ServerFiles.Clear();
                    foreach (var item in files)
                    {
                        ProjectInfo.ServerFiles.Add(item);
                    }
                });
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
        private async Task LoadFileDataFromServer(string filePath)
        {
            try
            {
                SelectedServerFile = filePath;
                var result = PublisherServiceProvider.Initialize(SelectedServerInfo);
                ServerManagerStreamService serverManagerStreamService = new ServerManagerStreamService(result.CurrentClientProvider);
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
                AutoLogger.Default.LogError(ex, "LoadFileDataFromServer");
            }
            finally
            {
                IsBusy = false;
                LoadFileCommmand.ValidateCanExecute(filePath);
            }
        }

        private async Task UploadFileDataFromServer()
        {
            try
            {
                var providerResult = PublisherServiceProvider.Initialize(SelectedServerInfo);
                ServerManagerStreamService serverManagerStreamService = new ServerManagerStreamService(providerResult.CurrentClientProvider);
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
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "UploadFileDataFromServer");
            }
            finally
            {
                IsBusy = false;
                UploadFileCommmand.ValidateCanExecute();
            }
        }
    }
}
