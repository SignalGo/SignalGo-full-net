using MvvmGo.Commands;
using MvvmGo.ViewModels;
using SignalGo.Publisher.Engines.Commands;
using SignalGo.Publisher.Engines.Interfaces;
using SignalGo.Publisher.Models;
using SignalGo.Publisher.Views;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
            UpdateDatabaseCommand = new Command(UpdateDatabase);
            DeleteCommand = new Command(Delete);
            ClearLogCommand = new Command(ClearLog);
            CopyCommand = new Command<TextLogInfo>(Copy);
            RemoveCommand = new Command<ICommand>((x) =>
            {
                ProjectInfo.Commands.Remove(x);
            });
        }

        /// <summary>
        /// compile and check project assemblies for build
        /// </summary>
        public Command BuildCommand { get; set; }
        public Command<ICommand> RemoveCommand { get; set; }
        /// <summary>
        /// run a custome command/expression
        /// </summary>
        public Command RunCommand { get; set; }
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

            //BuildProjectAssemblies(new ProjectInfo
            //{
            //    AssemblyPath = ProjectInfo.AssemblyPath,
            //    Name = ProjectInfo.Name
            //});
        }
        /// <summary>
        /// 
        /// </summary>
        private void RunCMD()
        {
            Task.Run(async () =>
            {
                await RunCustomCommand(ProjectInfo);
            });
        }

        /// <summary>
        /// restore (install/fix) nuget packages
        /// </summary>
        private void RestorePackages()
        {
            RestoreProjectPackages(ProjectInfo);
        }

        /// <summary>
        /// Run Project Test Cases
        /// </summary>
        private void RunTests()
        {
            RunProjectTests(ProjectInfo);
        }

        /// <summary>
        /// Check Project For Entities/Models Change And Prepair to add migrations
        /// </summary>
        private void ApplyMigrations()
        {
            ApplyProjectMigrations(ProjectInfo);
        }
        /// <summary>
        /// ef update database
        /// </summary>
        private void UpdateDatabase()
        {
            UpdateProjectDatabase(ProjectInfo);
        }


        // 

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
        public static void RestoreProjectPackages(ProjectInfo ProjectInfo)
        {
            ProjectInfo.RestorePackages();
        }

        public static void Publish(ProjectInfo ProjectInfo)
        {
            ProjectInfo.Publish();
        }

        /// <summary>
        /// compile assemblies
        /// </summary>
        /// <param name="ProjectInfo"></param>
        public static void BuildProjectAssemblies(ProjectInfo ProjectInfo)
        {
            Debug.WriteLine("Build Started");
            ProjectInfo.Build();
            Debug.WriteLine("Build Finished");
        }

        /// <summary>
        /// execute test cases
        /// </summary>
        /// <param name="ProjectInfo"></param>
        public static void RunProjectTests(ProjectInfo ProjectInfo)
        {
            ProjectInfo.RunTests();
        }

        /// <summary>
        /// apply migrations update to db
        /// </summary>
        /// <param name="ProjectInfo"></param>
        public static void UpdateProjectDatabase(ProjectInfo ProjectInfo)
        {
            ProjectInfo.UpdateDatabase();
        }

        /// <summary>
        /// check for migrations change/availibility
        /// </summary>
        /// <param name="ProjectInfo"></param>
        public static void ApplyProjectMigrations(ProjectInfo ProjectInfo)
        {
            ProjectInfo.ApplyMigrations();
        }

        /// <summary>
        /// clear logs
        /// </summary>
        private void ClearLog()
        {
            ProjectInfo.Logs.Clear();
        }

        private void Copy(TextLogInfo textLogInfo)
        {
            Clipboard.SetText(textLogInfo.Text);
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
