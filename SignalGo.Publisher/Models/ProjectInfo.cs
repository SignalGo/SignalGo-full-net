using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using MvvmGo.ViewModels;
using SignalGo.Shared.Log;
using SignalGo.Publisher.Views;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SignalGo.Publisher.Engines.Interfaces;
using MvvmGo.Commands;
using System.Linq;
using System.Collections.Generic;
using SignalGo.Publisher.Engines.Commands;

namespace SignalGo.Publisher.Models
{
    public class ProjectInfo : BaseViewModel
    {
        public ProjectInfo()
        {
            RunCommmands = new Command(async () =>
            {
                try
                {
                    await RunCommands();
                }
                catch (Exception ex)
                {

                }
            });
        }

        public Command<ICommand> RunCommmand { get; set; }
        public Command RunCommmands { get; set; }

        [JsonIgnore]
        public ObservableCollection<TextLogInfo> Logs { get; set; } = new ObservableCollection<TextLogInfo>();

        [JsonIgnore]
        public ProjectProcessInfoBase CurrentServerBase { get; set; }
        [JsonIgnore]
        public Action ProcessStarted { get; set; }

        public ObservableCollection<ICommand> Commands { get; set; } = new ObservableCollection<ICommand>();

        private string _Name;
        private Guid _ProjectKey;
        private string _AssemblyPath;
        private ServerInfoStatus _status = ServerInfoStatus.Stable;
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        public Guid ProjectKey
        {
            get
            {
                if (_ProjectKey != Guid.Empty)
                {
                    return _ProjectKey;
                }
                else
                {
                    _ProjectKey = Guid.NewGuid();
                    return _ProjectKey;
                }
            }
            set
            {
                _ProjectKey = value;
                OnPropertyChanged(nameof(ProjectKey));
            }
        }

        public string AssemblyPath
        {
            get
            {
                return _AssemblyPath;
            }
            set
            {
                _AssemblyPath = value;
                OnPropertyChanged(nameof(AssemblyPath));
            }
        }

        [JsonIgnore]
        public ServerInfoStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        /// <summary>
        /// dotnet build
        /// </summary>
        public async void Build()
        {
            var cmd = new BuildCommandInfo()
            {
                Path = AssemblyPath
            };
            await cmd.Run();

        }

        public async Task RunCommands()
        {
            QueueCommandInfo queueCommandInfo = new QueueCommandInfo(Commands.ToList());
            await queueCommandInfo.Run();
        }

        public void AddCommand(ICommand command)
        {
            command.Path = AssemblyPath;
            Commands.Add(command);
        }

        public void UpdateDatabase()
        {

        }

        public void ApplyMigrations()
        {

        }

        public void Publish()
        {
            List<ICommand> buildCommands = new List<ICommand>();
            // call compiler
            buildCommands.Add(new BuildCommandInfo
            {
                Path = AssemblyPath
            });
            //buildCommands.Add(new TestsCommand());
            // call publish tool
            buildCommands.Add(new PublishCommandInfo
            {
                Path = AssemblyPath
            });

            foreach (var item in buildCommands)
            {
                item.Run();
            }

        }

        public void RunTests()
        {

        }

        /// <summary>
        /// dotnet restore
        /// </summary>
        public void RestorePackages()
        {
            // if server status is Stopped
            if (Status == ServerInfoStatus.Stable)
            {
                try
                {
                    // set server status to Started
                    Status = ServerInfoStatus.Updating;
                    CurrentServerBase = new ProjectProcessInfoBase();
                    // start the server from the path
                    CurrentServerBase.Start("dotnet restore" + Name, AssemblyPath);
                    // Insert/Merge Servers Console Window to Server manager Windows Tab
                    ProjectInfoPage.SendToMainHostForHidden(CurrentServerBase.BaseProcess, null);
                    ProcessStarted?.Invoke();
                }
                catch (Exception ex)
                {
                    AutoLogger.Default.LogError(ex, "BuildProject");
                    if (CurrentServerBase != null)
                    {
                        CurrentServerBase.Dispose();
                        CurrentServerBase = null;
                    }
                    Status = ServerInfoStatus.NotStable;
                }
                SettingInfo.SaveSettingInfo();
            }
        }

        public class ConsoleWriter : TextWriter
        {
            public string ProjectName { get; set; }
            public Action<string, string> TextAddedAction { get; set; }

            public ConsoleWriter()
            {
            }

            public override void Write(char value)
            {
                try
                {
                    TextAddedAction?.Invoke(ProjectName, value.ToString());
                }
                catch (Exception ex)
                {
                    AutoLogger.Default.LogError(ex, "Write char");
                }
            }

            /// <summary>
            /// write action
            /// </summary>
            /// <param name="value"></param>
            public override void Write(string value)
            {
                try
                {
                    TextAddedAction?.Invoke(ProjectName, value);
                }
                catch (Exception ex)
                {
                    AutoLogger.Default.LogError(ex, "Write string");
                }
            }

            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
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

        public class TextLogInfo : BaseViewModel
        {
            private string _Text;
            public string Text
            {
                get
                {
                    return _Text;
                }
                set
                {
                    _Text = value;
                    OnPropertyChanged(nameof(Text));
                }
            }
            public bool IsDone { get; set; }
        }

    }
}
