using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

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
