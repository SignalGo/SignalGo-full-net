using System;
using System.Windows;
using System.Diagnostics;
using SignalGo.Shared.Log;
using System.Windows.Controls;
using System.Windows.Navigation;
using SignalGo.ServerManager.Views;
using SignalGo.ServerManager.Models;
using System.Windows.Media.Animation;
using SignalGo.Server.ServiceManager;
using SignalGo.ServerManager.Services;
using SignalGo.ServerManager.ViewModels;

namespace SignalGo.ServerManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow This;
        public MainWindow()
        {
            This = this;
            InitializeComponent();
            mainframe.Navigate(new FirstPage());
            Closing += MainWindow_Closing;
            ServerProvider serverProvider = new ServerProvider();

            serverProvider.RegisterServerService<ServerManagerService>();
            serverProvider.RegisterServerService<ServerManagerStreamService>();
            serverProvider.ProviderSetting.HttpSetting.HandleCrossOriginAccess = true;
            serverProvider.Start("http://localhost:5468/ServerManager/SignalGo");

            Debug.WriteLine("server is started");
        }


        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AutoLogger.Default.LogText("Try to close happens.");
            if (MessageBox.Show("Are you sure to close server manager? this will close all of servers.", "Close application", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                AutoLogger.Default.LogText("Manualy user cancel closing.");
                e.Cancel = true;
                return;
            }
            AutoLogger.Default.LogText("Manualy user closed the server manager.");
            foreach (var server in SettingInfo.Current.ServerInfo)
            {
                try
                {
                    server.CurrentServerBase.Dispose();
                }
                catch (Exception ex)
                {

                }
            }
            Process.GetCurrentProcess().Kill();
        }

        private void MainFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            var ta = new ThicknessAnimation();
            ta.Duration = TimeSpan.FromSeconds(0.3);
            ta.DecelerationRatio = 0.7;
            ta.To = new Thickness(0, 0, 0, 0);
            if (e.NavigationMode == NavigationMode.New || e.NavigationMode == NavigationMode.Forward)
            {
                ta.From = new Thickness(500, 0, -500, 0);
            }
            else if (e.NavigationMode == NavigationMode.Back)
            {
                ta.From = new Thickness(-500, 0, 500, 0);
            }
            else if (e.NavigationMode == NavigationMode.Refresh)
            {
                ta.From = new Thickness(0, 0, 0, 0);
            }

            (e.Content as Page).BeginAnimation(MarginProperty, ta);
        }

        private void Frame_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel.MainFrame = (Frame)sender;
        }
    }
}
