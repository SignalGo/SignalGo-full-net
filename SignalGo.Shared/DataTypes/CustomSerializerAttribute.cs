using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.DataTypes
{
    /// <summary>
    /// custom serialize your object to do it faster
    /// </summary>
    public abstract class CustomOutputSerializerAttribute : Attribute
    {
        /// <summary>
        /// serialize data to string
        /// </summary>
        /// <param name="data"></param>
        /// <param name="serverBase"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public abstract string Serialize(object data, object serverBase, object client);
        /// <summary>
        /// deserialize string to object
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <param name="serverBase"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public abstract object Deserialize(Type type, string data, object serverBase, object client);
    }
}
