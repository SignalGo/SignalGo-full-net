using MvvmGo.Commands;
using MvvmGo.ViewModels;
using SignalGo.ServiceManager.Models;
using SignalGo.Shared.Log;
using System;
using System.IO;

namespace SignalGo.ServiceManager.BaseViewModels
{
    public class MainWindowBaseViewModel : BaseViewModel
    {
        public static MainWindowBaseViewModel This { get; set; }

        public MainWindowBaseViewModel()
        {
            This = this;
            AddNewServerCommand = new Command(AddNewServer);
            ShowServieLogsCommand = new Command(ShowServieLogs);
            ExitApplicationCommand = new Command(ExitApplication);
            ShowSettingPageCommand = new Command(ShowSettingPage);
        }


        public Command AddNewServerCommand { get; set; }
        public Command ShowServieLogsCommand { get; set; }
        public Command ExitApplicationCommand { get; set; }
        public Command ShowSettingPageCommand { get; set; }

        private ServerInfo _SelectedServerInfo;

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
                ShowServerInfoPage(value);
            }
        }

        public SettingInfo CurrentSettingInfo
        {
            get
            {
                return SettingInfo.Current;
            }
        }
        public void ShowServieLogs()
        {
            System.Diagnostics.Process.Start(
                "notepad",
                Path.Combine(Environment.CurrentDirectory, "AppLogs.log"));
        }

        protected virtual void ShowSettingPage()
        {
        }

        protected virtual void ExitApplication()
        {
           
        }

        protected virtual void ShowServerInfoPage(ServerInfo serverInfo)
        {

        }

        protected virtual void AddNewServer()
        {
            
        }

    }
}
