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
            ServerInfo.Servers.Clear();
            SaveIgnoreFileListCommand = new Command(SaveIgnoreFileList);
            SaveChangeCommand = new Command(SaveChanges);
            ApplyMigrationsCommand = new Command(ApplyMigrations);
            BuildCommand = new Command(Build);
            BrowseProjectPathCommand = new Command(BrowseProjectPath);
            BrowseAssemblyPathCommand = new Command(BrowseAssemblyPath);
            CancellationCommand = new Command(() =>
            {
                CancelCommands();
            });
            DeleteCommand = new Command(Delete);
            RunTestsCommand = new Command(RunTests);
            RunCommand = new Command(RunCMD);
            RestorePackagesCommand = new Command(RestorePackages);
            RemoveIgnoredServerFileCommand = new Command<string>(RemoveIgnoredServerFile);
            RemoveIgnoredFileCommand = new Command<string>((s) =>
            {
                RemoveIgnoredFile(s);
            });
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
            ToDownCommand = new Command<ICommand>((x) =>
            {
                MoveCommandLower(x);
            });
            ToUpCommand = new Command<ICommand>((x) =>
            {
                MoveCommandUpper(x);
            });
            PublishCommand = new Command(PublishToServers);
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
        public Command<string> RemoveIgnoredFileCommand { get; set; }
        public Command<string> RemoveIgnoredServerFileCommand { get; set; }
        public Command AddIgnoreClientFileCommand { get; set; }
        public Command AddIgnoreServerFileCommand { get; set; }
        public Command<ICommand> RetryCommand { get; set; }
        public Command<ICommand> ToDownCommand { get; set; }
        public Command<ICommand> ToUpCommand { get; set; }
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
            if (!ProjectInfo.Commands.Any(x => x is BuildCommandInfo))
                ProjectInfo.AddCommand(new BuildCommandInfo());
            // add TestsRunner Command if not exist in commands list
            if (!ProjectInfo.Commands.Any(x => x is TestsCommandInfo))
                ProjectInfo.AddCommand(new TestsCommandInfo());
            // add publish command with peroject data|key
            if (!ProjectInfo.Commands.Any(x => x is PublishCommandInfo))
            {
                ProjectInfo.AddCommand(new PublishCommandInfo(new Shared.Models.ServiceContract
                {
                    Name = ProjectInfo.Name,
                    ServiceKey = ProjectInfo.ProjectKey
                }));
            }
        }

        /// <summary>
        /// Cancel/Break All Commands and Queued Commands
        /// </summary>
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
                if (!ProjectInfo.IgnoredFiles.Contains(fileDialog.SafeFileName) && fileDialog.CheckFileExists)
                {
                    ProjectInfo.IgnoredFiles.Add(fileDialog.SafeFileName);
                }
            }
        }
        /// <summary>
        /// add specified file to Server Ignore Update List settings
        /// </summary>
        private void AddIgnoreServerFile()
        {
            if (!ProjectInfo.ServerIgnoredFiles.Contains(IgnoreServerFileName) && !string.IsNullOrEmpty(IgnoreServerFileName))
            {
                ProjectInfo.ServerIgnoredFiles.Add(IgnoreServerFileName);
                IgnoreServerFileName = string.Empty;
            }
            else
                System.Windows.MessageBox.Show("Invalid Input Or exist", "validation error", System.Windows.MessageBoxButton.OK);
        }
        /// <summary>
        /// Save all ignore file settings to user settings
        /// </summary>
        private void SaveIgnoreFileList()
        {
            var clientIgnoreList = CurrentProjectSettingInfo.ProjectInfo.Select(x => x.IgnoredFiles).ToList();
            var serverIgnoreList = CurrentProjectSettingInfo.ProjectInfo.Select(x => x.ServerIgnoredFiles).ToList();
            clientIgnoreList.Add(ProjectInfo.IgnoredFiles);
            serverIgnoreList.Add(ProjectInfo.ServerIgnoredFiles);
            SettingInfo.SaveSettingInfo();
            clientIgnoreList.Clear();
            serverIgnoreList.Clear();
        }
        /// <summary>
        /// remove specified file from server ignore list
        /// </summary>
        /// <param name="name"></param>
        private void RemoveIgnoredServerFile(string name)
        {
            if (ProjectInfo.ServerIgnoredFiles.Contains(name))
                ProjectInfo.ServerIgnoredFiles.Remove(name);
            //SaveIgnoreFileList();
        }
        /// remove specified file from client(publisher) ignore list
        private void RemoveIgnoredFile(string name)
        {
            if (ProjectInfo.IgnoredFiles.Contains(name))
                ProjectInfo.IgnoredFiles.Remove(name);
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
                if (ProjectInfo.Commands.Any(x => x is PublishCommandInfo) && ServerInfo.Servers.Count <= 0)
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
            catch (Exception ex)
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
        private bool _CanRunCommands= true;

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

    }
}
