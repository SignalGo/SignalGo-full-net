using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SignalGo.Publisher.Engines.Interfaces
{
    public interface ICommand : IRunnable
    {
        /// <summary>
        /// humanity text/name of command
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// working directory path for Commands
        /// </summary>
        public string WorkingPath { get; set; }
        /// <summary>
        /// path of dll's/executable file of published project
        /// </summary>
        public string AssembliesPath { get; set; }
        /// <summary>
        /// size if file/stream to set max progress value
        /// </summary>
        public long Size { get; set; }
        /// <summary>
        /// postion of file/stream for reporting it's progress value
        /// </summary>
        public long Position { get; set; }
        /// <summary>
        /// shell (cmd,bash...)
        /// </summary>
        public string ExecutableFile { get; set; }
        public string Command { get; set; }
        /// <summary>
        /// parameters
        /// </summary>
        public string Arguments { get; set; }
    }
}
