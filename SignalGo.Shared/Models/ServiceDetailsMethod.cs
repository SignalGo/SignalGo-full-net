using SignalGo.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.Models
{
    /// <summary>
    /// method of service
    /// </summary>
    public class ServiceDetailsMethod
    {
        /// <summary>
        /// id of class
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// name of method
        /// </summary>
        public string MethodName { get; set; }
        /// <summary>
        /// comment of class
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// comment of return type
        /// </summary>
        public string ReturnComment { get; set; }
        /// <summary>
        /// comment of exceptions
        /// </summary>
        public string ExceptionsComment { get; set; }
        /// <summary>
        /// return type
        /// </summary>
        public string ReturnType { get; set; }
        /// <summary>
        /// test example to call thi method
        /// </summary>
        public string TestExample { get; set; }
        /// <summary>
        /// list of parameters
        /// </summary>
        public List<ServiceDetailsParameterInfo> Parameters { get; set; }
    }

    public class MethodParameterDetails
    {
        public string ServiceName { get; set; }
        public string MethodName { get; set; }
        public int ParameterIndex { get; set; }
        public int ParametersCount { get; set; }
    }
}
