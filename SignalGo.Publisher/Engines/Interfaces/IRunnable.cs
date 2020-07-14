using System.Threading;
using System.Threading.Tasks;
using SignalGo.Publisher.Engines.Models;

namespace SignalGo.Publisher.Engines.Interfaces
{
    public interface IRunnable
    {
        public bool IsEnabled { get; set; }
        //Task<RunStatusType> Run(CancellationToken cancellationToken);
        Task<RunStatusType> Run(CancellationToken cancellationToken, string caller);
        public RunStatusType Status { get; set; }

    }
}
