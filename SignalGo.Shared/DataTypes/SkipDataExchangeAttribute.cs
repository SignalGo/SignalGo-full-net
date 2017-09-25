using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SignalGo.Shared.DataTypes
{
    /// <summary>
    /// skip any data when serialize or deserializing in signalgo
    /// </summary>
    public class SkipDataExchangeAttribute : Attribute
    {
        /// <summary>
        /// skip mode
        /// </summary>
        public LimitExchangeType Mode { get; private set; }
        /// <summary>
        /// constructor of this attrib neeed your strategy mode
        /// </summary>
        /// <param name="mode">strategy mode</param>
        public SkipDataExchangeAttribute(LimitExchangeType mode)
        {
            Mode = mode;
        }

        /// <summary>
        /// you can create your custom skipper with override this method
        /// when you want to skip to serialize or deserialize your object return true else return false
        /// default value is null, if you want to system use LimitExchangeType for ignore or not ignore object return null
        /// </summary>
        /// <param name="model">object model that want to serialize or deserialize</param>
        /// <param name="property">property of type that want serialize or deserialize,if it is null parameter type is fill</param>
        /// <param name="type">type that want serialize or deserialize</param>
        /// <param name="attribute">attribute</param>
        /// <returns></returns>
        public virtual bool? CanIgnore(object model, PropertyInfo property, Type type, SkipDataExchangeAttribute attribute)
        {
            return null;
        }
    }
}
