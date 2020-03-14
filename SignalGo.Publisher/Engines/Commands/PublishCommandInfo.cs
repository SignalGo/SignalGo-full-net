using System;
using System.Collections.Generic;
using System.Text;

namespace SignalGo.Publisher.Engines.Commands
{
    public class PublishCommandInfo : CommandBaseInfo
    {
        public PublishCommandInfo()
        {
            Name = "upload projects to servers";
            ExecutableFile = "cmd.exe";
            Command = "dotnet ";
            Arguments = $"publish -nologo";
            IsEnabled = true;
        }
    }
}
