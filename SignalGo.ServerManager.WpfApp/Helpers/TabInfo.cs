using SignalGo.ServerManager.WpfApp.ViewModels;
using SignalGo.ServerManager.WpfApp.Views;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Media;

namespace SignalGo.ServerManager.WpfApp.Helpers
{
    public class TabInfo : IDisposable
    {
        public TabInfo(TabItem tabItem)
        {
            CurrentTabItem = tabItem;
            ServerInfoViewModel = tabItem.DataContext as ServerInfoViewModel;

            CurrentGrid = new Grid()
            {
                Background = Brushes.DarkGray
            };

            tabItem.LayoutUpdated += TabItem_LayoutUpdated;

            WindowsFormsHost host = new WindowsFormsHost()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Background = Brushes.Green
            };
            System.Windows.Forms.Panel formPanel = new System.Windows.Forms.Panel();
            host.Child = formPanel;
            CurrentGrid.Children.Add(host);
            tabItem.Content = CurrentGrid;
            if (ServerInfoViewModel?.ServerInfo?.CurrentServerBase != null)
                ChangeParent(ServerInfoViewModel.ServerInfo.CurrentServerBase.BaseProcess.MainWindowHandle, formPanel.Handle, ServerInfoViewModel.ServerInfo.CurrentServerBase.BaseProcess, formPanel);

            ServerInfoViewModel.ServerInfo.ProcessStarted = () =>
            {
                ChangeParent(ServerInfoViewModel.ServerInfo.CurrentServerBase.BaseProcess.MainWindowHandle, formPanel.Handle, ServerInfoViewModel.ServerInfo.CurrentServerBase.BaseProcess, formPanel);
            };
        }

        //int counter = 0;
        public bool IsEnabled { get; set; }
        public Grid CurrentGrid { get; set; }
        public ServerInfoViewModel ServerInfoViewModel { get; set; }
        public TabItem CurrentTabItem { get; set; }

        private void TabItem_LayoutUpdated(object sender, EventArgs e)
        {
            if (!IsEnabled)
                return;
            //Debug.WriteLine($"tabitem layout updated {counter++}");
            if (ServerInfoViewModel?.ServerInfo?.CurrentServerBase != null)
                SetWindowPos(ServerInfoViewModel.ServerInfo.CurrentServerBase.BaseProcess.MainWindowHandle, IntPtr.Zero, 0, 0, (int)CurrentGrid.ActualWidth, (int)CurrentGrid.ActualHeight, SWP_NOZORDER | SWP_NOACTIVATE);
        }

        public static void UpdateServerInfoLayout(Process process)
        {
            SetWindowPos(process.MainWindowHandle, IntPtr.Zero, 0, 0, (int)MainWindow.This.ActualWidth, (int)MainWindow.This.ActualHeight, SWP_NOZORDER | SWP_NOACTIVATE);
        }

        static WindowsFormsHost mainHost = new WindowsFormsHost();
        static System.Windows.Forms.Panel mainPannel = new System.Windows.Forms.Panel();

        public static void SendToMainHostForHidden(Process process, System.Windows.Forms.Panel panel)
        {
            mainHost.Child = mainPannel;
            ChangeParent(process.MainWindowHandle, mainPannel.Handle, process, panel);
        }

        static void ChangeParent(IntPtr main, IntPtr panelHanle, Process process, System.Windows.Forms.Panel panel)
        {
            SetParent(main, panelHanle);
            // remove control box
            int style = GetWindowLong(process.MainWindowHandle, GWL_STYLE);
            style = style & ~WS_CAPTION & ~WS_THICKFRAME;
            SetWindowLong(process.MainWindowHandle, GWL_STYLE, style);
        }

        public IntPtr MainWindowHandle { get; set; }
        [DllImport("user32.dll", SetLastError = true)]
        private static extern long SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

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

        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    CurrentTabItem.LayoutUpdated -= TabItem_LayoutUpdated;
                }
                CurrentTabItem = null;
                ServerInfoViewModel = null;
                CurrentGrid = null;
                disposedValue = true;
            }
        }

        ~TabInfo()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            //GC.SuppressFinalize(this);
        }
    }
}
