using System;
using System.Collections.Generic;
using System.Text;

namespace SignalGo.Shared.Models
{
    public class WebApiDetailsInfo
    {
        /// <summary>
        /// id of class
        /// </summary>
        public int Id { get; set; }
        public List<HttpControllerDetailsInfo> HttpControllers { get; set; } = new List<HttpControllerDetailsInfo>();
        /// <summary>
        /// if item is exanded from treeview
        /// </summary>
        public bool IsExpanded { get; set; }
        /// <summary>
        /// if item is selected from treeview
        /// </summary>
        public bool IsSelected { get; set; }
        public WebApiDetailsInfo Clone()
        {
            return new WebApiDetailsInfo() { Id = Id, HttpControllers= new List<HttpControllerDetailsInfo>(), IsSelected = IsSelected, IsExpanded = IsExpanded };
        }
    }
}
