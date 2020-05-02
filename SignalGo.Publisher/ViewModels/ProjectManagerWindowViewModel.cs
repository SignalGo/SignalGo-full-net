using MvvmGo.Commands;
using MvvmGo.ViewModels;
using SignalGo.Publisher.Models;
using SignalGo.Publisher.Views;
using SignalGo.Shared.Log;
using System;
using System.Diagnostics;
using System.IO;
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
            AddNewServerCommand = new Command(AddNewServer);
            ShowSettingsCommand = new Command(ShowSettingsPage);
            AddNewProjectCommand = new Command(AddNewProject);
            ShowAppLogsCommand = new Command(ShowAppLogs);
            ExitApplicationCommand = new Command(ExitApplication);
            ShowCompilerLogsCommand = new Command(ShowCompilerLogs);
            ShowServersCommand = new Command(ShowServers);
            LoadProjects();
        }


        public Command ExitApplicationCommand { get; set; }
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




    }
}
