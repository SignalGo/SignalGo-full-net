using System;
using SignalGo.Publisher.Engines.Interfaces;
using SignalGo.Shared.Log;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SignalGo.Publisher.Engines.Models;
using SignalGo.Publisher.Models.Extra;

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
                    //await ReadCommandLog(caller);
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
