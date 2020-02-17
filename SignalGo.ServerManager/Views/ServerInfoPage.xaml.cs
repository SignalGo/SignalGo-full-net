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
            tabItem.LayoutUpdated += (x, ee) =>
            {
                SetWindowPos(vm.ServerInfo.CurrentServerBase.BaseProcess.MainWindowHandle, IntPtr.Zero, 0, 0, (int)this.ActualWidth, (int)this.ActualHeight, SWP_NOZORDER | SWP_NOACTIVATE);
            };
            WindowsFormsHost host = new WindowsFormsHost();
            System.Windows.Forms.Panel p = new System.Windows.Forms.Panel();
            host.Child = p;
            tabItem.Content = host;
            if (vm.ServerInfo.CurrentServerBase != null)
                ChangeParent(vm.ServerInfo.CurrentServerBase.BaseProcess.MainWindowHandle, p.Handle, vm.ServerInfo.CurrentServerBase.BaseProcess, p);
            vm.ServerInfo.ProcessStarted = () =>
            {
                ChangeParent(vm.ServerInfo.CurrentServerBase.BaseProcess.MainWindowHandle, p.Handle, vm.ServerInfo.CurrentServerBase.BaseProcess, p);
            };
        }

        static WindowsFormsHost mainHost = new WindowsFormsHost();
        static System.Windows.Forms.Panel mainPannel = new System.Windows.Forms.Panel();
        public static void SendToMainHostForHidden(System.Diagnostics.Process process, System.Windows.Forms.Panel panel)
        {
            mainHost.Child = mainPannel;
            ChangeParent(process.MainWindowHandle, mainPannel.Handle, process, panel);
        }

        static void ChangeParent(IntPtr main, IntPtr panelHanle, System.Diagnostics.Process process, System.Windows.Forms.Panel panel)
        {
            SetParent(main, panelHanle);

            // remove control box
            int style = GetWindowLong(process.MainWindowHandle, GWL_STYLE);
            style = style & ~WS_CAPTION & ~WS_THICKFRAME;
            SetWindowLong(process.MainWindowHandle, GWL_STYLE, style);

        }

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_NOACTIVATE = 0x0010;
        private const int GWL_STYLE = -16;
        private const int WS_CAPTION = 0x00C00000;
        private const int WS_THICKFRAME = 0x00040000;


        [DllImport("user32.dll", SetLastError = true)]
        private static extern int ShowWindow(IntPtr hwnd, int nCmdShow);
    }
}
