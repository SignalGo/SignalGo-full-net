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
        public QueueCommandInfo(IEnumerable<ICommand> commands)
        {
            Commands = commands.ToList();
        }

        public override async Task<Process> Run()
        {
            foreach (var item in Commands)
            {
                await item.Run();
            }
            return null;
        }
    }
}
