using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.Models
{
    /// <summary>
    /// call is return back from client or server
    /// </summary>
    public class MethodCallbackInfo : ISegment
    {
        /// <summary>
        /// method access code
        /// </summary>
        public string Guid { get; set; }
        /// <summary>
        /// json data
        /// </summary>
        public string Data { get; set; }
        /// <summary>
        /// data is exception
        /// </summary>
        public bool IsException { get; set; }
        /// <summary>
        /// if client have not permision to call method
        /// </summary>
        public bool IsAccessDenied { get; set; }
        /// <summary>
        /// part number 
        /// </summary>
        public short PartNumber { get; set; }

        public MethodCallbackInfo Clone()
        {
            return (MethodCallbackInfo)MemberwiseClone();
        }
    }
}
