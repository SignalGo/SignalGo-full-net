using System;
using System.Collections.Generic;
using System.Text;

namespace SignalGo.Shared.Models
{
    public class CompressArchiveDto
    {
        public string ArchivePath { get; set; }
        public List<HashedFileDto> FileHashes { get; set; }
    }
}
