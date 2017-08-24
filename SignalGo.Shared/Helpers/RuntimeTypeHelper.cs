using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SignalGo.Shared.Helpers
{
    
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
    }
}
