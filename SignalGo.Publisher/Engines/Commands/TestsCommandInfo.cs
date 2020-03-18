using SignalGo.Publisher.Engines.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
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
