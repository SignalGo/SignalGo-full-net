using System;

namespace SignalGo.Shared.DataTypes
{
    /// <summary>
    /// 
    /// </summary>
    public enum PriorityCallType
    {
        IgnoreHolding = 0
    }
    /// <summary>
    /// 
    /// </summary>
    public class PriorityCallAttribute : Attribute
    {
        public PriorityCallType Type { get; set; }
        public PriorityCallAttribute(PriorityCallType type)
        {
            Type = type;
        }
    }
}
