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
            var methods = serviceType.GetMethods();
#endif
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
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            if (type.GetTypeInfo().IsGenericType)
#else
            if (type.IsGenericType)
#endif
            {
                foreach (var item in type.GetGenericArguments())
                {
                    GetListOfUsedTypes(item, ref findedTypes);
                }
            }
            else
            {
                foreach (var item in type.GetProperties())
                {
                    GetListOfUsedTypes(item.PropertyType, ref findedTypes);
                }
            }
        }
    }
}
