using SignalGo.Publisher.Engines.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SignalGo.Publisher.Engines.Commands
{
    public class MigrationCommandInfo : CommandBaseInfo
    {
        /// <summary>
        /// MsBuild
        /// </summary>
        public MigrationCommandInfo()
        {
            Name = "add/apply db migrations";
            ExecutableFile = "cmd.exe";
            Command = "dotnet  ";
            Arguments = $"ef add migrations";
            IsEnabled = true;
        }

        public override bool CalculateStatus(string line)
        {
            throw new NotImplementedException();
        }

        public override async Task<RunStatusType> Run(CancellationToken cancellationToken, string caller)
        {
            var result = await base.Run(cancellationToken, caller);
            //var output = result.StartInfo;
            //Status = Models.RunStatusType.Done;
            //Status = Models.RunStatusType.Error;
            return result;
        }
    }
}
