using SignalGo.Publisher.Engines.Models;
using SignalGo.Publisher.Models;
using System.IO.Compression;
using System.Threading.Tasks;

namespace SignalGo.Publisher.Engines.Interfaces
{
    public interface IPublish : ICommand
    {
        public Task<string> Compress(CompressionMethodType compressionMethod = CompressionMethodType.Zip, bool includeParent = false, CompressionLevel compressionLevel = CompressionLevel.Fastest);
        public Task DeCompress(CompressionMethodType compressionMethod = CompressionMethodType.Zip);
        public Task<TaskStatus> Upload(string dataPath,ServerInfo serverInfo);

    }
}
