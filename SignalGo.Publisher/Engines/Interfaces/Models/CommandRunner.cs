using System;
using System.IO;
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
    public class CommandRunner //: IDisposable
    {
        static readonly string CommandsLogPath = Path.Combine(UserSettingInfo.Current.UserSettings.CommandRunnerLogsPath);

        public async static Task<RunStatusType> Run(ICommand command, CancellationToken cancellationToken)
        {
            command.Size = 0;
            command.Position = 0;
            string standardOutputResult;
            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo();
                if (!File.Exists(CommandsLogPath))
                {
                    await File.Create(CommandsLogPath).DisposeAsync();
                }

                await command.Initialize(processInfo);
                using var process = Process.Start(processInfo);
                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Debug.WriteLine("Cancellation Is Requested in CommandRunner");
                        return RunStatusType.Canceled;
                    }
                    standardOutputResult = await process.StandardOutput.ReadLineAsync();
                    if (standardOutputResult == null)
                        break;

                    // to reduce log file size

                    //if (standardOutputResult.TrimStart().StartsWith("Skipping") || standardOutputResult.TrimStart().StartsWith("Copying"))
                    //{
                    //    continue;
                    //}
                    //else
                    //{
                    await File.AppendAllTextAsync(CommandsLogPath, standardOutputResult + Environment.NewLine, cancellationToken);
                    //}

                    // check if compiler return error, or test runner return error
                    if (command.CalculateStatus(standardOutputResult))
                    {
                        return command.Status;
                    }
                }
                command.Status = RunStatusType.Done;
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "CommandRunner(Run)");
                Thread.Sleep(500);
            }
            finally
            {

            }
            return command.Status;
        }

        #region IDisposable Support
        //private bool disposedValue = false; // To detect redundant calls

        //protected virtual void Dispose(bool disposing)
        //{
        //    if (!disposedValue)
        //    {
        //        if (disposing)
        //        {
        //        }

        //        disposedValue = true;
        //    }
        //}

        //~CommandRunner()
        //{
        //    Dispose(false);
        //}

        //public void Dispose()
        //{
        //    Dispose(true);
        //    GC.SuppressFinalize(this);
        //}
        #endregion
    }
}
