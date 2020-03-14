using SignalGo.Publisher.Engines.Interfaces;
using SignalGo.Publisher.Engines.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Publisher.Engines.Commands
{
    public abstract class CommandBaseInfo : ICommand
    {

        public RunStatusType Status { get; set; } = RunStatusType.None;
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
                Status = RunStatusType.Error;
                return null;
            }
        }


    }
}
