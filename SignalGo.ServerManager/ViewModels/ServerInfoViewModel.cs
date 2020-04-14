using Microsoft.Win32;
using MvvmGo.Commands;
using MvvmGo.ViewModels;
using SignalGo.ServerManager.Models;
using System;
using System.Windows;

namespace SignalGo.ServerManager.ViewModels
{
    public class ServerInfoViewModel : BaseViewModel
    {
        public ServerInfoViewModel()
        {
            StartCommand = new Command(Start);
            StopCommand = new Command(Stop);
            BrowsePathCommand = new Command(BrowsePath);
            ChangeCommand = new Command(Change);
            DeleteCommand = new Command(Delete);
            ClearLogCommand = new Command(ClearLog);
            CopyCommand = new Command<TextLogInfo>(Copy);
        }

        public Command StartCommand { get; set; }
        public Command StopCommand { get; set; }
        public Command ChangeCommand { get; set; }
        public Command BrowsePathCommand { get; set; }
        public Command DeleteCommand { get; set; }
        public Command ClearLogCommand { get; set; }
        public Command<TextLogInfo> CopyCommand { get; set; }

        ServerInfo _ServerInfo;

        public ServerInfo ServerInfo
        {
            get
            {
                return _ServerInfo;
            }

            set
            {
                _ServerInfo = value;
                OnPropertyChanged(nameof(ServerInfo));
            }
        }


        private void Delete()
        {
            SettingInfo.Current.ServerInfo.Remove(ServerInfo);
            SettingInfo.SaveSettingInfo();
            MainWindowViewModel.MainFrame.GoBack();
        }

        private void Stop()
        {
            ServerInfo.Stop();
        }

        private void Start()
        {
            StartServer(ServerInfo);
        }
        private void Change()
        {
            var gu = Guid.Empty;
            if (Guid.TryParse(ServerInfo.ServerKey.ToString(), out gu))
            {
                ServerInfo.ServerKey = gu;
            }
            SettingInfo.SaveSettingInfo();
        }
        private void BrowsePath()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.FileName = ServerInfo.AssemblyPath;
            if (fileDialog.ShowDialog().GetValueOrDefault())
                ServerInfo.AssemblyPath = fileDialog.FileName;
            SettingInfo.SaveSettingInfo();
        }

        public static void StartServer(ServerInfo serverInfo)
        {
            serverInfo.Start();
        }

        private void ClearLog()
        {
            // ServerInfo.Logs.Clear();
        }

        private void Copy(TextLogInfo textLogInfo)
        {
            System.Windows.Forms.Clipboard.SetText(textLogInfo.Text);
        }
    }
}
