using MvvmGo.Commands;
using MvvmGo.ViewModels;
using SignalGo.Server.ServiceManager;
using SignalGo.ServerManager.Models;
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
                try
                {
                    ServerInfo.CurrentServerBase.Dispose();
                    ServerInfo.CurrentServerBase = null;
                    ServerInfo.Status = ServerInfoStatus.Stopped;
                }
                catch (Exception ex)
                {
                    ServerInfo.Logs.Add(new TextLogInfo() { Text = ex.ToString(), IsDone = true });
                }
                finally
                {
                    GC.Collect();
                    GC.WaitForFullGCComplete();
                    GC.Collect();
                }
            }
        }

        private void Start()
        {
            if (ServerInfo.Status == ServerInfoStatus.Stopped)
            {
                try
                {
                    ServerInfo.Status = ServerInfoStatus.Started;
                    ServerInfo.CurrentServerBase = new Helpers.ServerInfoBase();
                    //LogSystem logSystem = new LogSystem(ServerInfo.Logs);

                    var logger = ServerInfo.CurrentServerBase.Start(ServerInfo.Name, ServerInfo.AssemblyPath);
                    LogSystem logSystem = new LogSystem();

                    logger.TextAddedAction = logSystem.Add;
                    //LogSystem logSystem = new LogSystem();
                    //AssemblyLoader.ConsoleWriterAction = logSystem.Add;
                }
                catch (Exception ex)
                {
                    ServerInfo.Logs.Add(new TextLogInfo() { Text = ex.ToString(), IsDone = true });
                    if (ServerInfo.CurrentServerBase != null)
                    {
                        ServerInfo.CurrentServerBase.Dispose();
                        ServerInfo.CurrentServerBase = null;
                    }
                    ServerInfo.Status = ServerInfoStatus.Stopped;
                }
            }
        }

        private void ClearLog()
        {
            ServerInfo.Logs.Clear();
        }

        private void Copy(TextLogInfo textLogInfo)
        {
            Clipboard.SetText(textLogInfo.Text);
        }

    }

    public class LogSystem : MarshalByRefObject
    {
        public void Add(string serverName, string value)
        {
            MainWindow.This.Dispatcher.Invoke(() =>
            {
                var serverInfo = MainWindowViewModel.This.Servers.FirstOrDefault(x => x.Name == serverName);
                if (serverInfo == null)
                    return;
                var _logs = serverInfo.Logs;
                TextLogInfo textLogInfo;
                if (_logs.Count == 0 || _logs.Last().IsDone)
                {
                    textLogInfo = new TextLogInfo();
                    _logs.Add(textLogInfo);
                }
                else
                    textLogInfo = _logs.Last();
                textLogInfo.Text += value;
                if (value == "\n")
                    textLogInfo.IsDone = true;
            });
        }

    }


}
