using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
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

        public override async Task<Process> Run()
        {
            var result = await base.Run();
            //var output = result.StartInfo;
            //Status = Models.RunStatusType.Done;
            //Status = Models.RunStatusType.Error;
            return result;
        }
    }
}
