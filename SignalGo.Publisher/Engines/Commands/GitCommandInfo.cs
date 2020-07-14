using System;
using SignalGo.Publisher.Engines.Models;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SignalGo.Shared.Log;

namespace SignalGo.Publisher.Engines.Commands
{
    public class GitCommandInfo : CommandBaseInfo
    {
        public GitCommandInfo()
        {
            Name = "Pull Changes From Repository";
            ExecutableFile = "cmd.exe";
            Command = $"git ";
            Arguments = $"pull ";
            IsEnabled = true;
        }

        public override async Task<RunStatusType> Run(CancellationToken cancellationToken, string caller)
        {
            var result = await base.Run(cancellationToken, caller);
            return result;
        }

        public override Task Initialize(ProcessStartInfo processStartInfo)
        {
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.FileName = $"{ExecutableFile}";
            processStartInfo.CreateNoWindow = true;
            processStartInfo.Arguments = $"/c {Command} {Arguments}";
            processStartInfo.WorkingDirectory = WorkingPath;
            return Task.CompletedTask;
        }
        //public void CalcProgress(string str)
        //{
        //    var zx = str.LastIndexOf("Unpacking objects:  ");
        //    long progress = 0;
        //    while (progress <= 100)
        //    {
        //        var p = str.Substring(zx, str.IndexOf("%"));
        //        progress = long.Parse(p);
        //        Position = progress;
        //    }

        //}

        //public void CalcProgress(string str)
        //{
        //    var zx = str.LastIndexOf("Unpacking objects:  ");
        //    long progress = 0;
        //    while (progress <= 100)
        //    {
        //        var p = str.Substring(zx, str.IndexOf("%"));
        //        progress = long.Parse(p);
        //        Position = progress;
        //    }

        //}
        public override bool CalculateStatus(string line)
        {
            try
            {
                Size = 100;

                //int index = 0;
                //if (line.StartsWith("remote: Found "))
                //{
                //    index = line.LastIndexOf("remote: Found ");
                //    int unpackCount = int.Parse(line.Substring(index, 2));
                //    Size = unpackCount;
                //    if (line.StartsWith("Unpacking objects:  "))
                //        CalcProgress(line);
                //    if (line.StartsWith("Aborting"))
                //    {
                //        Status = RunStatusType.Error;
                //        return true;
                //    }
                //    return true;
                //}
                if (line.StartsWith("Already up to date."))
                {
                    Status = RunStatusType.Done;
                    Position = Size;
                    return true;
                }
                else if (line.TrimStart().StartsWith("fatal: ") || line.TrimStart().StartsWith("error") || line.Contains("Could not read from remote repository."))
                {
                    Status = RunStatusType.Error;
                    return true;
                }
                else
                {
                    //Position++;
                    Position = Size;
                }
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "GitCommandInfo");
                return true;
            }
            return false;
        }
    }
}
