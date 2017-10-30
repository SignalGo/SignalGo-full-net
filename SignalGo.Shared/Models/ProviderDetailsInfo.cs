using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.Models
{
    public class ProviderDetailsInfo
    {
        /// <summary>
        /// id of class
        /// </summary>
        public int Id { get; set; }
        public List<ServiceDetailsInfo> Services { get; set; } = new List<ServiceDetailsInfo>();
        public WebApiDetailsInfo WebApiDetailsInfo { get; set; } = new WebApiDetailsInfo();
        public ProjectDomainDetailsInfo ProjectDomainDetailsInfo { get; set; } = new ProjectDomainDetailsInfo();
        /// <summary>
        /// if item is exanded from treeview
        /// </summary>
        public bool IsExpanded { get; set; }
        /// <summary>
        /// if item is selected from treeview
        /// </summary>
        public bool IsSelected { get; set; }
        public ProviderDetailsInfo Clone()
        {
            return new ProviderDetailsInfo() { Id = Id, ProjectDomainDetailsInfo = new ProjectDomainDetailsInfo(), Services = new List<ServiceDetailsInfo>(), WebApiDetailsInfo = new WebApiDetailsInfo(), IsSelected = IsSelected, IsExpanded = IsExpanded };
        }
    }
}
