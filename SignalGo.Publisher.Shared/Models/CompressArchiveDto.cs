using SignalGo.Publisher.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SignalGo.Shared.Models
{
    public class CompressArchiveDto
    {
        public Guid TargetServiceKey { get; set; }
        public string ArchivePath { get; set; }
        public List<HashedFileDto> FileHashes { get; set; }
    }
}
