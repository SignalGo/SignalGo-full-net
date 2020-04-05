using System;
using System.Linq;
using System.Windows;
using MvvmGo.Commands;
using MvvmGo.ViewModels;
using SignalGo.Publisher.Models;

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

        private string _ServerName;
        private Guid _ServerKey;
        private string _ServerAddress;
        private string _ServerPort;


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

        private void Cancel()
        {
            ProjectManagerWindowViewModel.MainFrame.GoBack();
        }

        private void SaveServerSettings()
        {
            if (string.IsNullOrEmpty(ServerName))
                MessageBox.Show("Plase set name of server");
            if (string.IsNullOrEmpty(ServerAddress))
                MessageBox.Show("Plase set Server Address");
            if (string.IsNullOrEmpty(ServerPort))
                MessageBox.Show("Plase set Server Port");
            else if (SettingInfo.CurrentServer.ServerInfo.Any(x => x.ServerName == ServerName))
                MessageBox.Show("Server name exist on list, please set a different name");
            else
            {
                SettingInfo.CurrentServer.ServerInfo.Add(new ServerInfo()
                {
                    ServerName = ServerName,
                    ServerAddress = ServerAddress,
                    ServerPort = ServerPort
                });
                SettingInfo.SaveServersSettingInfo();
                ProjectManagerWindowViewModel.MainFrame.GoBack();
            }
        }
    }
}