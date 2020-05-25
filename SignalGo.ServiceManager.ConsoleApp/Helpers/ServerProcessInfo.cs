using System;
using System.Diagnostics;
using SignalGo.ServiceManager.Models;
using SignalGo.Shared.Log;

namespace SignalGo.ServiceManager.ConsoleApp.Helpers
{
    public class ServerProcessInfo : ServerProcessBaseInfo
    {

        /// <summary>
        /// the shell to execute commands
        /// example: /bin/bash
        /// </summary>
        //const string ShellPath = "/bin/ash"; // or /bin/bash, /bin/sh, /bin/zsh ...
        public override void Start(string command, string assemblyPath, string shell = "/bin/ash")
        {
            try
            {
                BaseProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = shell,
                    Arguments = "-c \" " + "dotnet " + assemblyPath + " \"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                });
                while (!BaseProcess.StandardOutput.EndOfStream)
                {
                    Console.WriteLine(BaseProcess.StandardOutput.ReadLine());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                AutoLogger.Default.LogError(ex, "Start Process Error!");
            }
        }

        public override void Dispose()
        {
            BaseProcess.Kill();
        }

    }
}
