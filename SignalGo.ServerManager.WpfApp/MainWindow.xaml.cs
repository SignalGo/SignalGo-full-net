using System;
using SignalGo.ServerManager.WpfApp.Helpers;
using SignalGo.ServerManager.WpfApp.ViewModels;
using SignalGo.ServiceManager.BaseViewModels.Core;
using SignalGo.ServiceManager.Core.Models;
using SignalGo.ServiceManager.Core.Services;
using SignalGo.Shared;
using SignalGo.Shared.Log;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using MvvmGo.ViewModels;

namespace SignalGo.ServerManager.WpfApp.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow This;

        public MainWindow()
        {
            ServerManagerStreamService.FocusTabFunc = async (server) =>
            {
                AsyncActions.RunOnUI(() =>
                {
                    if (lstProjects.SelectedItem != server)
                        lstProjects.SelectedItem = server;
                    MainWindowViewModel.CurrentServerInfoPage.tabWindow.IsSelected = true;
                });
            };

            ServerInfo.SendToMainHostForHidden = (process) =>
            {
                ServerInfoPage.SendToMainHostForHidden(process, null);
            };
            BaseViewModel.Initialize();
            BaseViewModel.RunOnUIAction = (x) =>
            {
                Dispatcher.BeginInvoke(x);
            };
            AsyncActions.InitializeUIThread();
            ServerProcessBaseInfo.Instance = () => new ServerProcessInfo();
            This = this;
            StartUp.Initialize();
            InitializeComponent();
            mainframe.Navigate(new FirstPage());
            Closing += MainWindow_Closing;
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
            StartUp.Exit();
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
