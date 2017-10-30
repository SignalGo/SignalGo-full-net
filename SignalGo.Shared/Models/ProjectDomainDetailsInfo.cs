using System;
using System.Collections.Generic;
using System.Text;

namespace SignalGo.Shared.Models
{
    public class ProjectDomainDetailsInfo
    {
        /// <summary>
        /// id of class
        /// </summary>
        public int Id { get; set; }
        public List<ModelDetailsInfo> Models { get; set; } = new List<ModelDetailsInfo>();
        /// <summary>
        /// if item is exanded from treeview
        /// </summary>
        public bool IsExpanded { get; set; }
        /// <summary>
        /// if item is selected from treeview
        /// </summary>
        public bool IsSelected { get; set; }
        public ProjectDomainDetailsInfo Clone()
        {
            return new ProjectDomainDetailsInfo() { Id = Id, Models = new List<ModelDetailsInfo>(), IsSelected = IsSelected, IsExpanded = IsExpanded };
        }
    }
}
