using System.Collections.Generic;

namespace SignalGo.Shared.Models
{
    /// <summary>
    /// one of interface
    /// </summary>
    public class ServiceDetailsInterface
    {
        /// <summary>
        /// id of class
        /// </summary>
        public int Id { get; set; }
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
        /// list of methods
        /// </summary>
        public List<ServiceDetailsMethod> Methods { get; set; } = new List<ServiceDetailsMethod>();
        /// <summary>
        /// if item is exanded from treeview
        /// </summary>
        public bool IsExpanded { get; set; }
        /// <summary>
        /// if item is selected from treeview
        /// </summary>
        public bool IsSelected { get; set; }

        public ServiceDetailsInterface Clone()
        {
            return new ServiceDetailsInterface() { Id = Id, Comment = Comment, FullNameSpace = FullNameSpace, Methods = new List<ServiceDetailsMethod>(), NameSpace = NameSpace, IsSelected = IsSelected, IsExpanded = IsExpanded };
        }
    }
}
