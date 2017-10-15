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
    }
}
