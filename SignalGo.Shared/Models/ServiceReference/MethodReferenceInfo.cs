using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Shared.Models.ServiceReference
{
    public class MethodReferenceInfo
    {
        public string Name { get; set; }
        public string ReturnTypeName { get; set; }
        public List<ParameterReferenceInfo> Parameters { get; set; } = new List<ParameterReferenceInfo>();
    }
}
