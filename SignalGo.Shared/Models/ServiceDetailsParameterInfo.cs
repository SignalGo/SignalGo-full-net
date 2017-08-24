using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.Models
{
    /// <summary>
    /// details of parameter
    /// </summary>
    public class ServiceDetailsParameterInfo
    {
        /// <summary>
        /// name of parameter
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// type of parameter
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// full type name of parameter
        /// </summary>
        public string FullTypeName { get; set; }
        /// <summary>
        /// value of parameter
        /// </summary>
        public object Value { get; set; }
        /// <summary>
        /// is value json type
        /// </summary>
        public bool IsJson { get; set; }
        /// <summary>
        /// comment of class
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// example template of request data
        /// </summary>
        public string TemplateValue { get; set; }
    }
}
