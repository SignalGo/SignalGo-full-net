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
using SignalGo.Publisher.Engines.Models;

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
            ApplyMigrationsCommand = new Command(ApplyMigrations);
            BuildCommand = new Command(Build);
            BrowsePathCommand = new Command(BrowsePath);
            CancellationCommand = new Command(() =>
            {
                CancelCommands();
            });
            DeleteCommand = new Command(Delete);
            RunTestsCommand = new Command(RunTests);
            RunCommand = new Command(RunCMD);
            RestorePackagesCommand = new Command(RestorePackages);
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
            var logFile = await File.ReadAllTextAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CommandRunnerLogs.txt"));
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
        private void BrowsePath()
        {

            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = folderBrowserDialog.SelectedPath;
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                this.ProjectInfo.ProjectPath = folderBrowserDialog.SelectedPath;
            }
            //SettingInfo.Current.ProjectInfo.Add(new ProjectInfo()
            //{
            //    ProjectKey = this.ProjectKey,mg
            //    AssemblyPath = AssemblyPath,
            //    Name = Name,
            //});
            //SettingInfo.SaveSettingInfo();
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
        /// Move a command to topest of Queue List.
        /// that will run first
        /// </summary>
        /// <param name="x"></param>
        public void MoveCommandToppest(ICommand x)
        {
            var index = ProjectInfo.Commands.IndexOf(x);
            if (index != 0)
                ProjectInfo.Commands.Move(index - 1, 0);
        }

        /// <summary>
        /// compile and check project assemblies for build
        /// </summary>
        public Command BrowsePathCommand { get; set; }
        public Command BuildCommand { get; set; }
        public Command CancellationCommand { get; set; }
        public Command<ICommand> RemoveCommand { get; set; }
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
        /// <summary>
        /// push/upload changes to remote servers
        /// </summary>
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
        /// push/update projects and related assemblies
        /// </summary>
        private void PublishToServers()
        {
            if (!ProjectInfo.Commands.Any(x => x is BuildCommandInfo))
                ProjectInfo.AddCommand(new BuildCommandInfo());
            //if (!ProjectInfo.Commands.Any(x => x is TestsCommandInfo))
            //ProjectInfo.AddCommand(new TestsCommandInfo());
            if (!ProjectInfo.Commands.Any(x => x is PublishCommandInfo))
                ProjectInfo.AddCommand(new PublishCommandInfo());
        }
        private void CancelCommands()
        {
            try
            {
                // if cancellation was called before, Make new cancel token for current Cancel

                //if (CancellationToken.IsCancellationRequested)
                //{
                //    CancellationTokenSource = new CancellationTokenSource();
                //    CancellationToken = CancellationTokenSource.Token;
                //}
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
                CancellationTokenSource = new CancellationTokenSource();
                CancellationToken = new CancellationToken(false);
                CancellationToken = CancellationTokenSource.Token;
            }
        }

        /// <summary>
        /// Compile Project Source
        /// </summary>
        private void Build()
        {
            if (!ProjectInfo.Commands.Any(x => x is BuildCommandInfo))
                ProjectInfo.AddCommand(new BuildCommandInfo());
        }
        public Thread thread;
        /// <summary>
        /// Init Commands to Run in Queue
        /// </summary>
        private async void RunCMD()
        {
            var runner = Task.Run(async () =>
            {
                await Task.Delay(2000);
                await RunCustomCommand(ProjectInfo, CancellationToken);
                await ReadCommandLog();
            }, CancellationToken);


            //t1.GetAwaiter().OnCompleted(() =>
            //{
            //    MessageBox.Show("All Commands Completed");
            //});
            //Debug.WriteLine("Task {0} executing", t1.Id);
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
            // TODO: add test runner logics
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

        public SettingInfo CurrentServerSettingInfo
        {
            get
            {
                return SettingInfo.CurrentServer;
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


    }
}
