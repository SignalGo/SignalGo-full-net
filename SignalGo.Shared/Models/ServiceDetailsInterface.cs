using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
