using System;
using System.Collections.Generic;
using System.Text;

namespace SignalGo.Publisher.Engines.Commands
{
    class RestoreCommandInfo : CommandBaseInfo
    {
        /// <summary>
        /// run package restore for project
        /// </summary>
        public RestoreCommandInfo()
        {
            Name = "run package restore for project";
            ExecutableFile = "cmd.exe";
            Command = "dotnet";
            Arguments = "restore";
            IsEnabled = true;
        }
    }
}
