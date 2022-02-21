using System;

namespace SignalGo.Shared.DataTypes
{
    /// <summary>
    /// enable of disable references resolver $Id and $Ref and $values for method output
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ActivityReferenceResolverAttribute : Attribute
    {
        /// <summary>
        /// is enable $if and $ref for method outputes
        /// </summary>
        public bool IsEnabledReferenceResolver { get; set; }
        /// <summary>
        /// es enabled $values for method output
        /// </summary>
        public bool IsEnabledReferenceResolverForArray { get; set; }
    }
}
