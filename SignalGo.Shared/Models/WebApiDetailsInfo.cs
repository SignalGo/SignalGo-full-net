using System;
using System.Collections.Generic;
using System.Text;

namespace SignalGo.Shared.Models
{
    public class WebApiDetailsInfo
    {
        public List<HttpControllerDetailsInfo> HttpControllers { get; set; } = new List<HttpControllerDetailsInfo>();
    }
}
