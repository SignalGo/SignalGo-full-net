using MvvmGo.Commands;
using MvvmGo.ViewModels;
using SignalGo.Publisher.Models;
using SignalGo.Publisher.Views;
using SignalGo.Shared.Log;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SignalGo.Publisher.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class ProjectManagerWindowViewModel : BaseViewModel
    {

        public static ProjectManagerWindowViewModel This { get; set; }

        public ProjectManagerWindowViewModel()
        {
            This = this;
            ShowAppHelpPageCommand = new Command(ShowAppHelpPage);
            AddNewServerCommand = new Command(AddNewServer);
            ShowSettingsCommand = new Command(ShowSettingsPage);
            AddNewProjectCommand = new Command(AddNewProject);
            ShowAppLogsCommand = new Command(ShowAppLogs);
            ExitApplicationCommand = new Command(ExitApplication);
            ShowCompilerLogsCommand = new Command(ShowCompilerLogs);
            ShowServersCommand = new Command(ShowServers);
            LoadProjects();

            // get application resouce usage in background
            GetAppUsage();
        }


        public Command ExitApplicationCommand { get; set; }
        public Command ShowAppHelpPageCommand { get; set; }
        public Command AddNewServerCommand { get; set; }
        public Command AddNewProjectCommand { get; set; }
        public Command ShowServersCommand { get; set; }
        public Command ShowSettingsCommand { get; set; }
        public Command ShowAppLogsCommand { get; set; }
        public Command ShowCompilerLogsCommand { get; set; }

        public static Frame MainFrame { get; set; }

        private ProjectInfo _SelectedProjectInfo;

        public ProjectInfo SelectedProjectInfo
        {
            get
            {
                return _SelectedProjectInfo;
            }
            set
            {
                _SelectedProjectInfo = value;
                OnPropertyChanged(nameof(SelectedProjectInfo));
                ProjectInfoPage page = new ProjectInfoPage();
                ProjectInfoViewModel vm = page.DataContext as ProjectInfoViewModel;
                vm.ProjectInfo = value;
                MainFrame.Navigate(page);
            }
        }

        public SettingInfo CurrentSettingInfo
        {
            get
            {
                return SettingInfo.Current;
            }
        }


        private void ShowSettingsPage()
        {
            MainFrame.Navigate(new PublisherSettingManagerPage());

        }
        private void AddNewProject()
        {
            MainFrame.Navigate(new AddNewProjectPage());
        }
        private void ShowCompilerLogs()
        {
            Process.Start("notepad", Path.Combine(Environment.CurrentDirectory, "CommandRunnerLogs.txt"));
        }
        private void ShowAppLogs()
        {
            Process.Start("notepad", Path.Combine(Environment.CurrentDirectory, "AppLogs.log"));
            //using (Process.Start("notepad", $"{Environment.CurrentDirectory}{ConfigurationManager.AppSettings["CommandRunnerLogFilePath"]}")) { }
        }
        private void ShowServers()
        {
            MainFrame.Navigate(new ServerInfoPage());
        }
        private void AddNewServer()
        {
            ProjectManagerWindow.This.mainframe.Navigate(new AddNewServerPage());

        }

        public void ShowAppHelpPage()
        {
            ProjectManagerWindow.This.mainframe.Navigate(new AppHelpPage());

        }
        public void ExitApplication()
        {
            try
            {
                if (MessageBox.Show("Are you sure?", "Exit Application", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                    Application.Current.Shutdown();
            }
            catch { }
        }
        public void LoadProjects()
        {

            try
            {
                foreach (ProjectInfo project in SettingInfo.Current.ProjectInfo)
                {
                }
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "LoadProjectInfo");
            }
        }
        public async Task GetAppUsage()
        {
            try
            {
                Task.Factory.StartNew(async () =>
                {
                    ApplicationRAMUsage = (Process.GetCurrentProcess().PrivateMemorySize64 / 1000000).ToString();
                    await Task.Delay(20000);
                    GetAppUsage();
                }, TaskCreationOptions.LongRunning);
                //ApplicationRAMUsage = GC.GetTotalMemory(true) / 10000;v
                //if (GC.GetGCMemoryInfo().MemoryLoadBytes == 0)
                //    GC.GetTotalMemory(true);
                //ApplicationRAMUsage = (GC.GetGCMemoryInfo().MemoryLoadBytes / 100000).ToString("N0");
                //var cpuSet = ;
                //ApplicationCPUUsage = (cpuSet / 100).ToString();


            }
            catch (Exception e)
            {

            }
            finally
            {
            }

        }
        //private void worker_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    ApplicationRAMUsage = GC.GetTotalMemory(true) / 10000;
        //}

        //private void worker_RunWorkerCompleted(object sender,
        //                                           RunWorkerCompletedEventArgs e)
        //{
        //    ApplicationRAMUsage = GC.GetTotalMemory(true) / 10000;
        //}
        //public static string RAMUsage { get; set; }

        public string _ApplicationRAMUsage;
        public string ApplicationRAMUsage
        {
            get
            {
                return $"< { _ApplicationRAMUsage}";
            }
            set
            {
                _ApplicationRAMUsage = value;
                OnPropertyChanged(nameof(ApplicationRAMUsage));
            }
        }
        //public string _ApplicationCPUUsage;
        //public string ApplicationCPUUsage
        //{
        //    get
        //    {
        //        return $"< { _ApplicationCPUUsage}";
        //    }
        //    set
        //    {
        //        _ApplicationCPUUsage = value;
        //        OnPropertyChanged(nameof(ApplicationCPUUsage));
        //    }
        //}


    }
}
