using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using SignalGo.Shared.Log;
using System.Threading.Tasks;
using SignalGo.Publisher.Engines.Interfaces;
using System.Linq;
using System.Collections.Generic;

namespace SignalGo.Publisher.Engines.Models
{
    /// <summary>
    /// Mother of command executers
    /// </summary>
    public static class CommandRunner //: IDisposable
    {
        static string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CommandRunnerLogs.txt");
        /// <summary>
        /// Runner Of Commands
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async static Task<Process> Run(ICommand command)
        {
            var process = new Process();
            command.Size = 0;
            command.Position = 0;
            int position = 0;
            //List<string> outStr = new List<string>();
            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo
                {
                    RedirectStandardOutput = true,
                    FileName = command.ExecutableFile,
                    CreateNoWindow = true,
                    Arguments = $"/c {command.Command} {command.Arguments}",
                    WorkingDirectory = command.WorkingPath
                };
                process = Process.Start(processInfo);
                var projectCount = LoadSlnProjectsCount(command.WorkingPath);
                command.Size = projectCount;
                while (true)
                {
                    string standardOutputResult = await process.StandardOutput.ReadToEndAsync();
                    await File.WriteAllTextAsync(logFilePath, standardOutputResult);

                    if (standardOutputResult == null || standardOutputResult.Contains("Time Elapsed"))
                        break;
                    if (standardOutputResult.Contains("Done Building"))
                    {
                        position++;
                        command.Position = position;
                    }
                    Debug.WriteLine($"Progress {position} from {projectCount}");
                }
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "CommandRunner(Run)");
                Thread.Sleep(500);
            }
            return process;
        }

        public static int LoadSlnProjectsCount(string path)
        {
            int count = 0;
            var slnFile = Directory.GetFiles(path, "*.*").FirstOrDefault(x => x.EndsWith(".sln", StringComparison.OrdinalIgnoreCase));
            try
            {
                foreach (var item in File.ReadAllLines(slnFile))
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

        #region IDisposable Support
        //private bool disposedValue = false; // To detect redundant calls

        //protected virtual void Dispose(bool disposing)
        //{
        //    if (!disposedValue)
        //    {
        //        if (disposing)
        //        {
        //            // TODO: dispose managed state (managed objects).

        //        }

        //        // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
        //        // TODO: set large fields to null.

        //        disposedValue = true;
        //    }
        //}

        //// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        //// ~CommandRunner()
        //// {
        ////   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        ////   Dispose(false);
        //// }

        //// This code added to correctly implement the disposable pattern.
        //public void Dispose()
        //{
        //    // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //    Dispose(true);
        //    // TODO: uncomment the following line if the finalizer is overridden above.
        //    GC.SuppressFinalize(this);
        //}
        #endregion
    }
}
