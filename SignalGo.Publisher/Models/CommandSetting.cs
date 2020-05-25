using MvvmGo.ViewModels;
using System;
using System.Collections.ObjectModel;

namespace SignalGo.Publisher.Models
{
    public class CommandSetting : BaseViewModel
    {
        public CommandSetting()
        {

        }
        
        private Guid _Id;
        private string _Name;
        private string _Command;
        private string _CommandArgs;
        private string _CommandDescription;

        /// <summary>
        /// unique id of command
        /// </summary>
        public Guid Id
        {
            get
            {
                if (_Id != Guid.Empty)
                {
                    return _Id;
                }
                else
                {
                    _Id = Guid.NewGuid();
                    return _Id;
                }
            }
            set
            {
                _Id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        /// <summary>
        /// Command String.
        /// Example: msbuild, dotnet, vstestconsole, etc...
        /// </summary>
        public string Command
        {
            get
            {
                return _Command;
            }
            set
            {
                _Command = value;
                OnPropertyChanged(nameof(Command));
            }
        }
        /// <summary>
        /// argumants for command
        /// example: -verbose:q -logger:file.txt 
        /// </summary>
        public string CommandArgs
        {
            get
            {
                return _CommandArgs;
            }
            set
            {
                _CommandArgs = value;
                OnPropertyChanged(nameof(CommandArgs));
            }
        }
        /// <summary>
        /// Command Name 
        /// Example: Build
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
        /// description for command.
        /// Example: compile dotnet project using msbuild compiler.
        /// </summary>
        public string CommandDescription
        {
            get
            {
                return _CommandDescription;
            }
            set
            {
                _CommandDescription = value;
                OnPropertyChanged(nameof(CommandDescription));
            }
        }
    }
}
