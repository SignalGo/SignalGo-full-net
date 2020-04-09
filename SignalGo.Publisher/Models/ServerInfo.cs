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
            AddToServerQueueCommand = new Command(AddToServerQueue);

        }

        /// <summary>
        /// add server to Server Queue List
        /// </summary>
        private void AddToServerQueue()
        {
            if (!ServerInfo.Servers.Any(x => x.ServerKey == this.ServerKey))
                AddServerToQueueCommand(new ServerInfo());
            else
            {
                RemoveServerFromQueueCommand(this);
            }
        }

        private string _ServerName;
        private Guid _ServerKey;
        private string _ServerEndPoint;
        private string _ServerAddress;
        private string _ServerPort;
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
        } //= new ObservableCollection<string>();

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

        [JsonIgnore]
        public Command AddToServerQueueCommand { get; set; }

        /// <summary>
        /// initialize detail of server that must be add to ServerQueue List
        /// </summary>
        /// <param name="server"></param>
        private void AddServerToQueueCommand(ServerInfo server)
        {
            try
            {
                server.ServerAddress = this.ServerAddress;
                server.ServerName = this.ServerName;
                server.ServerPort = this.ServerPort;
                server.ServerKey = this.ServerKey;
                if (!Servers.Any(s => s.ServerKey == server.ServerKey))
                {
                    Servers.Add(server);
                }
                Debug.WriteLine($"Added {server.ServerName} To ServerQueue");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"cant add {server.ServerName} To ServerQueue");
            }
        }

        /// <summary>
        /// remove a server from ServerQueue List
        /// </summary>
        /// <param name="server"></param>
        public void RemoveServerFromQueueCommand(ServerInfo server)
        {
            try
            {
                var srv = Servers.FirstOrDefault(x => x.ServerKey == server.ServerKey);
                srv.ServerStatus = ServerInfoStatusEnum.Disconnected;
                Servers.Remove(srv);
                Debug.WriteLine($"Removed {server.ServerName} from ServerQueue");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"cant remove {server.ServerName} from ServerQueue");
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

    }
}
