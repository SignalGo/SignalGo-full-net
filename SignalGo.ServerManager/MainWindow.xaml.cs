using SignalGo.ServerManager.Helpers;
using SignalGo.ServerManager.ViewModels;
using SignalGo.ServerManager.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SignalGo.ServerManager
{
    public class Loader : MarshalByRefObject
    {
        public Assembly Load(byte[] bytes, AppDomain domain)
        {
            return domain.Load(bytes);
        }
    }
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
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
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

            //if (e.Content.GetType() == typeof(ManagePersonInfoes))
            //{
            //    var page = e.Content as ManagePersonInfoes;
            //    var vm = page.DataContext as ManagePersonInfoesViewModel;
            //    if (vm.DefaultPersonInfoes == null)
            //        vm.RefreshPersons(0, 10);
            //}
        }

        private void Frame_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel.MainFrame = (Frame)sender;
        }
    }
}
