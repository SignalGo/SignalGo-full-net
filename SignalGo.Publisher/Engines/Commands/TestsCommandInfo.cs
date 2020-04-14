using SignalGo.Publisher.Engines.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SignalGo.Publisher.Engines.Commands
{
    public class TestsCommandInfo : CommandBaseInfo
    {
        public TestsCommandInfo()
        {
            Name = "run tests";
            ExecutableFile = "cmd.exe";
            Command = "dotnet";
            Arguments = "test";
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
