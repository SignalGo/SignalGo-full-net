// Licensed to the ali.visual.studio@gmail.com under one or more agreements.
// The license this file to you under the GNU license.
// See the LICENSE file in the project root for more information.
//https://github.com/Ali-YousefiTelori
//https://github.com/SignalGo/SignalGo-full-net

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
