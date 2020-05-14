using MvvmGo.Commands;
using MvvmGo.ViewModels;
using Newtonsoft.Json;
using SignalGo.Publisher.Engines.Commands;
using SignalGo.Publisher.Engines.Interfaces;
using SignalGo.Publisher.ViewModels;
using SignalGo.Shared.Log;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;

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
                    await RunCommands(ProjectInfoViewModel.CancellationToken);

                }
                catch (Exception ex)
                {
                    AutoLogger.Default.LogError(ex, "ProjectInfo Constructor, Commands Initialize");
                }
            });
        }

        /// <summary>
        /// Run Command Prop
        /// </summary>
        [JsonIgnore]
        public Command<ICommand> RunCommmand { get; set; }
        [JsonIgnore]
        public Command RunCommmands { get; set; }

        /// <summary>
        /// List of Commands
        /// </summary>
        [JsonIgnore]
        public ObservableCollection<ICommand> Commands { get; set; } = new ObservableCollection<ICommand>();
        [JsonIgnore]
        public ICommand TestCommand
        {
            get
            {
                return Commands.FirstOrDefault(x => x is TestsCommandInfo);
            }
        }

        private string _Name;
        private Guid _ProjectKey;
        private string _ProjectPath;
        private string _ProjectAssembliesPath;
        private ProjectInfoStatus _Status = ProjectInfoStatus.Stable;

        private ObservableCollection<string> _IgnoredFiles { get; set; } = new ObservableCollection<string>();
        private ObservableCollection<string> _ServerIgnoredFiles { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> ServerIgnoredFiles
        {
            get
            {
                return _ServerIgnoredFiles;
            }
            set
            {
                _ServerIgnoredFiles = value;
                OnPropertyChanged(nameof(ServerIgnoredFiles));
            }
        }

        public ObservableCollection<string> IgnoredFiles
        {
            get
            {
                return _IgnoredFiles;
            }
            set
            {
                _IgnoredFiles = value;
                OnPropertyChanged(nameof(IgnoredFiles));
            }
        }
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

        /// <summary>
        /// unique key of project
        /// </summary>
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

        /// <summary>
        /// project solutions files path
        /// </summary>
        public string ProjectPath
        {
            get
            {
                return _ProjectPath;
            }
            set
            {
                _ProjectPath = value;
                OnPropertyChanged(nameof(ProjectPath));
            }
        }
        /// <summary>
        /// project assemblies(dll's and exe) path
        /// </summary>
        public string ProjectAssembliesPath
        {
            get
            {
                return _ProjectAssembliesPath;
            }
            set
            {
                _ProjectAssembliesPath = value;
                OnPropertyChanged(nameof(ProjectAssembliesPath));
            }
        }

        /// <summary>
        /// status of server
        /// </summary>
        [JsonIgnore]
        public ProjectInfoStatus Status
        {
            get
            {
                return _Status;
            }
            set
            {
                _Status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        //public async void Build()
        //{
        //    try
        //    {
        //        var cmd = new BuildCommandInfo()
        //        {
        //            Path = AssemblyPath
        //        };
        //        await cmd.Run();
        //    }
        //    catch (Exception ex)
        //    {
        //        AutoLogger.Default.LogError(ex, "Build Command");
        //    }

        //}

        /// <summary>
        /// run each commands in queue async
        /// </summary>
        /// <returns></returns>
        public async Task RunCommands(CancellationToken cancellationToken)
        {
            try
            {
                OnPropertyChanged(nameof(TestCommand));
                if (cancellationToken.IsCancellationRequested)
                    return;
                QueueCommandInfo queueCommandInfo = new QueueCommandInfo(Commands.ToList());
                await queueCommandInfo.Run(cancellationToken);
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "Run Command Task");
            }
        }

        /// <summary>
        /// add a command to commands list
        /// </summary>
        /// <param name="command"></param>
        public void AddCommand(ICommand command)
        {
            command.WorkingPath = ProjectPath;
            command.AssembliesPath = ProjectAssembliesPath;
            Commands.Add(command);
        }
        //public void UpdateDatabase()
        //{

        //}

        //public void ApplyMigrations()
        //{

        //}

        //public void Publish()
        //{
        //    List<ICommand> multipleCommands = new List<ICommand>();
        //    // call compiler
        //    multipleCommands.Add(new BuildCommandInfo
        //    {
        //        Path = AssemblyPath
        //    });
        //    //buildCommands.Add(new TestsCommand());
        //    // call publish tool
        //    multipleCommands.Add(new PublishCommandInfo
        //    {
        //        Path = AssemblyPath
        //    });

        //    foreach (var item in multipleCommands)
        //    {
        //        item.Run();
        //    }

        //}

        //public async Task RunTestsAsync()
        //{
        //    var cmd = new TestsCommandInfo()
        //    {
        //        Path = AssemblyPath
        //    };
        //    await cmd.Run();
        //}

        /// <summary>
        /// dotnet restore
        /// </summary>
        //public async Task RestorePackagesAsync()
        //{
        //    var cmd = new RestoreCommandInfo()
        //    {
        //        Path = AssemblyPath
        //    };
        //    await cmd.Run();
        //}

        ///Console Writer
        //public class ConsoleWriter : TextWriter
        //{
        //    public string ProjectName { get; set; }
        //    public Action<string, string> TextAddedAction { get; set; }

        //    public ConsoleWriter()
        //    {
        //    }

        //    public override void Write(char value)
        //    {
        //        try
        //        {
        //            TextAddedAction?.Invoke(ProjectName, value.ToString());
        //        }
        //        catch (Exception ex)
        //        {
        //            AutoLogger.Default.LogError(ex, "Write char");
        //        }
        //    }

        //    /// <summary>
        //    /// write action
        //    /// </summary>
        //    /// <param name="value"></param>
        //    public override void Write(string value)
        //    {
        //        try
        //        {
        //            TextAddedAction?.Invoke(ProjectName, value);
        //        }
        //        catch (Exception ex)
        //        {
        //            AutoLogger.Default.LogError(ex, "Write string");
        //        }
        //    }

        //    public override Encoding Encoding
        //    {
        //        get { return Encoding.UTF8; }
        //    }
        //}

        /// <summary>
        /// status if server
        /// </summary>
        public enum ProjectInfoStatus : byte
        {
            Stable = 1,
            NotStable = 2,
            Updating = 3,
            Restarting = 4,
            Disabled = 5
        }
        /// <summary>
        /// TextLog
        /// </summary>
        //public class TextLogInfo : BaseViewModel
        //{
        //    private string _Text;
        //    public string Text
        //    {
        //        get
        //        {
        //            return _Text;
        //        }
        //        set
        //        {
        //            _Text = value;
        //            OnPropertyChanged(nameof(Text));
        //        }
        //    }
        //    public bool IsDone { get; set; }
        //}

    }
}
