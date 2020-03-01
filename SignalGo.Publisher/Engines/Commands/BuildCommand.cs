

namespace SignalGo.Publisher.Engines.Commands
{
    public class BuildCommand : CommandBase
    {
        /// <summary>
        /// 
        /// </summary>
        public BuildCommand()
        {
            Name = "compile dotnet project";
            ExecutableFile = "cmd.exe";
            Command = "dotnet";
            Arguments = "build";
            IsEnabled = true;
        }
    }
}
