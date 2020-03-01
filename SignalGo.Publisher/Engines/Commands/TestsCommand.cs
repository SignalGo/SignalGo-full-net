using SignalGo.Publisher.Engines.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Publisher.Engines.Commands
{
    public class TestsCommand : CommandBase
    {
        public TestsCommand()
        {
            Name = "run tests of project";
            ExecutableFile = "cmd.exe";
            Command = "dotnet";
            Arguments = "run tests";
            IsEnabled = true;
        }
    }
}
