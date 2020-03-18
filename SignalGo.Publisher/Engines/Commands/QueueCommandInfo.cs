using SignalGo.Publisher.Engines.Interfaces;
using SignalGo.Shared.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SignalGo.Publisher.Engines.Commands
{
    public class QueueCommandInfo : CommandBaseInfo
    {
        public List<ICommand> Commands { get; set; }
        public bool IsSuccess { get; set; } = false;
        public QueueCommandInfo(IEnumerable<ICommand> commands)
        {
            Commands = commands.ToList();
        }

        public override async Task<Process> Run()
        {
            var proc = new Process();
            Status = Models.RunStatusType.Running;
            try
            {
                foreach (var item in Commands)
                {
                    proc = await item.Run();
                    if (proc.ExitCode != 0)
                    {
                        IsSuccess = false;
                        Status = Models.RunStatusType.Error;
                    }
                    var outStr = proc.StandardOutput.ReadToEnd();
                    Console.WriteLine(outStr);
                    await System.IO.File.AppendAllTextAsync(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                     "CommandRunnerLogs.txt"),
                        outStr);
                }
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "QueueRunner");
            }
            Status = Models.RunStatusType.Done;
            return proc;
        }
    }
}
