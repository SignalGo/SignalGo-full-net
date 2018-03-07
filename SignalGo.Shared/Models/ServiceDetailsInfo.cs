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
        /// id of class
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// name of service
        /// </summary>
        public string ServiceName { get; set; }
        /// <summary>
        /// name of class
        /// </summary>
        public string NameSpace { get; set; }
        /// <summary>
        /// class name and name space of class
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
        /// <summary>
        /// if item is exanded from treeview
        /// </summary>
        public bool IsExpanded { get; set; }
        /// <summary>
        /// if item is selected from treeview
        /// </summary>
        public bool IsSelected { get; set; }
        /// <summary>
        /// clone class
        /// </summary>
        /// <returns></returns>
        public ServiceDetailsInfo Clone()
        {
            return new ServiceDetailsInfo() { Id = Id, Comment = Comment, FullNameSpace = FullNameSpace, NameSpace = NameSpace, ServiceName = ServiceName, Services = new List<ServiceDetailsInterface>(), IsSelected = IsSelected, IsExpanded = IsExpanded };
        }
    }
}
