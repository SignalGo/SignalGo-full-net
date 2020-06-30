using System;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Windows.Forms.Integration;
using SignalGo.ServerManager.WpfApp.ViewModels;
using System.Windows.Media;
using SignalGo.ServerManager.WpfApp.Helpers;

namespace SignalGo.ServerManager.WpfApp.Views
{
    /// <summary>
    /// Interaction logic for ServerInfoPage.xaml
    /// </summary>
    public partial class ServerInfoPage : Page
    {
        public ServerInfoPage()
        {
            InitializeComponent();
        }

        private void TabItem_Loaded(object sender, RoutedEventArgs e)
        {
            TabItem tabItem = (TabItem)sender;
            var vm = tabItem.DataContext as ServerInfoViewModel;

            ProcessTabLoader.Add(vm.ServerInfo, tabItem);
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = tabWindow.DataContext as ServerInfoViewModel;
            ProcessTabLoader.SetEnabled(tabWindow.IsSelected, vm.ServerInfo);
        }
    }
}
