using SignalGo.Publisher.Shared.Models;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SignalGo.Publisher.Shared.Helpers
{
    public class FileHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="directoryPath">The target directory which is seeked for files</param>
        /// <param name="delimiter">The specific delimiter by which the file name is extracted</param>
        /// <returns></returns>
        public static List<HashedFileDto> CalculateFileHashesInDirectory(string directoryPath, string delimiter = "\\")
        {
            //Check to see if directoryPath refers to a file or directory
            if (!Directory.Exists(directoryPath))
                directoryPath = Directory.GetParent(directoryPath).FullName;

            var filePaths = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);

            return filePaths?
                //.Select(x => new HashedFileInfo(delimiter)
                .Select(x => new HashedFileDto()
                {
                    FilePath = x,
                    FileHash = HashHelper.ComputeHash(File.ReadAllBytes(x)),
                })
                .ToList() ?? new List<HashedFileDto>();
        }
    }
}
