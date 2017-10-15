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
    }
}
