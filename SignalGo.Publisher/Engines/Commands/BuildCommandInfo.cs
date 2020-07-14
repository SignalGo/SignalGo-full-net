using SignalGo.Publisher.Engines.Models;
using SignalGo.Publisher.Models;
using SignalGo.Shared.Log;
using System;
using System.Diagnostics;
using System.IO;
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

            Arguments = $"-t:{buildType} -r:{Configuration.IsRestore} -p:Configuration={outputType} -noWarn:MSB4011;CS1591 -nologo";
            IsEnabled = true;
        }

        public override async Task<RunStatusType> Run(CancellationToken cancellationToken, string caller)
        {
            var result = await base.Run(cancellationToken,caller);
            return result;
        }

        public override async Task Initialize(ProcessStartInfo processStartInfo)
        {
            var projectCount = await LoadSlnProjectsCount(WorkingPath);
            Size = projectCount;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.FileName = $"{Command}";
            processStartInfo.CreateNoWindow = true;
            processStartInfo.Arguments = $" {Arguments}";
            processStartInfo.WorkingDirectory = WorkingPath;
        }

        public static async Task<int> LoadSlnProjectsCount(string path)
        {
            int count = 0;
            var slnFile = Directory.GetFiles(path, "*.*").FirstOrDefault(x => x.EndsWith(".sln", StringComparison.OrdinalIgnoreCase));
            try
            {
                foreach (var item in await File.ReadAllLinesAsync(slnFile))
                {
                    if (item.Contains("Project("))
                    {
                        var pPath = item.Split(',')[1].Replace("\"", "").Trim();
                        if (pPath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                        {
                            var projectPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(slnFile), pPath));
                            var findLine = File.ReadAllLines(projectPath).FirstOrDefault(x => !x.Contains("<!--") && x.Contains("<TargetFrameworks>"));
                            if (findLine != null)
                            {
                                count += findLine.Split(';').Count();
                            }
                            count++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "CommandRunner(LoadSlnProjectsCount)");
            }
            return count;
        }

        public override bool CalculateStatus(string line)
        {
            if (line.Contains("Done Building"))
            {
                Position++;
            }
            else if (line.StartsWith("Build FAILED."))
            {
                Status = RunStatusType.Error;
                return true;
            }
            return false;
        }
    }
}
