using System;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Windows.Forms.Integration;
using SignalGo.ServerManager.WpfApp.ViewModels;
using System.Windows.Media;

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

        public IntPtr MainWindowHandle { get; set; }
        [DllImport("user32.dll", SetLastError = true)]
        private static extern long SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        int counter = 0;
        private void TabItem_Loaded(object sender, RoutedEventArgs e)
        {
            var tabItem = (TabItem)sender;
            var vm = tabItem.DataContext as ServerInfoViewModel;

            Grid grid = new Grid()
            {
                Background = Brushes.DarkGray
            };
            tabItem.LayoutUpdated += (x, ee) =>
            {
                // to fix show console window error/bug
                //TODO: This infinite code(line 38) slow down cpu Performance
                tabItem.Header = $"Window ({grid.ActualWidth},{grid.ActualHeight})";
                System.Diagnostics.Debug.WriteLine(
                    $"tabitem layout updated {counter++}");

                if (vm?.ServerInfo?.CurrentServerBase != null)
                    SetWindowPos(vm.ServerInfo.CurrentServerBase.BaseProcess.MainWindowHandle, IntPtr.Zero, 0, 0, (int)grid.ActualWidth, (int)grid.ActualHeight, SWP_NOZORDER | SWP_NOACTIVATE);
            };
            WindowsFormsHost host = new WindowsFormsHost()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Background = Brushes.Green
            };
            System.Windows.Forms.Panel p = new System.Windows.Forms.Panel();
            host.Child = p;
            grid.Children.Add(host);
            tabItem.Content = grid;
            if (vm?.ServerInfo?.CurrentServerBase != null)
                ChangeParent(vm.ServerInfo.CurrentServerBase.BaseProcess.MainWindowHandle, p.Handle, vm.ServerInfo.CurrentServerBase.BaseProcess, p);
            vm.ServerInfo.ProcessStarted = () =>
            {
                ChangeParent(vm.ServerInfo.CurrentServerBase.BaseProcess.MainWindowHandle, p.Handle, vm.ServerInfo.CurrentServerBase.BaseProcess, p);
            };
        }

        public static void UpdateServerInfoLayout(System.Diagnostics.Process process)
        {
            SetWindowPos(process.MainWindowHandle, IntPtr.Zero, 0, 0, (int)MainWindow.This.ActualWidth, (int)MainWindow.This.ActualHeight, SWP_NOZORDER | SWP_NOACTIVATE);
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
