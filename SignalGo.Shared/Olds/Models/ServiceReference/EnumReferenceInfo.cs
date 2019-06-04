using System.Collections.Generic;

namespace SignalGo.Shared.Models.ServiceReference
{
    public class EnumReferenceInfo
    {
        public string Name { get; set; }
        public string TypeName { get; set; }
        public string NameSpace { get; set; }
        public List<KeyValue<string, string>> KeyValues { get; set; } = new List<KeyValue<string, string>>();
    }
}
