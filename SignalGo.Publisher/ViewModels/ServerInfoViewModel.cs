using MvvmGo.Commands;
using MvvmGo.ViewModels;
using SignalGo.Publisher.Models;
using SignalGo.Publisher.Views;
using SignalGo.Shared.Log;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace SignalGo.Publisher.ViewModels
{
    public class ServerInfoViewModel : BaseViewModel
    {
        public static ServerInfoViewModel This { get; set; }

        public ServerInfoViewModel()
        {
            This = this;
            //AddNewServerCommand = new Command(AddNewServer);
            RemoveServerCommand = new Command(Delete);
            BackCommand = new Command(Back);
            SaveChangesCommand = new Command(SaveChanges);

            //LoadServers();
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
        //public Command AddNewServerCommand { get; set; }
        public Command RemoveServerCommand { get; set; }
        public Command BackCommand { get; set; }
        public Command SaveChangesCommand { get; set; }

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
                ProjectManagerWindowViewModel.MainFrame.Navigate(page);
            }
        }
        //private void AddNewServer()
        //{
        //    ProjectManagerWindow.This.mainframe.Navigate(new AddNewServerPage());

        //}
        //public void LoadServers()
        //{

        //    try
        //    {
        //        foreach (ServerInfo project in SettingInfo.CurrentServer.ServerInfo)
        //        {
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        AutoLogger.Default.LogError(ex, "LoadServerInfo");
        //    }
        //}
        public ServerSettingInfo CurrentServerSettingInfo
        {
            get
            {
                return ServerSettingInfo.CurrentServer;
            }
        }
        private void Delete()
        {
            if (MessageBox.Show("are you sure?", "Remove Server", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                ServerSettingInfo.CurrentServer.ServerInfo.Remove(ServerInfo);
                ServerSettingInfo.SaveServersSettingInfo();
            }
            ProjectManagerWindowViewModel.MainFrame.GoBack();
        }
        private void Back()
        {
            ProjectManagerWindowViewModel.MainFrame.GoBack();

        }
        private void SaveChanges()
        {
            ServerSettingInfo.SaveServersSettingInfo();
            ProjectManagerWindowViewModel.MainFrame.GoBack();
        }

    }
}
