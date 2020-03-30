using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using SignalGo.Publisher.Models;
using SignalGo.Publisher.Services;

namespace SignalGo.Publisher.Engines.Commands
{
    public class PublishCommandInfo : CommandBaseInfo
    {
        public PublishCommandInfo()
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
            await Upload();
            return result;
        }

        /// <summary>
        /// test e hanooz
        /// </summary>
        /// <returns></returns>
        public async Task Upload()
        {
            string fileName = @"uploadme.zip";
            //string simpleFilePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), fileName);
            string fileDirPath = $"{System.IO.Path.Combine("D", "DevOps", "LoggerCopy", "Utravs.Hub.Logger", "ConsoleApp", "bin", "Debug", "netcoreapp3.1")}";
            var p = System.IO.Path.Combine(fileName);
            Size = new FileInfo(p).Length;
            var uploadInfo = new UploadInfo(this)
            {
                FileName = fileName,
                //FileExtension = "zip",
                HasProgress = true,
                FilePath = p
            };
            await StreamManagerService.UploadAsync(uploadInfo);

        }

    }
}
