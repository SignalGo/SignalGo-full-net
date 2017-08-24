using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.Models
{
    public class RSAAESEncryptionData
    {
        public string RSAEncryptionKey { get; set; }
        public byte[] Key { get; set; }
        public byte[] IV { get; set; }
    }

    public class SecuritySettingsInfo
    {
        public SecurityMode SecurityMode { get; set; }
        public RSAAESEncryptionData Data { get; set; }
    }
}
