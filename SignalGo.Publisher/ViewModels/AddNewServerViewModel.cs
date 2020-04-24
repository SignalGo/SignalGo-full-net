using System;
using System.Linq;
using System.Windows;
using MvvmGo.Commands;
using MvvmGo.ViewModels;
using SignalGo.Publisher.Models;
using SignalGo.Shared.Log;

namespace SignalGo.Publisher.ViewModels
{
    public class AddNewServerViewModel : BaseViewModel
    {
        public AddNewServerViewModel()
        {
            CancelCommand = new Command(Cancel);
            SaveCommand = new Command(SaveServerSettings);
        }

        public Command CancelCommand { get; set; }
        public Command SaveCommand { get; set; }
        /// <summary>
        /// uniqe server key, very important to integrate server manager's - publisher
        /// </summary>
        private Guid _ServerKey = Guid.NewGuid();
        private string _ServerName;
        private string _ServerAddress;
        private string _ServerPort;
        /// <summary>
        /// default endpoint like /ServerManager/SignalGo
        /// </summary>
        private string _ServerEndPoint = "/ServerManager/SignalGo";


        public string ServerName
        {
            get
            {
                return _ServerName;
            }
            set
            {
                _ServerName = value;
                OnPropertyChanged(nameof(ServerName));
            }
        }
        public Guid ServerKey
        {
            get
            {
                return _ServerKey;
            }
            set
            {
                _ServerKey = value;
                OnPropertyChanged(nameof(ServerKey));
            }
        }

        public string ServerAddress
        {
            get
            {
                return _ServerAddress;
            }
            set
            {
                _ServerAddress = value;
                OnPropertyChanged(nameof(ServerAddress));
            }
        }
        public string ServerPort
        {
            get
            {
                return _ServerPort;
            }
            set
            {
                _ServerPort = value;
                OnPropertyChanged(nameof(ServerPort));
            }
        }

        public string ServerEndPoint
        {
            get
            {
                return _ServerEndPoint;
            }
            set
            {
                _ServerEndPoint = value;
                OnPropertyChanged(nameof(ServerEndPoint));
            }
        }

        private void Cancel()
        {
            ProjectManagerWindowViewModel.MainFrame.GoBack();
        }

        private void SaveServerSettings()
        {
            try
            {
                if (ServerSettingInfo.CurrentServer == null)
                {

                }
                if (string.IsNullOrEmpty(ServerName))
                    MessageBox.Show("Plase set name of server");
                if (string.IsNullOrEmpty(ServerAddress))
                    MessageBox.Show("Plase set Server Address");
                if (string.IsNullOrEmpty(ServerPort))
                    MessageBox.Show("Plase set Server Port");
                if (string.IsNullOrEmpty(ServerEndPoint))
                    MessageBox.Show("Plase set Server Port");
                else if (ServerSettingInfo.CurrentServer.ServerInfo.Any(x => x.ServerName == ServerName))
                    MessageBox.Show("Server name exist on list, please set a different name");
                else
                {
                    ServerSettingInfo.CurrentServer.ServerInfo.Add(new ServerInfo()
                    {
                        ServerKey = ServerKey,
                        ServerName = ServerName,
                        ServerAddress = ServerAddress,
                        ServerPort = ServerPort,
                        ServerEndPoint = ServerEndPoint
                    });
                    ServerSettingInfo.SaveServersSettingInfo();
                    ProjectManagerWindowViewModel.MainFrame.GoBack();
                }
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "save server settings");
            }
        }
    }
}