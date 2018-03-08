using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Shared.DataTypes
{
    /// <summary>
    /// map your client model to service reference
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ModelMappAttribute : Attribute
    {
        /// <summary>
        /// if you don't want to Inheritance NotifyPropertyChangedBase class just set it to false
        /// </summary>
        public bool IsEnabledNotifyPropertyChangedBaseClass { get; set; } = true;
        /// <summary>
        /// when you want to include your inheritance of this class
        /// this will automatic ignore name of classes in service that is main class
        /// </summary>
        public bool IsIncludeInheritances { get; set; } = true;
        /// <summary>
        /// map properties of this class to your service reference type
        /// </summary>
        public Type MapToType { get; set; }
        /// <summary>
        /// ignore properties to generate from server
        /// </summary>
        public string[] IgnoreProperties { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ModelMappAttribute(Type mapToType)
        {
            MapToType = mapToType;
        }
    }
}
