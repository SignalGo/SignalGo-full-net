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
            //bool isTestsFound = false;
            var process = new Process();
            command.Size = 0;
            command.Position = 0;
            //int position = 0;
            string standardOutputResult;
            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo();
                if (!File.Exists(CommandsLogPath))
                {
                    File.Create(CommandsLogPath).Close();
                }

                await command.Initialize(processInfo);
                process = Process.Start(processInfo);
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
                    await File.AppendAllTextAsync(CommandsLogPath, standardOutputResult + Environment.NewLine);

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
            return command.Status;
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
