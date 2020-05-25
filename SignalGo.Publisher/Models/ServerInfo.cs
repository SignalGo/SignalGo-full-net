using System;
using Newtonsoft.Json;
using MvvmGo.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;
using MvvmGo.Commands;
using System.Diagnostics;
using System.Windows;

namespace SignalGo.Publisher.Models
{
    public class ServerInfo : BaseViewModel
    {
        public static ServerInfo This;
        public ServerInfo()
        {
            This = this;
        }

        private string _ServerName;
        private Guid _ServerKey;
        private string _ServerEndPoint;
        private string _ServerAddress;
        private string _ServerPort;
        private string _ServerLastUpdate;
        private bool _IsChecked;

        private ServerInfoStatusEnum _ServerStatus = ServerInfoStatusEnum.Stable;
        [JsonIgnore]
        private ServerInfoStatusEnum _IsUpdated = ServerInfoStatusEnum.Updating;
        [JsonIgnore]
        private static ObservableCollection<string> _ServerLogs = new ObservableCollection<string>();
        /// <summary>
        /// list of servers that user added for upload/publish to them, 
        /// static because don't renew by AddServerToQueueCommand. just update it
        /// </summary>
        [JsonIgnore]
        public static ObservableCollection<ServerInfo> Servers { get; set; } = new ObservableCollection<ServerInfo>();
        [JsonIgnore]
        public bool IsChecked
        {
            get => _IsChecked; set
            {
                _IsChecked = value;
                OnPropertyChanged(nameof(IsChecked));
            }
        }

        [JsonIgnore]
        public ServerInfoStatusEnum IsUpdated
        {
            get { return _IsUpdated; }
            set
            {
                _IsUpdated = value;
                OnPropertyChanged(nameof(IsUpdated));
            }
        }
        [JsonIgnore]
        public static ObservableCollection<string> ServerLogs
        {
            get { return _ServerLogs; }
            set
            {
                _ServerLogs = value;
                //OnPropertyChanged(nameof(ServerLogs));
            }
        }

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

        public string ServerLastUpdate
        {
            get
            {
                if (string.IsNullOrEmpty(_ServerLastUpdate))
                    return "Never";
                else
                    return _ServerLastUpdate;
            }
            set
            {
                _ServerLastUpdate = value;
                OnPropertyChanged(nameof(ServerLastUpdate));
            }
        }

        /// <summary>
        /// unique key of project
        /// </summary>
        public Guid ServerKey
        {
            get
            {
                if (_ServerKey != Guid.Empty)
                {
                    return _ServerKey;
                }
                else
                {
                    _ServerKey = Guid.NewGuid();
                    return _ServerKey;
                }
            }
            set
            {
                _ServerKey = value;
                OnPropertyChanged(nameof(ServerKey));
            }
        }

        /// <summary>
        /// project solutions files path
        /// </summary>
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
        /// <summary>
        /// server port
        /// </summary>
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
        /// <summary>
        /// server endpoint example:(.../ServerManager/SignalGo)
        /// </summary>
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
        /// <summary>
        /// status of server
        /// </summary>
        public ServerInfoStatusEnum ServerStatus
        {
            get
            {
                return _ServerStatus;
            }
            set
            {
                _ServerStatus = value;
                OnPropertyChanged(nameof(ServerStatus));
            }
        }


        [Flags]
        public enum ServerInfoStatusEnum //: byte
        {
            Stable = 1,
            NotStable = 2,
            Updating = 3,
            Restarting = 4,
            Disabled = 5,
            Disconnected = 6,
            Updated = 7,
            UpdateError = 8,
        }

        public ServerInfo Clone()
        {
            return new ServerInfo()
            {
                IsChecked = IsChecked,
                ServerAddress = ServerAddress,
                ServerEndPoint = ServerEndPoint,
                ServerKey = ServerKey,
                ServerLastUpdate = ServerLastUpdate,
                ServerName = ServerName,
                ServerPort = ServerPort,
                ServerStatus = ServerStatus
            };
        }
    }
}
