using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
