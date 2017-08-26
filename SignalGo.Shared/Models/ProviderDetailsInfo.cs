using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.Models
{
    public class ProviderDetailsInfo
    {
        public List<ServiceDetailsInfo> Services { get; set; } = new List<ServiceDetailsInfo>();
        public WebApiDetailsInfo WebApiDetailsInfo { get; set; } = new WebApiDetailsInfo();
        public ProjectDomainDetailsInfo ProjectDomainDetailsInfo { get; set; } = new ProjectDomainDetailsInfo();
    }
}
