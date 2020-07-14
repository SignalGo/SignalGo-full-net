using System;
using SignalGo.Publisher.Engines.Interfaces;
using SignalGo.Shared.Log;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SignalGo.Publisher.Engines.Models;
using SignalGo.Publisher.Models;
using SignalGo.Publisher.Models.Extra;
using System.IO;

namespace SignalGo.Publisher.Engines.Commands
{
    /// <summary>
    /// Queue Command's To Run And Report Log's
    /// </summary>
    public class QueueCommandInfo : CommandBaseInfo
    {
        public List<ICommand> Commands { get; set; }
        public bool IsSuccess { get; set; } = false;

        public QueueCommandInfo(IEnumerable<ICommand> commands)
        {
            Commands = commands.ToList();
        }

        /// <summary>
        /// Read the excecuted command logs (verbosity based on IsFullLogging in user settings) default read last 5 line of logs
        /// </summary>
        /// <param name="caller">who asked command logs (for display in ui sector's)</param>
        /// <param name="recent">only read the recent logs (reverse)</param>
        /// <param name="count">Get specific number of lines from the logs</param>
        /// <returns></returns>
        public async Task ReadCommandLog(string caller, bool recent = false, int count = 5)
        {
            IEnumerable<string> logs;
            // if the user had defined log verbosity to full:
            if (UserSettingInfo.Current.UserSettings.LoggingVerbosity == UserSetting.LoggingVerbosityEnum.Minimuum)
            {
                // take latest logs based on the count is defined 
                logs = File.ReadLines(UserSettingInfo.Current.UserSettings.CommandRunnerLogsPath).TakeLast(count);
            }
            // log verbosity minimuum:
            else if (UserSettingInfo.Current.UserSettings.LoggingVerbosity == UserSetting.LoggingVerbosityEnum.Full && !recent)
            {
                // take all logs
                logs = await File.ReadAllLinesAsync(UserSettingInfo.Current.UserSettings.CommandRunnerLogsPath);
            }
            else
            {
                logs = Enumerable.Empty<string>();
            }
            foreach (string item in logs)
            {
                LogModule.AddLog(caller, SectorType.Builder, item, DateTime.Now.ToLongTimeString(), LogTypeEnum.Compiler);
            }
            for (int i = 0; i < ServerInfo.ServerLogs.Count; i++)
            {
                LogModule.AddLog(caller, SectorType.Server, ServerInfo.ServerLogs[i], DateTime.Now.ToLongTimeString(), LogTypeEnum.Compiler);
            }
        }

        public override async Task<RunStatusType> Run(CancellationToken cancellationToken, string caller)
        {

            Status = RunStatusType.Running;
            try
            {
                foreach (var item in Commands)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        LogModule.AddLog(caller, SectorType.Builder, "Cancellation Requested in Run Queue", DateTime.Now.ToLongTimeString(), LogTypeEnum.Compiler);
                        return Status = RunStatusType.Canceled;
                    }

                    var res = await item.Run(cancellationToken, caller);
                    if (res == RunStatusType.Error)
                        return Status = RunStatusType.Error;
                    // read the last command log's
                    await ReadCommandLog(caller);
                }
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "Queue task Run");
            }
            return Status = RunStatusType.Done;
        }

        public override bool CalculateStatus(string line)
        {
            return false;
        }
    }
}
