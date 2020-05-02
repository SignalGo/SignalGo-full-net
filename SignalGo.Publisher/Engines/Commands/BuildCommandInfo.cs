using SignalGo.Publisher.Engines.Models;
using SignalGo.Publisher.Models;
using System;
using System.Diagnostics;
using System.Linq;
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
        public UserSetting CurrentSettings
        {
            get
            {
                return UserSettingInfo.Current.UserSettings;
            }
        }
        string buildType = "Rebuild";
        string outputType = "Debug";
        /// <summary>
        /// MsBuild
        /// </summary>
        public BuildCommandInfo()
        {
            Name = "compile dotnet project";
            ExecutableFile = "cmd.exe";
            Command = $"{UserSettingInfo.Current.UserSettings.MsbuildPath} ";
            var Configuration = CurrentSettings;
            //p:Configuration=Debug
            //int MinT, maxT, CurrentT, IOT;
            //ThreadPool.GetAvailableThreads(out maxT, out IOT);
            //var tx = ThreadPool.SetMaxThreads(CurrentSettings.MaxThreads, CurrentSettings.MaxThreads);
            //ThreadPool.GetAvailableThreads(out maxT, out IOT);

            if (Configuration.IsBuild)
                buildType = "Build";
            else
                buildType = "Rebuild";
            if (Configuration.IsRelease)
                outputType = "Release";
            else
                outputType = "Debug";


            Arguments = $"-t:{buildType} -r:{CurrentSettings.IsRestore} -p:Configuration={outputType} -noWarn:CS1591 -nologo";
            IsEnabled = true;
        }

        public override async Task<RunStatusType> Run(CancellationToken cancellationToken)
        {
            var result = await base.Run(cancellationToken);
            //var output = result.StartInfo;
            //Status = Models.RunStatusType.Done;
            //Status = Models.RunStatusType.Error;
            return result;
        }
    }
}
