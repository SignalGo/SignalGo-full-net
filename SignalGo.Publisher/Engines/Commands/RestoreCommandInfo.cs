using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SignalGo.Publisher.Engines.Commands
{
    class RestoreCommandInfo : CommandBaseInfo
    {
        /// <summary>
        /// run package restore for project
        /// </summary>
        public RestoreCommandInfo()
        {
            Name = "packages restore";
            ExecutableFile = "cmd.exe";
            Command = "dotnet";
            Arguments = "restore";
            IsEnabled = true;
        }
        public override async Task<Process> Run(CancellationToken cancellationToken)
        {
            var result = await base.Run(cancellationToken);
            //var output = result.StartInfo;
            //Status = Models.RunStatusType.Done;
            //Status = Models.RunStatusType.Error;
            return result;
        }
    }
}
