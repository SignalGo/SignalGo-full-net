using System;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using SignalGo.Publisher.Engines.Interfaces;
using SignalGo.Shared.Log;
using System.Text;
using System.IO;

namespace SignalGo.Publisher.Engines.Models
{
    public static class CommandRunner
    {
        /// <summary>
        /// Runner Of Commands
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async static Task<Process> Run(ICommand command)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                //CreateNoWindow = true,
                //RedirectStandardInput = true,
                //StandardOutputEncoding = Encoding.UTF8,
                RedirectStandardOutput = true,
                FileName = command.ExecutableFile,
                Arguments = $"/c {command.Command} {command.Arguments}",
                WorkingDirectory = command.Path
            };
            var process = Process.Start(processInfo);
            try
            {
                var outStr = process.StandardOutput.ReadToEnd();
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CommandRunnerLogs.txt"), outStr);
                Debug.WriteLine(outStr);

            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "CommandRunner(Run)");
                Thread.Sleep(500);
            }
            return process;
        }

    }
}
