using System;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using SignalGo.Publisher.Engines.Interfaces;
using SignalGo.Shared.Log;
using System.Text;

namespace SignalGo.Publisher.Engines.Models
{
    public static class CommandRunner
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async static Task<Process> Run(ICommand command)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                //CreateNoWindow = true,
                RedirectStandardOutput = true,
                //RedirectStandardInput = true,
                //StandardOutputEncoding = Encoding.UTF8,
                FileName = command.ExecutableFile,
                Arguments = $"/c {command.Command} {command.Arguments}",
                WorkingDirectory = command.Path
            };
            var process = Process.Start(processInfo);
            //process.BeginOutputReadLine();
            //process.OutputDataReceived += Process_OutputDataReceived;
            try
            {
                var outStr = process.StandardOutput.ReadToEnd();
                Debug.WriteLine(outStr);
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "CommandRunner(Run)");
                Thread.Sleep(500);
            }
            return process;
        }

        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }
    }
}
