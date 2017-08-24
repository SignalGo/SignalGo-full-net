using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System
{
    /// <summary>
    /// Attribute extensions
    /// </summary>
    public static class AttributeHelper
    {
        /// <summary>Searches and returns attributes. The inheritance chain is not used to find the attributes.</summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="type">The type which is searched for the attributes.</param>
        /// <returns>Returns all attributes.</returns>
        public static T[] GetCustomAttributes<T>(this Type type) where T : Attribute
        {
            return GetCustomAttributes(type, typeof(T), false).Select(arg => (T)arg).ToArray();
        }

        /// <summary>Searches and returns attributes.</summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="type">The type which is searched for the attributes.</param>
        /// <param name="inherit">Specifies whether to search this member's inheritance chain to find the attributes. Interfaces will be searched, too.</param>
        /// <returns>Returns all attributes.</returns>
        public static T[] GetCustomAttributes<T>(this Type type, bool inherit) where T : Attribute
        {
            return GetCustomAttributes(type, typeof(T), inherit).Select(arg => (T)arg).ToArray();
        }

        /// <summary>Private helper for searching attributes.</summary>
        /// <param name="type">The type which is searched for the attribute.</param>
        /// <param name="attributeType">The type of attribute to search for.</param>
        /// <param name="inherit">Specifies whether to search this member's inheritance chain to find the attribute. Interfaces will be searched, too.</param>
        /// <returns>An array that contains all the custom attributes, or an array with zero elements if no attributes are defined.</returns>
        private static object[] GetCustomAttributes(Type type, Type attributeType, bool inherit)
        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var typeInfo = type.GetTypeInfo();
#else
            var typeInfo = type;
#endif
            if (!inherit)
            {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                return typeInfo.GetCustomAttributes(attributeType, false).Cast<object>().ToArray();
#else
                return typeInfo.GetCustomAttributes(attributeType, false);
#endif
            }

            var attributeCollection = new Collection<object>();
            var baseType = typeInfo;

            do
            {
                baseType.GetCustomAttributes(attributeType, true).Apply(attributeCollection.Add);
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                baseType = baseType.BaseType.GetTypeInfo();
#else
                baseType = baseType.BaseType;
#endif
            }
            while (baseType != null);

            foreach (var interfaceType in type.GetInterfaces())
            {
                GetCustomAttributes(interfaceType, attributeType, true).Apply(attributeCollection.Add);
            }

            var attributeArray = new object[attributeCollection.Count];
            attributeCollection.CopyTo(attributeArray, 0);
            return attributeArray;
        }

        static ConcurrentDictionary<Type, List<Type>> SavedTypesOfAttribute = new ConcurrentDictionary<Type, List<Type>>();
        public static List<Type> GetTypesByAttribute<T>(this Type type, bool isManual = true) where T : Attribute
        {
            List<Type> result = new List<Type>();
            if (type == null)
                return result;
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var typeInfo = type.GetTypeInfo();
#else
            var typeInfo = type;
#endif
            if (SavedTypesOfAttribute.ContainsKey(type))
                return SavedTypesOfAttribute[type];
            foreach (var attrib in typeInfo.GetCustomAttributes(false))
            {
                if (attrib.GetType() == typeof(T))
                {
                    if (!result.Contains(type))
                        result.Add(type);
                    break;
                }
            }

            foreach (var interfaceType in type.GetInterfaces())
            {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                var interfaceTypeInfo = interfaceType.GetTypeInfo();
#else
            var interfaceTypeInfo = interfaceType;
#endif
                foreach (var attrib in interfaceTypeInfo.GetCustomAttributes(false))
                {
                    if (attrib.GetType() == typeof(T))
                    {
                        if (!result.Contains(interfaceType))
                            result.Add(interfaceType);
                        break;
                    }
                }
            }
            result.AddRange(typeInfo.BaseType.GetTypesByAttribute<T>(isManual: false));
            if (isManual)
                SavedTypesOfAttribute[type] = result;
            return result;
        }

        /// <summary>Applies a function to every element of the list.</summary>
        private static void Apply<T>(this IEnumerable<T> enumerable, Action<T> function)
        {
            foreach (var item in enumerable)
            {
                function.Invoke(item);
            }
        }
    }

}
