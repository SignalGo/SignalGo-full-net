using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Shared.Helpers
{
    /// <summary>
    /// helper of reflection methods
    /// </summary>
    public static class ReflectionHelper
    {
        /// <summary>
        /// get Nested Types
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetListOfNestedTypes(this Type type)
        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
            var typeInfo = type.GetTypeInfo();
#else
            Type typeInfo = type;
#endif
#if (PORTABLE)
            return typeInfo.DeclaredNestedTypes.Select(x => x.AsType());
#else
            return typeInfo.GetNestedTypes();
#endif
        }

        /// <summary>
        /// get Nested Types
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetListOfBaseTypes(this Type type)
        {
            List<Type> result = new List<Type>();
            Type parent = type;
            while (parent != null)
            {

                result.Add(parent);
#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
                parent = parent.GetTypeInfo().BaseType;
#else
                parent = parent.BaseType;
#endif
            }
            return result;
        }

        /// <summary>
        /// interfaces
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetListOfInterfaces(this Type type)
        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
            var typeInfo = type.GetTypeInfo();
#else
            Type typeInfo = type;
#endif
#if (PORTABLE)
            return typeInfo.ImplementedInterfaces;
#else
            return typeInfo.GetInterfaces();
#endif
        }
        /// <summary>
        /// get base type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetBaseType(this Type type)
        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
            return type.GetTypeInfo().BaseType;
#else
            return type.BaseType;
#endif
        }
        /// <summary>
        /// is generic type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool GetIsGenericType(this Type type)
        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
            return type.GetTypeInfo().IsGenericType;
#else
            return type.IsGenericType;
#endif
        }
        /// <summary>
        /// get property
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name">property name</param>
        /// <returns></returns>
        public static PropertyInfo GetPropertyInfo(this Type type, string name)
        {
            return type
#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
                .GetTypeInfo()
#endif
#if (PORTABLE)
                .GetDeclaredProperty(name);
#else
                .GetProperty(name);
#endif
        }
        /// <summary>
        /// get method
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static MethodInfo FindMethod(this Type type, string name)
        {
            return type
#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
                .GetTypeInfo()
#endif
#if (PORTABLE)
                .GetDeclaredMethod(name);
#else
                .GetMethod(name);
#endif
        }
        /// <summary>
        /// get list of properties
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetListOfProperties(this Type type)
        {
            return type
#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
                .GetTypeInfo()
#endif
#if (PORTABLE)
                .DeclaredProperties;
#else
                .GetProperties();
#endif
        }
        /// <summary>
        /// IsAssignableFrom
        /// </summary>
        /// <param name="type"></param>
        /// <param name="newType"></param>
        /// <returns></returns>
        public static bool GetIsAssignableFrom(this Type type, Type newType)
        {
            return type
#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
                .GetTypeInfo()
#endif
#if (PORTABLE)
                .IsAssignableFrom(newType.GetTypeInfo());
#else
                .IsAssignableFrom(newType);
#endif
        }

        /// <summary>
        /// is class
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool GetIsClass(this Type type)
        {
            return type
#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
                .GetTypeInfo()
#endif
#if (PORTABLE)
                .IsClass;
#else
                .IsClass;
#endif
        }
        /// <summary>
        /// is interface
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool GetIsInterface(this Type type)
        {
            return type
#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
                .GetTypeInfo()
#endif
#if (PORTABLE)
                .IsInterface;
#else
                .IsInterface;
#endif
        }
        /// <summary>
        /// IS ENUM
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool GetIsEnum(this Type type)
        {
            return type
#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
                .GetTypeInfo()
#endif
#if (PORTABLE)
                .IsEnum;
#else
                .IsEnum;
#endif
        }
        /// <summary>
        /// get list of methods
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<MethodInfo> GetListOfMethods(this Type type)
        {
            return type
#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
                .GetTypeInfo()
#endif
#if (PORTABLE || NETSTANDARD1_6 || NETCOREAPP1_1)
                .DeclaredMethods;
#else
                .GetMethods();
#endif

        }
        /// <summary>
        /// get generic Arguments
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetListOfGenericArguments(this Type type)
        {
#if (PORTABLE)
            return type.GenericTypeArguments;
#elif (NETSTANDARD1_6 || NETCOREAPP1_1)
            return type.GetTypeInfo().GetGenericArguments();
#else
            return type.GetGenericArguments();
#endif
        }
    }
}
