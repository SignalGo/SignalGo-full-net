using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.DataTypes
{
    /// <summary>
    /// generate server service by custom url with custom services and methods
    /// just use this over your services or methods
    /// </summary>
    public class GenerationServiceAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="urls">block of url example: Web, Mobile, AdminPanel</param>
        public GenerationServiceAttribute(params string[] urls)
        {
            Urls = urls;
        }

        /// <summary>
        /// block of url example: Web, Mobile, AdminPanel
        /// </summary>
        public string[] Urls { get; set; }
    }
}
