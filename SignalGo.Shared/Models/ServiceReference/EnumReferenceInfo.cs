using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Shared.Models.ServiceReference
{
    public class EnumReferenceInfo
    {
        public string Name { get; set; }
        public string TypeName { get; set; }
        public List<KeyValue<string, string>> KeyValues { get; set; } = new List<KeyValue<string, string>>();
    }
}
