using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.Models
{
    public class ProviderDetailsInfo
    {
        public List<ServiceDetailsInfo> Services { get; set; } = new List<ServiceDetailsInfo>();
        public List<HttpControllerDetailsInfo> HttpControllers { get; set; } = new List<HttpControllerDetailsInfo>();
    }
}
