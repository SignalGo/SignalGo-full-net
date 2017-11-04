using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Shared.Models
{
    /// <summary>
    /// server callback classes
    /// </summary>
    public class CallbackServiceDetailsInfo
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
        /// <summary>
        /// clone class
        /// </summary>
        /// <returns></returns>
        public CallbackServiceDetailsInfo Clone()
        {
            return new CallbackServiceDetailsInfo() { Id = Id, Comment = Comment, FullNameSpace = FullNameSpace, NameSpace = NameSpace, ServiceName = ServiceName, IsSelected = IsSelected, IsExpanded = IsExpanded };
        }
    }
}
