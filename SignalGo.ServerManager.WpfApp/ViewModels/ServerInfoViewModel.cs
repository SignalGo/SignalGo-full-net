using Microsoft.Win32;
using MvvmGo.Commands;
using SignalGo.ServiceManager.Core.BaseViewModels;
using SignalGo.ServiceManager.Core.Models;
using System;
using System.Diagnostics;
using System.IO;

namespace SignalGo.ServerManager.WpfApp.ViewModels
{
    public class ServerInfoViewModel : ServerInfoBaseViewModel
    {
        public ServerInfoViewModel() : base()
        {
            OpenProjectFolderCommand = new Command(OpenProjectFolder);
        }
        protected override void Delete()
        {
            MainWindowViewModel.MainFrame.GoBack();
            base.Delete();
        }
        protected override void BrowsePath()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.FileName = ServerInfo.AssemblyPath;
            if (fileDialog.ShowDialog().GetValueOrDefault())
                ServerInfo.AssemblyPath = fileDialog.FileName;
            SettingInfo.SaveSettingInfo();
        }
        public Command OpenProjectFolderCommand { get; set; }
        private void OpenProjectFolder()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = Directory.GetParent(ServerInfo.AssemblyPath).FullName
                });
        }
            catch (Exception ex)
            {
                Shared.Log.AutoLogger.Default.LogError(ex, "open project folder");
                System.Windows.MessageBox.Show("error", ex.Message);
            }
}
    }
}
