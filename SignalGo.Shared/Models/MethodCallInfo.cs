using System.Collections.Generic;

namespace SignalGo.Shared.Models
{
    /// <summary>
    /// signal go call method type
    /// </summary>
    public enum MethodType : byte
    {
        /// <summary>
        /// programmer type
        /// </summary>
        User = 0,
        /// <summary>
        /// signalGo type
        /// </summary>
        SignalGo = 1
    }

    /// <summary>
    /// call info class is data for call client or server method
    /// </summary>
    public class MethodCallInfo : ISegment
    {
        /// <summary>
        /// method access code
        /// </summary>
        public string Guid { get; set; }
        /// <summary>
        /// service name in client or server from ServiceContract class
        /// </summary>
        public string ServiceName { get; set; }
        /// <summary>
        /// method name in client or server from ServiceContract class 
        /// </summary>
        public string MethodName { get; set; }
        /// <summary>
        /// data to send
        /// </summary>
        public object Data { get; set; }
        /// <summary>
        /// method parameters
        /// </summary>
        public List<ParameterInfo> Parameters { get; set; }
        /// <summary>
        /// sender of call from ignalGo service or not
        /// </summary>
        public MethodType Type { get; set; } = MethodType.User;

        /// <summary>
        /// Part number of call method
        /// </summary>
        public short PartNumber { get; set; }

        public MethodCallInfo Clone()
        {
            MethodCallInfo mci = (MethodCallInfo)MemberwiseClone();
            mci.Parameters = Parameters;
            return mci;
        }
    }
}
