using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SignalGo.Publisher.Engines.Models;

namespace SignalGo.Publisher.Engines.Interfaces
{
    public interface IRunnable
    {
        public bool IsEnabled { get; set; }
        Task<Process> Run();
        public RunStatusType Status { get; set; }

    }
}
