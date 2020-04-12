using SignalGo.Publisher.Engines.Models;
using System.Diagnostics;
using System.IO.Compression;
using System.Threading.Tasks;

namespace SignalGo.Publisher.Engines.Commands
{
    public class PublishCommandInfo : CommandBaseInfo
    {
        public PublishCommandInfo() : base()
        {
            Name = "upload to servers";
            ExecutableFile = "cmd.exe";
            Command = "dotnet ";
            Arguments = $"publish -nologo";
            IsEnabled = true;
        }

        public override async Task<Process> Run()
        {
            var result = await base.Run();
            //var output = result.StartInfo;
            //Status = Models.RunStatusType.Done;
            //Status = Models.RunStatusType.Error;
            var compressedData = await Compress();
            await Upload(compressedData, null, true);

            return result;
        }

        public override async Task<string> Compress(CompressionMethodType compressionMethod = CompressionMethodType.Zip, bool includeParent = false, CompressionLevel compressionLevel = CompressionLevel.Fastest)
        {
            return await base.Compress();
        }

        /// <summary>
        /// Call Decompress (for test, naturaly it called by server manager stream service)
        /// </summary>
        /// <param name="compressionMethod"></param>
        /// <returns></returns>
        public override async Task DeCompress(CompressionMethodType compressionMethod = CompressionMethodType.Zip)
        {
            await base.DeCompress();

        }
    }
}
