using System.Collections.Generic;

namespace SignalGo.Shared.Models.ServiceReference
{
    public enum ClassReferenceType
    {
        ServiceLevel,
        HttpServiceLevel,
        CallbackLevel,
        ModelLevel,
        StreamLevel,
        OneWayLevel,
        InterfaceLevel
    }
    public class ClassReferenceInfo
    {
        public string NameSpace { get; set; }
        public ClassReferenceType Type { get; set; }
        public string Name { get; set; }
        public string ServiceName { get; set; }
        public string BaseClassName { get; set; }
        /// <summary>
        /// example : Where T : struct
        /// </summary>
        public string GenericParameterConstraints { get; set; }
        public List<MethodReferenceInfo> Methods { get; set; } = new List<MethodReferenceInfo>();
        public List<PropertyReferenceInfo> Properties { get; set; } = new List<PropertyReferenceInfo>();
    }
}
