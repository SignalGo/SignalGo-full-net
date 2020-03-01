using MvvmGo.Commands;
using MvvmGo.ViewModels;
using SignalGo.Publisher.Models;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            PushToServerCommand = new Command(PushToServer);
            RestorePackagesCommand = new Command(RestorePackages);
            UpdateDatabaseCommand = new Command(UpdateDatabase);
            DeleteCommand = new Command(Delete);
            ClearLogCommand = new Command(ClearLog);
            CopyCommand = new Command<TextLogInfo>(Copy);
        }

        /// <summary>
        /// compile and check project assemblies for build
        /// </summary>
        public Command BuildCommand { get; set; }
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
        public Command PushToServerCommand { get; set; }
        /// <summary>
        /// Execute Test Cases of Project
        /// </summary>
        public Command RunTestsCommand { get; set; }
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
        private void PushToServer()
        {
            PushChangesToServers(ProjectInfo);
        }

        /// <summary>
        /// Compile Project Source
        /// </summary>
        private void Build()
        {
            //BuildProjectAssemblies(ProjectInfo);
            BuildProjectAssemblies(new ProjectInfo
            {
                AssemblyPath = ProjectInfo.AssemblyPath,
                Name = "Logger"
            });
        }
        /// <summary>
        /// 
        /// </summary>
        private void RunCMD()
        {
            RunCustomCommand(ProjectInfo);
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

        public static void RunCustomCommand(ProjectInfo ProjectInfo)
        {
            ProjectInfo.RunCommands();
        }
        public static void RestoreProjectPackages(ProjectInfo ProjectInfo)
        {
            ProjectInfo.RestorePackages();
        }

        public static void PushChangesToServers(ProjectInfo ProjectInfo)
        {
            ProjectInfo.PushToServer();
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
