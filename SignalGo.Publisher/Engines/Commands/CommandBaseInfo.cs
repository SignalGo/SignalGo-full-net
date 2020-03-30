using MvvmGo.ViewModels;
using SignalGo.Publisher.Engines.Interfaces;
using SignalGo.Publisher.Engines.Models;
using SignalGo.Shared.Log;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SignalGo.Publisher.Engines.Commands
{
    public abstract class CommandBaseInfo : PropertyChangedViewModel, ICommand
    {

        private RunStatusType _Status;
        public RunStatusType Status
        {
            get => _Status; set
            {
                _Status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        long _Size;
        long _Position;
 
        public long Size
        {
            get
            {
                return _Size;
            }
            set
            {
                _Size = value;
                OnPropertyChanged("Size");
            }
        }
        public long Position
        {
            get
            {
                return _Position;
            }
            set
            {
                _Position = value;
                OnPropertyChanged("Position");
            }
        }
        public string Name { get; set; }
        public string ExecutableFile { get; set; }
        public string Command { get; set; }
        public string Arguments { get; set; }
        public bool IsEnabled { get; set; }
        /// <summary>
        /// working directory path that process start on it
        /// </summary>
        public string Path { get; set; }

        public virtual async Task<Process> Run()
        {
            try
            {
                Status = RunStatusType.Running;
                //code to run
                //var process = CommandRunner.Run(ExecutableFile, $"{Command} {Arguments}");
                var process = CommandRunner.Run(this);
                Status = RunStatusType.Done;
                return process.Result;
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "Run CommandBaseInfo");
                Status = RunStatusType.Error;
                return null;
            }
        }


    }
}
