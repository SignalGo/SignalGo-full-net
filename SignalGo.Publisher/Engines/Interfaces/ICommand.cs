using System;
using System.Collections.Generic;
using System.Text;

namespace SignalGo.Publisher.Engines.Interfaces
{
    public interface ICommand : IRunnable
    {
        /// <summary>
        /// humanity text of name of command
        /// </summary>
        public string Name { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public long Position { get; set; }
        public string ExecutableFile { get; set; }
        public string Command { get; set; }
        public string Arguments { get; set; }
    }
}
