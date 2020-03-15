using SignalGo.Publisher.Engines.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
            foreach (var item in Commands)
            {
                proc = await item.Run();
                if (proc.ExitCode == 0)
                    IsSuccess = true;
            }
            Status = Models.RunStatusType.Done;
            return proc;
        }
    }
}
