

namespace SignalGo.Publisher.Engines.Commands
{
    public class BuildCommandInfo : CommandBaseInfo
    {
        /// <summary>
        /// dotnet core sdk
        /// </summary>
        //public BuildCommand()
        //{
        //    Name = "compile dotnet project";
        //    ExecutableFile = "cmd.exe";
        //    Command = "dotnet";
        //    Arguments = "build";
        //    IsEnabled = true;
        //}

        /// <summary>
        /// MsBuild
        /// </summary>
        public BuildCommandInfo()
        {
            Name = "compile dotnet project";
            ExecutableFile = "cmd.exe";
            Command = "msbuild  ";
            Arguments = $"-nologo";
            IsEnabled = true;
        }
    }
}
