using MvvmGo.Commands;
using MvvmGo.ViewModels;
using SignalGo.Server.ServiceManager;
using SignalGo.ServerManager.Models;
using SignalGo.ServerManager.Views;
using SignalGo.Shared.Log;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SignalGo.ServerManager.ViewModels
{
    public class ServerInfoViewModel : BaseViewModel
    {
        public ServerInfoViewModel()
        {
            StartCommand = new Command(Start);
            StopCommand = new Command(Stop);
            DeleteCommand = new Command(Delete);
            ClearLogCommand = new Command(ClearLog);
            CopyCommand = new Command<TextLogInfo>(Copy);
        }

        public Command StartCommand { get; set; }
        public Command StopCommand { get; set; }
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
            MainWindowViewModel.This.Servers.Remove(ServerInfo);
            MainWindowViewModel.Save();
            MainWindowViewModel.MainFrame.GoBack();
        }

        private void Stop()
        {
            if (ServerInfo.Status == ServerInfoStatus.Started)
            {
                while (true)
                {
                    try
                    {
                        ServerInfo.CurrentServerBase.Dispose();
                        ServerInfo.CurrentServerBase = null;
                        ServerInfo.Status = ServerInfoStatus.Stopped;
                        break;
                    }
                    catch (Exception ex)
                    {
                        AutoLogger.Default.LogError(ex, "Stop Server");
                    }
                    finally
                    {
                        GC.Collect();
                        GC.WaitForFullGCComplete();
                        GC.Collect();
                    }
                }
            }
        }

        private void Start()
        {
            StartServer(ServerInfo);
        }

        public static void StartServer(ServerInfo serverInfo)
        {
            if (serverInfo.Status == ServerInfoStatus.Stopped)
            {
                try
                {
                    serverInfo.Status = ServerInfoStatus.Started;
                    serverInfo.CurrentServerBase = new ServerProcessInfoBase();
                    serverInfo.CurrentServerBase.Start("App_" + serverInfo.Name, serverInfo.AssemblyPath);
                    ServerInfoPage.SendToMainHostForHidden(serverInfo.CurrentServerBase.BaseProcess);
                    serverInfo.ProcessStarted?.Invoke();
                }
                catch (Exception ex)
                {
                    AutoLogger.Default.LogError(ex, "StartServer");
                    if (serverInfo.CurrentServerBase != null)
                    {
                        serverInfo.CurrentServerBase.Dispose();
                        serverInfo.CurrentServerBase = null;
                    }
                    serverInfo.Status = ServerInfoStatus.Stopped;
                }
            }
        }

        private void ClearLog()
        {
            // ServerInfo.Logs.Clear();
        }

        private void Copy(TextLogInfo textLogInfo)
        {
            Clipboard.SetText(textLogInfo.Text);
        }

    }
}
