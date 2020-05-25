using SignalGo.ServerManager.Views;
using SignalGo.ServiceManager.BaseViewModels;
using SignalGo.ServiceManager.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace SignalGo.ServerManager.ViewModels
{
    public class MainWindowViewModel : MainWindowBaseViewModel
    {
        protected override void ExitApplication()
        {
            System.Windows.Application.Current.Shutdown();
        }
        public static Frame MainFrame { get; set; }

        protected override void AddNewServer()
        {
            MainFrame.Navigate(new AddNewServerPage());
        }

        protected override void ShowSettingPage()
        {
            ServerManagerSettingsPage page = new ServerManagerSettingsPage();
            //ServerManagerSettingsViewModel vm = page.DataContext as ServerManagerSettingsViewModel;
            //vm. = value;
            MainFrame.Navigate(page);
        }

        protected override void ShowServerInfoPage(ServerInfo serverInfo)
        {
            ServerInfoPage page = new ServerInfoPage();
            ServerInfoBaseViewModel vm = page.DataContext as ServerInfoBaseViewModel;
            vm.ServerInfo = serverInfo;
            MainFrame.Navigate(page);
        }
    }
}
