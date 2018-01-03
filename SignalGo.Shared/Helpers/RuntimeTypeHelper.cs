using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SignalGo.Shared.Helpers
{
    /// <summary>
    /// helper of types
    /// </summary>
    public static class RuntimeTypeHelper
    {
        /// <summary>
        /// return types of method parameter
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="callInfo"></param>
        /// <returns></returns>
        public static List<Type> GetMethodTypes(Type serviceType, MethodCallInfo callInfo)
        {
            List<Type> methodParameterTypes = new List<Type>();
#if (NETSTANDARD1_6)
            var methods = serviceType.GetTypeInfo().GetMethods();
#else
            var methods = serviceType.GetListOfMethods();
#endif
            //int sLen = streamType == null ? 0 : 1;
            foreach (var item in methods)
            {
                if (item.Name == callInfo.MethodName)
                {
                    if (item.GetParameters().Length != callInfo.Parameters.Count)
                        continue;
                    foreach (var p in item.GetParameters())
                    {
                        methodParameterTypes.Add(p.ParameterType);
                    }
                    break;
                }
            }
            return methodParameterTypes;
        }

        /// <summary>
        /// get full types of one type that types is in properteis
        /// </summary>
        /// <param name="type">your type</param>
        /// <param name="findedTypes">list of types you want</param>
        public static void GetListOfUsedTypes(Type type, ref List<Type> findedTypes)
        {
            if (!findedTypes.Contains(type))
                findedTypes.Add(type);
            else
                return;
            if (type.GetIsGenericType())
            {
                foreach (var item in type.GetListOfGenericArguments())
                {
                    GetListOfUsedTypes(item, ref findedTypes);
                }
            }
            else
            {
                foreach (var item in type.GetListOfProperties())
                {
                    GetListOfUsedTypes(item.PropertyType, ref findedTypes);
                }
            }
        }
        /// <summary>
        /// Get friendly name of type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetFriendlyName(this Type type)
        {
            if (type == typeof(int))
                return "int";
            else if (type == typeof(short))
                return "short";
            else if (type == typeof(byte))
                return "byte";
            else if (type == typeof(bool))
                return "bool";
            else if (type == typeof(long))
                return "long";
            else if (type == typeof(float))
                return "float";
            else if (type == typeof(double))
                return "double";
            else if (type == typeof(decimal))
                return "decimal";
            else if (type == typeof(string))
                return "string";
            else if (type.GetIsGenericType())
                return type.Name.Split('`')[0] + "<" + string.Join(", ", type.GetListOfGenericArguments().Select(x => GetFriendlyName(x)).ToArray()) + ">";
            else
                return type.Name;
        }
    }
}
