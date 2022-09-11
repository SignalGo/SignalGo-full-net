using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Publisher.Shared.Helpers
{
    //idea from: https://stackoverflow.com/a/35416368/18026723
    public static class ZipHelper
    {
        /// <summary>
        /// Creates an archive file by iterating through sourceFiles but excludes specified statuses.
        /// </summary>
        /// <param name="sourceFiles"></param>
        /// <param name="destinationArchiveFileName">archive file name</param>
        /// <param name="compressionLevel"></param>
        /// <param name="excludedStates">file types that are not intended to be part of the resulted archive(zip)</param>
        public static void CreateFromFileList(List<HashedFileDto> sourceFiles, string destinationArchiveFileName, CompressionLevel compressionLevel, params FileStatusType[] excludedStates)
        {
            //using declarations does not support in c# 7
            using (var zipFileStream = new FileStream(destinationArchiveFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create))
                {
                    foreach (var fileInfo in sourceFiles)
                    {
                        if (excludedStates != null && !excludedStates.Contains(fileInfo.FileStatus))
                            archive.CreateEntryFromFile(fileInfo.FilePath, fileInfo.FileName, compressionLevel);
                    }
                }
            }
        }
    }
}
