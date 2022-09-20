using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SignalGo.Publisher.Shared.Models
{
    public class ServiceInspectionDto
    {
        public Guid RemoteServerKey { get; set; }
        public Guid ServiceKey { get; set; }
        public bool IsExist { get; private set; } = false;
        public string ComputedHash { get; set; }
        public string ArchivePath { get; set; }
        public List<HashedFileDto> FileHashes { get; set; } = new List<HashedFileDto>();
        public List<HashedFileDto> ComparedHashes { get; set; } = new List<HashedFileDto>();

        public void MarkAsExist()
        {
            IsExist = true;
        }
    }
}
