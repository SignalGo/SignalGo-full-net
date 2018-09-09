using System.Collections.Generic;

namespace SignalGo.Shared.Models.ServiceReference
{
    public class NamespaceReferenceInfo
    {
        public string Name { get; set; }
        public List<string> Usings { get; set; } = new List<string>();

        public List<ClassReferenceInfo> Classes { get; set; } = new List<ClassReferenceInfo>();
        public List<EnumReferenceInfo> Enums { get; set; } = new List<EnumReferenceInfo>();
    }
}
