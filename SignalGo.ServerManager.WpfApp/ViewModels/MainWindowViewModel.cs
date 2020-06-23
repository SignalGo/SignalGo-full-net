using SignalGo.ServerManager.WpfApp.Views;
using SignalGo.ServiceManager.Core.BaseViewModels;
using SignalGo.ServiceManager.Core.Models;
using System.Windows.Controls;

namespace SignalGo.ServerManager.WpfApp.ViewModels
{
    public class MainWindowViewModel : MainWindowBaseViewModel
    {
        protected override void ExitApplication()
        {
            System.Windows.Application.Current.Shutdown();
        }
        public static Frame MainFrame { get; set; }
        public static ServerInfoPage CurrentServerInfoPage { get; set; }

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
            CurrentServerInfoPage = new ServerInfoPage();
            ServerInfoBaseViewModel vm = CurrentServerInfoPage.DataContext as ServerInfoBaseViewModel;
            vm.ServerInfo = serverInfo;
            MainFrame.Navigate(CurrentServerInfoPage);
        }
    }
}
