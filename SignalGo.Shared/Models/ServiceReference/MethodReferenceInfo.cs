using SignalGo.Shared.DataTypes;
using System.Collections.Generic;

namespace SignalGo.Shared.Models.ServiceReference
{
    public class MethodReferenceInfo
    {
        public string Name { get; set; }
        public string DuplicateName { get; set; }
        public string ReturnTypeName { get; set; }
        public ProtocolType ProtocolType { get; set; } = ProtocolType.HttpPost;
        public List<ParameterReferenceInfo> Parameters { get; set; } = new List<ParameterReferenceInfo>();
    }
}
