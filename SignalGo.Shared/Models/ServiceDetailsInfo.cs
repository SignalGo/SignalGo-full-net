using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.Models
{
    /// <summary>
    /// service detail of project
    /// </summary>
    public class ServiceDetailsInfo
    {
        /// <summary>
        /// name of service
        /// </summary>
        public string ServiceName { get; set; }
        /// <summary>
        /// name of class
        /// </summary>
        public string NameSpace { get; set; }
        /// <summary>
        /// name and namce space of class
        /// </summary>
        public string FullNameSpace { get; set; }
        /// <summary>
        /// comment of class
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// list of service interfaces
        /// </summary>
        public List<ServiceDetailsInterface> Services { get; set; } = new List<ServiceDetailsInterface>();
    }
}
