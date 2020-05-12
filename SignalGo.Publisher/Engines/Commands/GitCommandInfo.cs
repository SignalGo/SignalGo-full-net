using System;
using SignalGo.Publisher.Engines.Models;
using SignalGo.Publisher.Models;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SignalGo.Publisher.Engines.Commands
{
    public class GitCommandInfo : CommandBaseInfo
    {
        public GitCommandInfo()
        {
            Name = "Pull Changes From Repository";
            ExecutableFile = "cmd.exe";
            Command = $"git ";
            Arguments = $"pull ";
            IsEnabled = true;
        }


        public override async Task<RunStatusType> Run(CancellationToken cancellationToken)
        {
            var result = await base.Run(cancellationToken);
            return result;
        }
    }
}
