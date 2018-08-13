using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.Models
{
    /// <summary>
    /// a parameter data for method call
    /// </summary>
    public class ParameterInfo
    {
        /// <summary>
        /// type of parameter
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// value of parameter
        /// </summary>
        public string Value { get; set; }
    }
}
