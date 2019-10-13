using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.DataTypes
{
    public enum ProtocolType
    {
        None = 0,
        HttpGet = 1,
        HttpPost = 2
    }

    public class ProtocolAttribute : Attribute
    {
        public ProtocolType Type { get; set; } = ProtocolType.HttpPost;
    }
}
