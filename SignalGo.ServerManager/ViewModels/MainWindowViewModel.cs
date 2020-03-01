using MvvmGo.Commands;
using MvvmGo.ViewModels;
using SignalGo.ServerManager.Models;
using SignalGo.ServerManager.Views;
using SignalGo.Shared.Log;
using System;
using System.Windows.Controls;

namespace SignalGo.ServerManager.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        public static MainWindowViewModel This { get; set; }

        public MainWindowViewModel()
        {
            This = this;
            AddNewServerCommand = new Command(AddNewServer);
            Load();
        }


        public Command AddNewServerCommand { get; set; }

        public static Frame MainFrame { get; set; }

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
                ServerInfoPage page = new ServerInfoPage();
                ServerInfoViewModel vm = page.DataContext as ServerInfoViewModel;
                vm.ServerInfo = value;
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

        private void AddNewServer()
        {
            MainFrame.Navigate(new AddNewServerPage());
        }

        public void Load()
        {
            try
            {
                foreach (ServerInfo server in SettingInfo.Current.ServerInfo)
                {
                    server.Status = ServerInfoStatus.Stopped;
                    ServerInfoViewModel.StartServer(server);
                }
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "Load");
            }
        }
    }
}
