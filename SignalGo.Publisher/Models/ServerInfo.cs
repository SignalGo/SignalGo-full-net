using System;
using Newtonsoft.Json;
using MvvmGo.ViewModels;


namespace SignalGo.Publisher.Models
{
    public class ServerInfo : BaseViewModel
    {

        public ServerInfo()
        {

        }

        private string _ServerName;
        private Guid _ServerKey;
        private string _ServerAddress;
        private string _ServerPort;
        private ServerInfoStatus _ServerStatus = ServerInfoStatus.Stable;

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
        /// status of server
        /// </summary>
        [JsonIgnore]
        public ServerInfoStatus ServerStatus
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

        public enum ServerInfoStatus : byte
        {
            Stable = 1,
            NotStable = 2,
            Updating = 3,
            Restarting = 4,
            Disabled = 5
        }
    }
}
