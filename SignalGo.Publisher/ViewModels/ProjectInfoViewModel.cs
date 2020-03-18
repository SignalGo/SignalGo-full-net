using MvvmGo.Commands;
using MvvmGo.ViewModels;
using SignalGo.Publisher.Engines.Commands;
using SignalGo.Publisher.Engines.Interfaces;
using SignalGo.Publisher.Models;
using SignalGo.Publisher.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SignalGo.Publisher.Models.ProjectInfo;

namespace SignalGo.Publisher.ViewModels
{
    public class ProjectInfoViewModel : BaseViewModel
    {
        /// <summary>
        /// viewmodel information of project
        /// </summary>
        public ProjectInfoViewModel()
        {
            RunCommand = new Command(RunCMD);
            BuildCommand = new Command(Build);
            RunTestsCommand = new Command(RunTests);
            ApplyMigrationsCommand = new Command(ApplyMigrations);
            PublishCommand = new Command(PublishToServers);
            RestorePackagesCommand = new Command(RestorePackages);
            ToDownCommand = new Command<ICommand>((x) =>
            {
                MoveDownCmd(x);
            });
            //ToUpCommand = new Command(ToUp);
            ToUpCommand = new Command<ICommand>((x) =>
            {
                MoveUpCmd(x);
            });
            //UpdateDatabaseCommand = new Command(UpdateDatabase);
            DeleteCommand = new Command(Delete);
            ClearLogCommand = new Command(ClearLog);
            CopyCommand = new Command<TextLogInfo>(Copy);
            RemoveCommand = new Command<ICommand>((x) =>
            {
                ProjectInfo.Commands.Remove(x);
            });
            RetryCommand = new Command<ICommand>((x) =>
            {
                x.Run();
            });
            BrowsePathCommand = new Command(BrowsePath);
        }
        public async Task<string> ReadCommandLog()
        {
            return await File.ReadAllTextAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CommandRunnerLogs.txt"));
        }
        private void BrowsePath()
        {

            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = folderBrowserDialog.SelectedPath;
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                this.ProjectInfo.AssemblyPath = folderBrowserDialog.SelectedPath;
            }
            //SettingInfo.Current.ProjectInfo.Add(new ProjectInfo()
            //{
            //    ProjectKey = this.ProjectKey,
            //    AssemblyPath = AssemblyPath,
            //    Name = Name,
            //});
            //SettingInfo.SaveSettingInfo();
        }

        public void MoveDownCmd(ICommand x)
        {
            var index = ProjectInfo.Commands.IndexOf(x);
            //ProjectInfo.Commands.Remove(x);
            if (index + 1 != ProjectInfo.Commands.Count())
                ProjectInfo.Commands.Move(index + 1, index);
        }
        public void MoveUpCmd(ICommand x)
        {
            var index = ProjectInfo.Commands.IndexOf(x);
            //ProjectInfo.Commands.Remove(x);
            if (index != 0)
                ProjectInfo.Commands.Move(index - 1, index);
        }
        /// <summary>
        /// compile and check project assemblies for build
        /// </summary>
        public Command BrowsePathCommand { get; set; }
        public Command BuildCommand { get; set; }
        public Command<ICommand> RemoveCommand { get; set; }
        public Command<ICommand> RetryCommand { get; set; }
        public Command<ICommand> ToDownCommand { get; set; }
        public Command<ICommand> ToUpCommand { get; set; }
        /// <summary>
        /// run a custome command/expression
        /// </summary>
        public Command RunCommand { get; set; }
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
        public Command RunTestsCommand { get; set; }
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
        public Command<TextLogInfo> CopyCommand { get; set; }

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
            //    ProjectInfo.AddCommand(new TestsCommandInfo());
            if (!ProjectInfo.Commands.Any(x => x is PublishCommandInfo))
                ProjectInfo.AddCommand(new PublishCommandInfo());

        }

        /// <summary>
        /// Compile Project Source
        /// </summary>
        private void Build()
        {
            if (!ProjectInfo.Commands.Any(x => x is BuildCommandInfo))
                ProjectInfo.AddCommand(new BuildCommandInfo());
        }

        /// <summary>
        /// 
        /// </summary>
        private void RunCMD()
        {
            Task.Run(async () =>
            {
                await RunCustomCommand(ProjectInfo);
                var logs = ReadCommandLog().Result;
                CmdLogs += logs;
                CmdLogs += "=================";
            });
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

        public static async Task RunCustomCommand(ProjectInfo ProjectInfo)
        {
            try
            {
                await ProjectInfo.RunCommands();
            }
            catch (Exception ex)
            {

            }

        }


        /// <summary>
        /// ef update database
        /// </summary>
        //private void UpdateDatabase()
        //{
        //    if (!ProjectInfo.Commands.Any(x => x is RestoreCommandInfo))
        //        ProjectInfo.AddCommand(new RestoreCommandInfo());
        //}

        //public static void RestoreProjectPackages(ProjectInfo ProjectInfo)
        //{
        //    ProjectInfo.RestorePackagesAsync();
        //}

        //public static void Publish(ProjectInfo ProjectInfo)
        //{
        //    ProjectInfo.Publish();
        //}

        /// <summary>
        /// compile assemblies
        /// </summary>
        /// <param name="ProjectInfo"></param>
        //public static void BuildProjectAssemblies(ProjectInfo ProjectInfo)
        //{
        //    Debug.WriteLine("Build Started");
        //    ProjectInfo.Build();
        //    Debug.WriteLine("Build Finished");
        //}

        /// <summary>
        /// execute test cases
        /// </summary>
        /// <param name="ProjectInfo"></param>
        //public static void RunProjectTests(ProjectInfo ProjectInfo)
        //{
        //    ProjectInfo.RunTestsAsync();
        //}

        /// <summary>
        /// apply migrations update to db
        /// </summary>
        /// <param name="ProjectInfo"></param>
        //public static void UpdateProjectDatabase(ProjectInfo ProjectInfo)
        //{
        //    ProjectInfo.UpdateDatabase();
        //}

        /// <summary>
        /// check for migrations change/availibility
        /// </summary>
        /// <param name="ProjectInfo"></param>
        //public static void ApplyProjectMigrations(ProjectInfo ProjectInfo)
        //{
        //    ProjectInfo.ApplyMigrations();
        //}

        /// <summary>
        /// clear logs
        /// </summary>
        private void ClearLog()
        {
            ProjectInfo.Logs.Clear();
        }

        private void Copy(TextLogInfo textLogInfo)
        {
            System.Windows.Clipboard.SetText(textLogInfo.Text);
        }

        /// <summary>
        /// field of ProjectInfo Model
        /// </summary>
        ProjectInfo _ProjectInfo;

        /// <summary>
        /// instance of ProjectInfo Model
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

    }
}
