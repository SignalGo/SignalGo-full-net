using System;
using System.Collections.Generic;
using System.Text;

namespace SignalGo.Publisher.Engines.Commands
{
    class RestoreCommand : CommandBase
    {
        /// <summary>
        /// run package restore for project
        /// </summary>
        public RestoreCommand()
        {
            Name = "run package restore for project";
            ExecutableFile = "cmd.exe";
            Command = "dotnet";
            Arguments = "restore";
            IsEnabled = true;
        }
    }
}
