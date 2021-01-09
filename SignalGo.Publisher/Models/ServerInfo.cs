using System;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace SignalGo.Publisher.Models
{
    public class ServerInfo : MvvmGo.ViewModels.BaseViewModel
    {
        public static ServerInfo This;
        public ServerInfo()
        {
            This = this;
        }

        private string _ServerName;
        private string _ProtectionPassword;
        private Guid _ServerKey;
        private string _ServerEndPoint;
        private string _ServerAddress;
        private string _ServerPort;
        private string _ServerDefaultSolutionShortName;
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

        public string ProtectionPassword
        {
            get
            {
                return _ProtectionPassword;
            }
            set
            {
                _ProtectionPassword = value;
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
        /// short name of default solution in multiple solution projects
        /// the short name that exist in solution name
        /// </summary>
        public string ServerDefaultSolutionShortName
        {
            get
            {
                return _ServerDefaultSolutionShortName;
            }
            set
            {
                _ServerDefaultSolutionShortName = value;
                OnPropertyChanged(nameof(ServerDefaultSolutionShortName));
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

        #region Ignore Some MvvmGo Properties From Saving in file
        [JsonIgnore]
        public override bool IsBusy { get => base.IsBusy; set => base.IsBusy = value; }
        [JsonIgnore]
        public override MvvmGo.Models.ValidationMessageInfo FirstMessage { get => base.FirstMessage; }
        [JsonIgnore]
        public override string BusyContent { get => base.BusyContent; set => base.BusyContent = value; }
        [JsonIgnore]
        public override Action<string> BusyContentChangedAction { get => base.BusyContentChangedAction; set => base.BusyContentChangedAction = value; }
        [JsonIgnore]
        public override Action<bool, string> IsBusyChangedAction { get => base.IsBusyChangedAction; set => base.IsBusyChangedAction = value; }
        [JsonIgnore]
        public override System.Collections.ObjectModel.ObservableCollection<MvvmGo.Models.ValidationMessageInfo> AllMessages { get => base.AllMessages; set => base.AllMessages = value; }
        [JsonIgnore]
        public override bool HasError { get => base.HasError; set => base.HasError = value; }
        [JsonIgnore]
        public override bool IsChangeBusyWhenCommandExecute { get => base.IsChangeBusyWhenCommandExecute; set => base.IsChangeBusyWhenCommandExecute = value; }
        [JsonIgnore]
        public override System.Collections.Concurrent.ConcurrentDictionary<string, MvvmGo.Models.ViewModelItemsInfo> MessagesByProperty { get => base.MessagesByProperty; set => base.MessagesByProperty = value; }
        [JsonIgnore]
        public override Action<string> PropertyChangedAction { get => base.PropertyChangedAction; set => base.PropertyChangedAction = value; }
        #endregion

    }
}
