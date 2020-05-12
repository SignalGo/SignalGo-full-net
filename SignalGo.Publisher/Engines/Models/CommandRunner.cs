using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using SignalGo.Shared.Log;
using System.Threading.Tasks;
using SignalGo.Publisher.Engines.Interfaces;
using SignalGo.Publisher.Models;

namespace SignalGo.Publisher.Engines.Models
{

    /// <summary>
    /// Mother of command executers
    /// </summary>
    public class CommandRunner : IDisposable
    {
        static readonly string CommandsLogPath = Path.Combine(UserSettingInfo.Current.UserSettings.CommandRunnerLogsPath);

        public async static Task<RunStatusType> Run(ICommand command, CancellationToken cancellationToken)
        {
            bool isTestsFound = false;
            var process = new Process();
            command.Size = 0;
            command.Position = 0;
            int position = 0;
            string standardOutputResult;
            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo();
                if (!File.Exists(CommandsLogPath))
                {
                    File.Create(CommandsLogPath).Close();
                }
                if (command.Command.Contains("git"))
                {
                    //command.Size = testCount;

                    processInfo.RedirectStandardOutput = true;
                    processInfo.FileName = $"{command.ExecutableFile}";
                    processInfo.CreateNoWindow = true;
                    processInfo.Arguments = $"/c {command.Command} {command.Arguments}";
                    processInfo.WorkingDirectory = command.WorkingPath;
                }
                else if (command.Command.Contains("test"))
                {
                    var testCount = await LoadTestsCount(command.WorkingPath);
                    command.Size = testCount;

                    processInfo.RedirectStandardOutput = true;
                    processInfo.FileName = $"{command.ExecutableFile}";
                    processInfo.CreateNoWindow = true;
                    processInfo.Arguments = $"/c {command.Command} {command.Arguments}";
                    processInfo.WorkingDirectory = command.WorkingPath;
                }
                else // comoile
                {
                    var projectCount = LoadSlnProjectsCount(command.WorkingPath);
                    command.Size = projectCount;

                    processInfo.RedirectStandardOutput = true;
                    processInfo.FileName = $"{command.Command}";
                    processInfo.CreateNoWindow = true;
                    processInfo.Arguments = $" {command.Arguments}";
                    processInfo.WorkingDirectory = command.WorkingPath;
                }
                process = Process.Start(processInfo);
                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Debug.WriteLine("Cancellation Is Requested in CommandRunner");
                        return RunStatusType.Canceled;
                    }
                    standardOutputResult = await process.StandardOutput.ReadLineAsync();
                    await File.AppendAllTextAsync(CommandsLogPath, standardOutputResult + Environment.NewLine);
                    if (standardOutputResult.Contains("Done"))
                    {
                        break;
                        //return command.Status = RunStatusType.Done;
                    }
                    if (standardOutputResult.Contains("fatal") || standardOutputResult.Contains("Could not read from remote repository."))
                    {
                        return command.Status = RunStatusType.Error;
                    }
                    if (standardOutputResult == null)
                        break;
                    // check if compiler return error, or test runner return error
                    if (standardOutputResult.Contains("Failed") || standardOutputResult.Contains("Build FAILED"))
                    {
                        return command.Status = RunStatusType.Error;
                    }
                    if (standardOutputResult.Contains("Done Building"))
                    {
                        position++;
                        command.Position = position;
                    }
                    if (isTestsFound)
                    {
                        position++;
                        command.Position = command.Size;
                    }
                    if (!isTestsFound && standardOutputResult.Contains("Test Run Successful"))
                        isTestsFound = true;
                    Debug.WriteLine($"Progress {position} from {command.Size}");
                }
                command.Status = RunStatusType.Done;
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "CommandRunner(Run)");
                Thread.Sleep(500);
            }
            return command.Status;
        }
        static async Task<int> LoadTestsCount(string path)
        {
            int count = 0;
            var process = new Process();
            string standardOutputResult = string.Empty;
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                RedirectStandardOutput = true,
                FileName = $"cmd.exe",
                CreateNoWindow = true,
                Arguments = $"/c dotnet test -t --no-build -v q --nologo",
                WorkingDirectory = path
            };
            try
            {
                process = Process.Start(processInfo);
                bool isTestsFound = false;
                while (true)
                {
                    standardOutputResult = await process.StandardOutput.ReadLineAsync();
                    //Debug.WriteLine(standardOutputResult);
                    if (standardOutputResult == null || standardOutputResult == "")
                        break;
                    else
                    {
                        if (isTestsFound)
                            count++;
                        if (!isTestsFound && standardOutputResult.Contains("Tests are available:"))
                            isTestsFound = true;
                    }
                }
                Debug.WriteLine($"{count} Tests Are Available");
            }
            catch (Exception e)
            {
                AutoLogger.Default.LogError(e, "load tests count");
            }
            finally
            {
                process.Dispose();
            }
            return count;
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
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).

                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~CommandRunner()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
