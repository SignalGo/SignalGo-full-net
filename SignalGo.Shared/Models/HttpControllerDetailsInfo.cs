using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
