using MvvmGo.Commands;
using MvvmGo.ViewModels;
using SignalGo.Publisher.Engines.Security;
using SignalGo.Publisher.Models;
using SignalGo.Publisher.Models.Extra;
using SignalGo.Publisher.Views;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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
            ChangeAccessControlCommand = new Command<bool>(ChangeAccessControl);
            CleanMemoryCommand = new TaskCommand(CleanAppMemory,
                () => !IsBusy);
            RunSelfUpdateCommand = new TaskCommand(RunSelfUpdate,
                () => !IsBusy);
            ShowAppHelpPageCommand = new Command(ShowAppHelpPage);
            AddNewServerCommand = new Command(AddNewServer);
            ShowSettingsCommand = new Command(ShowSettingsPage);
            AddNewCategoryCommand = new Command(AddNewCategory);
            AddNewProjectCommand = new Command(AddNewProject);
            ShowAppLogsCommand = new Command(ShowAppLogs);
            ShowCompilerLogsCommand = new Command(ShowCompilerLogs);
            ShowAppBackupsCommand = new Command(ShowAppBackups);
            ExitApplicationCommand = new Command(ExitApplication);
            ShowServersCommand = new Command(ShowServers);
            ShowCommandManagerCommand = new Command(ShowCommandManagerPage);
            // get application resouce usage in background
            GetAppUsage();

            Projects.Filter = new Predicate<object>(o => Filter(o as ProjectInfo));
        }

        public Command<bool> ChangeAccessControlCommand { get; set; }
        public Command ExitApplicationCommand { get; set; }
        public Command ShowAppHelpPageCommand { get; set; }
        public Command ShowCommandManagerCommand { get; set; }
        public Command AddNewServerCommand { get; set; }
        public Command AddNewProjectCommand { get; set; }
        public Command ShowServersCommand { get; set; }
        public Command ShowSettingsCommand { get; set; }
        public Command ShowAppLogsCommand { get; set; }
        public Command ShowAppBackupsCommand { get; set; }
        public Command ShowCompilerLogsCommand { get; set; }
        public Command AddNewCategoryCommand { get; set; }
        public TaskCommand CleanMemoryCommand { get; set; }
        public TaskCommand RunSelfUpdateCommand { get; set; }

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

        string _SearchText;

        public string SearchText
        {
            get
            {
                return _SearchText;
            }
            set
            {
                _SearchText = value;
                OnPropertyChanged(nameof(SearchText));
                Projects.Refresh();
            }
        }

        public ICollectionView Projects
        {
            get
            {
                return CollectionViewSource.GetDefaultView(CurrentProjectsSettingInfo.ProjectInfo);
            }
        }

        public SettingInfo CurrentProjectsSettingInfo
        {
            get
            {
                return SettingInfo.Current;
            }
        }

        public UserSettingInfo CurrentUserSettingInfo
        {
            get
            {
                return UserSettingInfo.Current;
            }
        }

        private bool Filter(ProjectInfo projectInfo)
        {
            return string.IsNullOrEmpty(SearchText)
                || projectInfo.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
        }

        private Task RunSelfUpdate()
        {

            var answer = System.Windows.Forms.MessageBox.Show("Soon... \n You Can Get New Version's From Publisher Telegram Channel For Now. https://t.me/PublisherGo", "Update Publisher", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Information);

            if (answer == System.Windows.Forms.DialogResult.Yes)
            {
                Process.Start("explorer", "https://t.me/PublisherGo");
            }
            return Task.CompletedTask;
        }
        private void AddNewCategory()
        {
            MainFrame.Navigate(new AddNewCategoryPage());
        }
        private void ShowSettingsPage()
        {
            MainFrame.Navigate(new PublisherSettingManagerPage());

        }
        private void ShowCommandManagerPage()
        {
            MainFrame.Navigate(new CommandManagerPage());

        }
        private void AddNewProject()
        {
            MainFrame.Navigate(new AddNewProjectPage());
        }
        private void ShowCompilerLogs()
        {
            Process.Start("notepad", UserSettingInfo.Current.UserSettings.CommandRunnerLogsPath);
        }
        private void ShowAppBackups()
        {
            Process.Start("explorer.exe", Path.Combine(Environment.CurrentDirectory, "AppBackups"));
        }
        private void ShowAppLogs()
        {
            Process.Start("notepad", UserSettingInfo.Current.UserSettings.LoggerPath);
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

            ////try
            ////{
            ////    foreach (ProjectInfo project in SettingInfo.Current.ProjectInfo)
            ////    {
            ////    }
            ////}
            ////catch (Exception ex)
            ////{
            ////    AutoLogger.Default.LogError(ex, "LoadProjectInfo");
            ////}
        }

        /// <summary>
        /// clean/free application memorry an resource is used
        /// </summary>
        /// <returns></returns>
        private void ChangeAccessControl(bool state)
        {
            IsAccessControlUnlocked = state;
        }
        public async Task CleanAppMemory()
        {
            await LogModule.FullApplicationClean();
        }
        /// <summary>
        /// get publisher resource usage in Background (default every 20 sec)
        /// </summary>
        /// <returns></returns>
        public async void GetAppUsage()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    ApplicationRAMUsage = (Process.GetCurrentProcess().PrivateMemorySize64 / 1000000);
                    await Task.Delay(20000);
                }
            });

        }

        //public Color _UsageStatusIndicatorColor = new SolidColorBrush(Color.FromRgb(64, 201, 33)).Color;
        //public Color UsageStatusIndicatorColor
        //{
        //    get
        //    {
        //        return _UsageStatusIndicatorColor;
        //    }
        //    set
        //    {
        //        _UsageStatusIndicatorColor = value;
        //        OnPropertyChanged(nameof(UsageStatusIndicatorColor));
        //    }
        //}

        public long _ApplicationRAMUsage;
        public long ApplicationRAMUsage
        {
            get
            {
                return _ApplicationRAMUsage;
            }
            set
            {
                _ApplicationRAMUsage = value;
                OnPropertyChanged(nameof(ApplicationRAMUsage));
            }
        }

        private bool _LockState = false;
        /// <summary>
        /// lock/unlock state for AccessControl ToggleButton.
        /// </summary>
        public bool LockState
        {
            get { return _LockState; }
            set
            {
                if (value)
                    _LockState = AccessControl.UnlockAccessControl();
                else
                {
                    AccessControl.LockAccessControl();
                    _LockState = value;
                }
            }
        }

        private bool _IsAccessControlUnlocked = false;
        public bool IsAccessControlUnlocked
        {
            get
            {
                return _IsAccessControlUnlocked;
            }
            set
            {
                _IsAccessControlUnlocked = value;
                OnPropertyChanged(nameof(IsAccessControlUnlocked));
            }
        }

    }
}
