using MvvmGo.Commands;
using MvvmGo.ViewModels;
using Newtonsoft.Json;
using SignalGo.ServerManager.Models;
using SignalGo.ServerManager.Views;
using SignalGo.Shared.Log;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SignalGo.ServerManager.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        public static MainWindowViewModel This { get; set; }

        public MainWindowViewModel()
        {
            This = this;
            AddNewServerCommand = new Command(AddNewServer);
            Load();
        }


        public Command AddNewServerCommand { get; set; }

        public static Frame MainFrame { get; set; }

        ServerInfo _SelectedServerInfo;

        public ServerInfo SelectedServerInfo
        {
            get
            {
                return _SelectedServerInfo;
            }
            set
            {
                _SelectedServerInfo = value;
                OnPropertyChanged(nameof(SelectedServerInfo));
                var page = new ServerInfoPage();
                var vm = page.DataContext as ServerInfoViewModel;
                vm.ServerInfo = value;
                MainFrame.Navigate(page);
            }
        }

        public ObservableCollection<ServerInfo> Servers { get; set; } = new ObservableCollection<ServerInfo>();


        private void AddNewServer()
        {
            MainFrame.Navigate(new AddNewServerPage());
        }

        public static void Save()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            File.WriteAllText(Path.Combine(path, "database.db"), JsonConvert.SerializeObject(This.Servers.ToList()), Encoding.UTF8);
        }

        public void Load()
        {
            try
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database.db");
                if (File.Exists(path))
                {
                    var servers = JsonConvert.DeserializeObject<List<ServerInfo>>(File.ReadAllText(path, Encoding.UTF8));
                    foreach (var item in servers)
                    {
                        This.Servers.Add(item);
                    }
                }

                foreach (var server in Servers)
                {
                    ServerInfoViewModel.StartServer(server);
                }
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "Load");
            }
        }
    }
}
