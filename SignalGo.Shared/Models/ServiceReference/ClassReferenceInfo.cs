using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Shared.Models.ServiceReference
{
    public enum ClassReferenceType
    {
        ServiceLevel,
        HttpServiceLevel,
        CallbackLevel,
        ModelLevel,
        StreamLevel
    }
    public class ClassReferenceInfo
    {
        public ClassReferenceType Type { get; set; }
        public string Name { get; set; }
        public string ServiceName { get; set; }
        public string BaseClassName { get; set; }
        public List<MethodReferenceInfo> Methods { get; set; } = new List<MethodReferenceInfo>();
        public List<PropertyReferenceInfo> Properties { get; set; } = new List<PropertyReferenceInfo>();
    }
}
