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
        //private UserSetting CurrentSettings
        //{
        //    get
        //    {
        //        return UserSettingInfo.Current.UserSettings;
        //    }
        //}

        private string buildType = "Rebuild";
        private string outputType = "Debug";
        private UserSetting Configuration = UserSettingInfo.Current.UserSettings;

        /// <summary>
        /// MsBuild
        /// </summary>
        public BuildCommandInfo()
        {
            Name = "compile dotnet project";
            ExecutableFile = "cmd.exe";
            Command = $"{UserSettingInfo.Current.UserSettings.MsbuildPath} ";
            //int MinT, maxT, CurrentT, IOT;
            //ThreadPool.GetAvailableThreads(out maxT, out IOT);
            //var tx = ThreadPool.SetMaxThreads(CurrentSettings.MaxThreads, CurrentSettings.MaxThreads);
            //ThreadPool.GetAvailableThreads(out maxT, out IOT);
            buildType = Configuration.IsBuild ? "Build" : "Rebuild";
            outputType = Configuration.IsRelease ? "Release" : "Debug";

            Arguments = $"-t:{buildType} -r:{Configuration.IsRestore} -p:Configuration={outputType} -noWarn:CS1591 -nologo";
            IsEnabled = true;
        }

        public override async Task<RunStatusType> Run(CancellationToken cancellationToken)
        {
            var result = await base.Run(cancellationToken);
            return result;
        }
    }
}
