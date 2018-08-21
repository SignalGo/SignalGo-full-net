using System.Collections.Generic;

namespace SignalGo.Shared.Models
{
    public class HttpControllerDetailsInfo
    {
        /// <summary>
        /// id of class
        /// </summary>
        public int Id { get; set; }
        public string Url { get; set; }
        public List<ServiceDetailsMethod> Methods { get; set; } = new List<ServiceDetailsMethod>();
        /// <summary>
        /// if item is exanded from treeview
        /// </summary>
        public bool IsExpanded { get; set; }
        /// <summary>
        /// if item is selected from treeview
        /// </summary>
        public bool IsSelected { get; set; }
        public HttpControllerDetailsInfo Clone()
        {
            return new HttpControllerDetailsInfo() { Id = Id, Url = Url, Methods = new List<ServiceDetailsMethod>(), IsSelected = IsSelected, IsExpanded = IsExpanded };
        }
    }
}
