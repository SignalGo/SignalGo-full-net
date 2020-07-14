using SignalGo.Publisher.Engines.Models;
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

        public override bool CalculateStatus(string line)
        {
            return false;
        }

        public override async Task<RunStatusType> Run(CancellationToken cancellationToken,string caller)
        {
            var result = await base.Run(cancellationToken,caller);
            return result;
        }
    }
}
