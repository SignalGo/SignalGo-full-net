using SignalGo.ServerManager.WpfApp.Views;
using SignalGo.ServiceManager.Core.BaseViewModels;
using SignalGo.ServiceManager.Core.Models;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace SignalGo.ServerManager.WpfApp.ViewModels
{
    public class MainWindowViewModel : MainWindowBaseViewModel
    {
        public MainWindowViewModel()
        {
            Projects.Filter = new Predicate<object>(o => Filter(o as ServerInfo));
        }

        protected override void ExitApplication()
        {
            System.Windows.Application.Current.Shutdown();
        }
        public static Frame MainFrame { get; set; }
        public static ServerInfoPage CurrentServerInfoPage { get; set; }

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
                return CollectionViewSource.GetDefaultView(CurrentSettingInfo.ServerInfo);
            }
        }
        private bool Filter(ServerInfo projectInfo)
        {
            return string.IsNullOrEmpty(SearchText)
                || projectInfo.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
        }

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
