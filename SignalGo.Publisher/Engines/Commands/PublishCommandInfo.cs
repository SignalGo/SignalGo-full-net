using SignalGo.Publisher.Engines.Models;
using SignalGo.Shared.Models;
using System.Diagnostics;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace SignalGo.Publisher.Engines.Commands
{
    public class PublishCommandInfo : CommandBaseInfo
    {
        //public PublishCommandInfo() : base()
        //{
        //    Name = "upload to servers";
        //    ExecutableFile = "cmd.exe";
        //    Command = "dotnet ";
        //    Arguments = $"publish -nologo";
        //    IsEnabled = true;
        //}
        public PublishCommandInfo(ServiceContract serviceContract) : base()
        {
            Name = "upload to servers";
            ExecutableFile = "cmd.exe";
            Command = "dotnet ";
            Arguments = $"publish -nologo";
            IsEnabled = true;
            ServiceName = serviceContract.Name;
            ServiceKey = serviceContract.ServiceKey;
        }
        public override async Task<Process> Run(CancellationToken cancellationToken)
        {
            var result = await base.Run(cancellationToken);
            //var output = result.StartInfo;
            //Status = Models.RunStatusType.Done;
            //Status = Models.RunStatusType.Error;
            var compressedData = await Compress();
            await Upload(compressedData, cancellationToken, null, true);
            return result;
        }

        public override async Task<string> Compress(CompressionMethodType compressionMethod = CompressionMethodType.Zip, bool includeParent = false, CompressionLevel compressionLevel = CompressionLevel.Fastest)
        {
            return await base.Compress();
        }
    }
}
