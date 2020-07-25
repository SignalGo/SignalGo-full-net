using System;
using System.Linq;
using Newtonsoft.Json;
using MvvmGo.Commands;
using System.Threading;
using MvvmGo.ViewModels;
using SignalGo.Shared.Log;
using System.Threading.Tasks;
using SignalGo.Publisher.Engines.Commands;
using SignalGo.Publisher.Engines.Interfaces;
using SignalGo.Publisher.Shared.Models;
using SignalGo.Publisher.ViewModels;
using System.Collections.ObjectModel;

namespace SignalGo.Publisher.Models
{
    public class ProjectInfo : BaseViewModel
    {
        /// <summary>
        /// Project Info Model, Conation All Project Properties
        /// </summary>
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
        private string _LastUpdateDateTime;
        private Guid _ProjectKey;
        private string _ProjectPath;
        private string _ProjectAssembliesPath;

        /// <summary>
        /// ProjectName
        /// </summary>
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
        #region File Manager
        [JsonIgnore]
        public ObservableCollection<string> ServerFiles { get; set; } = new ObservableCollection<string>();
        #endregion

        #region Ignore Files
        private ObservableCollection<IgnoreFileInfo> _IgnoredFiles { get; set; } = new ObservableCollection<IgnoreFileInfo>();
        private ObservableCollection<IgnoreFileInfo> _ServerIgnoredFiles { get; set; } = new ObservableCollection<IgnoreFileInfo>();

        public ObservableCollection<IgnoreFileInfo> ServerIgnoredFiles
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

        public ObservableCollection<IgnoreFileInfo> IgnoredFiles
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
        #endregion
        public string LastUpdateDateTime
        {
            get
            {
                return _LastUpdateDateTime;
            }
            set
            {
                _LastUpdateDateTime = value;
                OnPropertyChanged(nameof(LastUpdateDateTime));
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
        /// run async each commands, in queue 
        /// </summary>
        /// <param name="cancellationToken">token for request cancellation</param>
        /// <returns></returns>
        public async Task RunCommands(CancellationToken cancellationToken)
        {
            try
            {
                OnPropertyChanged(nameof(TestCommand));
                if (cancellationToken.IsCancellationRequested)
                    return;
                QueueCommandInfo queueCommandInfo = new QueueCommandInfo(Commands.ToList());
                await queueCommandInfo.Run(cancellationToken, caller: Name);
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
