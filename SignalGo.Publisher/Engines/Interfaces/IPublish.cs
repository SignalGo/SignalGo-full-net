using SignalGo.Publisher.Engines.Models;
using SignalGo.Publisher.Models;
using SignalGo.Shared.Models;
using System;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace SignalGo.Publisher.Engines.Interfaces
{
    /// <summary>
    /// related to publish commands
    /// </summary>
    public interface IPublish : ICommand
    {
        public string ServiceName { get; set; }
        public Guid ServiceKey { get; set; }

        public Task<string> Compress(CompressionMethodType compressionMethod = CompressionMethodType.Zip, bool includeParent = false, CompressionLevel compressionLevel = CompressionLevel.Fastest);
        public Task<RunStatusType> Upload(string dataPath, CancellationToken cancellationToken,bool forceUpdate = false);

    }
}
