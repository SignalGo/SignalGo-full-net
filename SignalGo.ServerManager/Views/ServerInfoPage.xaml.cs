using SignalGo.ServerManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SignalGo.ServerManager.Views
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

        public IntPtr MainWindowHandle { get; set; }
        [DllImport("user32.dll", SetLastError = true)]
        private static extern long SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        private void TabItem_Loaded(object sender, RoutedEventArgs e)
        {
            var tabItem = (TabItem)sender;
            var vm = tabItem.DataContext as ServerInfoViewModel;
            WindowsFormsHost host = new WindowsFormsHost();
            System.Windows.Forms.Panel p = new System.Windows.Forms.Panel();
            host.Child = p;
            tabItem.Content = host;
            if (vm.ServerInfo.CurrentServerBase != null)
                SetParent(vm.ServerInfo.CurrentServerBase.BaseProcess.MainWindowHandle, p.Handle);
            vm.ServerInfo.ProcessStarted = () =>
            {
                SetParent(vm.ServerInfo.CurrentServerBase.BaseProcess.MainWindowHandle, p.Handle);
            };
        }

        static WindowsFormsHost mainHost = new WindowsFormsHost();
        static System.Windows.Forms.Panel mainPannel = new System.Windows.Forms.Panel();
        public static void SendToMainHostForHidden(System.Diagnostics.Process process)
        {
            mainHost.Child = mainPannel;
            SetParent(process.MainWindowHandle, mainPannel.Handle);
        }
    }
}
