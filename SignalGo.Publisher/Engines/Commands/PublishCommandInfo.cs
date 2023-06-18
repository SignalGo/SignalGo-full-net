using SignalGo.Publisher.Engines.Models;
using SignalGo.Publisher.Shared.Models;
using SignalGo.Shared.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
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
        public string ServerDefaultSolutionShortName { get; set; }
        /// <summary>
        /// this command will run dotnet sdk publish command, before it, rebuild must be called
        /// </summary>
        /// <param name="serviceContract"></param>
        public PublishCommandInfo(ServiceContract serviceContract, string serverDefaultSolutionShortName) : base()
        {
            ServerDefaultSolutionShortName = serverDefaultSolutionShortName;
            // title of command in Queue list
            Name = "upload to servers";
            // executable shell binary 
            ExecutableFile = "cmd.exe";
            // command which run in shell
            Command = "dotnet ";
            // command are avail/not available
            IsEnabled = true;
            // name of project service (in server)
            ServiceName = serviceContract.Name;
            // key of project that must integrate with key of service in ServerManager
            ServiceKey = serviceContract.ServiceKey;
            IsEnabled = true;
        }
        /// <summary>
        /// run publish tasks like get output and compressed data then upload
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<RunStatusType> Run(CancellationToken cancellationToken, string caller)
        {
            var runStatus = await base.Run(cancellationToken, caller);
            var compressedData = await Compress();
            var result = await Upload(compressedData, cancellationToken, true);
            return Status = result;
        }

        /// <summary>
        /// compress project to an archive for fastest upload
        /// </summary>
        /// <param name="compressionMethod"></param>
        /// <param name="includeParent"></param>
        /// <param name="compressionLevel"></param>
        /// <returns></returns>
        public override async Task<List<CompressArchiveDto>> Compress(CompressionMethodType compressionMethod = CompressionMethodType.Zip, bool includeParent = false, CompressionLevel compressionLevel = CompressionLevel.Fastest, bool compressOnlyChanges = true)
        {
            return await base.Compress();
        }

        public override async Task Initialize(ProcessStartInfo processStartInfo)
        {
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.FileName = $"{Command}";
            processStartInfo.CreateNoWindow = true;
            processStartInfo.WorkingDirectory = WorkingPath;
            var solutionFile = GetSolutionFileName(WorkingPath, ServerDefaultSolutionShortName);
            var netVersion = AssembliesPath.Split('\\').LastOrDefault();
            if (!string.IsNullOrEmpty(netVersion))
                netVersion = $"--framework {netVersion}";
            // args to send to command
            Arguments = $"publish {solutionFile} --no-build -nologo {netVersion}";
            processStartInfo.Arguments = $" {Arguments}";
        }
        public override bool CalculateStatus(string line)
        {
            return false;
        }
    }
}
