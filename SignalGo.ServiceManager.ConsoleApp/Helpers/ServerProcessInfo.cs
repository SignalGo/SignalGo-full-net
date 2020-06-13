using System;
using System.Diagnostics;
using SignalGo.ServiceManager.Core.Models;
using SignalGo.Shared.Log;

namespace SignalGo.ServiceManager.ConsoleApp.Helpers
{
    public class ServerProcessInfo : ServerProcessBaseInfo
    {
        //static readonly string _DotnetPath;

        static ServerProcessInfo()
        {
            //_DotnetPath = UserSettingInfo.Current.UserSettings.DotNetPath;
        }
        /// <summary>
        /// the shell to execute commands
        /// example: /bin/bash
        /// </summary>
        //const string ShellPath = "/bin/ash"; // or /bin/bash, /bin/sh, /bin/zsh ...
        public override void Start(string command, string assemblyPath, string shell = "/bin/ash")
        {
            var processInfo = new ProcessStartInfo();
            try
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    //processInfo.FileName = "cmd";
                    processInfo.UseShellExecute = false;
                    processInfo.RedirectStandardOutput = true;
                    processInfo.FileName = assemblyPath;
                    //processInfo.Arguments = $"{_DotnetPath} {assemblyPath}";
                }
                else if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    processInfo.UseShellExecute = false;
                    processInfo.RedirectStandardOutput = true;
                    processInfo.FileName = shell;
                    processInfo.Arguments = "-c \" " + UserSettingInfo.Current.UserSettings.DotNetPath + " " + assemblyPath + " \"";
                }
                BaseProcess = Process.Start(processInfo);

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
