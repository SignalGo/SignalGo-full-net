using SignalGo.Shared.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        /// <summary>Searches and returns attributes. The inheritance chain is not used to find the attributes.</summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="type">The type which is searched for the attributes.</param>
        /// <returns>Returns all attributes.</returns>
        public static T[] GetCustomAttributes<T>(this FieldInfo type) where T : Attribute
        {
            return GetCustomAttributes(type, typeof(T), false).Select(arg => (T)arg).ToArray();
        }

        /// <summary>Searches and returns attributes.</summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="type">The type which is searched for the attributes.</param>
        /// <param name="inherit">Specifies whether to search this member's inheritance chain to find the attributes. Interfaces will be searched, too.</param>
        /// <returns>Returns all attributes.</returns>
        public static T[] GetCustomAttributes<T>(this FieldInfo type, bool inherit) where T : Attribute
        {
            return GetCustomAttributes(type, typeof(T), inherit).Select(arg => (T)arg).ToArray();
        }

        /// <summary>Searches and returns attributes. The inheritance chain is not used to find the attributes.</summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="type">The type which is searched for the attributes.</param>
        /// <returns>Returns all attributes.</returns>
        public static T[] GetCustomAttributes<T>(this MethodInfo type) where T : Attribute
        {
            return GetCustomAttributes(type, typeof(T), false).Select(arg => (T)arg).ToArray();
        }

        /// <summary>Searches and returns attributes.</summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="type">The type which is searched for the attributes.</param>
        /// <param name="inherit">Specifies whether to search this member's inheritance chain to find the attributes. Interfaces will be searched, too.</param>
        /// <returns>Returns all attributes.</returns>
        public static T[] GetCustomAttributes<T>(this MethodInfo type, bool inherit) where T : Attribute
        {
            return GetCustomAttributes(type, typeof(T), inherit).Select(arg => (T)arg).ToArray();
        }

        /// <summary>Searches and returns attributes. The inheritance chain is not used to find the attributes.</summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="type">The type which is searched for the attributes.</param>
        /// <returns>Returns all attributes.</returns>
        public static T[] GetCustomAttributes<T>(this PropertyInfo type) where T : Attribute
        {
            return GetCustomAttributes(type, typeof(T), false).Select(arg => (T)arg).ToArray();
        }

        /// <summary>Searches and returns attributes.</summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="type">The type which is searched for the attributes.</param>
        /// <param name="inherit">Specifies whether to search this member's inheritance chain to find the attributes. Interfaces will be searched, too.</param>
        /// <returns>Returns all attributes.</returns>
        public static T[] GetCustomAttributes<T>(this PropertyInfo type, bool inherit) where T : Attribute
        {
            return GetCustomAttributes(type, typeof(T), inherit).Select(arg => (T)arg).ToArray();
        }

        public static ConcurrentDictionary<object, object[]> InheritCachedCustomAttributes = new ConcurrentDictionary<object, object[]>();
        public static ConcurrentDictionary<object, object[]> CachedCustomAttributes = new ConcurrentDictionary<object, object[]>();

        private static object[] TryAddCach(object type, bool inherit)
        {

            if (inherit)
            {
                if (InheritCachedCustomAttributes.TryGetValue(type, out object[] result))
                    return result;
                if (type is Type resultType)
                {
                    List<object> items = new List<object>();
                    Type baseType = resultType;
                    do
                    {
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
                        items.AddRange(baseType.GetTypeInfo().GetCustomAttributes().Cast<object>());
#else
                        items.AddRange(baseType.GetCustomAttributes(true).Cast<object>());
#endif

                        baseType = baseType.GetBaseType();
                    }
                    while (baseType != null);

                    foreach (Type interfaceType in resultType.GetListOfInterfaces())
                    {
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
                        items.AddRange(interfaceType.GetTypeInfo().GetCustomAttributes().Cast<object>());
#else
                        items.AddRange(interfaceType.GetCustomAttributes(true).Cast<object>());
#endif
                    }
                    InheritCachedCustomAttributes.TryAdd(type, items.ToArray());
                }
                else if (type is MethodInfo resultMethodInfo)
                    InheritCachedCustomAttributes.TryAdd(type, resultMethodInfo.GetCustomAttributes(true).Cast<object>().ToArray());
                else if (type is PropertyInfo resultPropertyInfo)
                    InheritCachedCustomAttributes.TryAdd(type, resultPropertyInfo.GetCustomAttributes(true).Cast<object>().ToArray());
                else if (type is FieldInfo resultFieldInfo)
                    InheritCachedCustomAttributes.TryAdd(type, resultFieldInfo.GetCustomAttributes(true).Cast<object>().ToArray());
            }
            else
            {
                if (CachedCustomAttributes.TryGetValue(type, out object[] result))
                    return result;
                if (type is Type resultType)
                {
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
                    CachedCustomAttributes.TryAdd(type, resultType.GetTypeInfo().GetCustomAttributes().Cast<object>().ToArray());
#else
                    CachedCustomAttributes.TryAdd(type, resultType.GetCustomAttributes(false).Cast<object>().ToArray());
#endif

                }
                else if (type is MethodInfo resultMethodInfo)
                    CachedCustomAttributes.TryAdd(type, resultMethodInfo.GetCustomAttributes(false).Cast<object>().ToArray());
                else if (type is PropertyInfo resultPropertyInfo)
                    CachedCustomAttributes.TryAdd(type, resultPropertyInfo.GetCustomAttributes(false).Cast<object>().ToArray());
                else if (type is FieldInfo resultFieldInfo)
                    CachedCustomAttributes.TryAdd(type, resultFieldInfo.GetCustomAttributes(false).Cast<object>().ToArray());
            }
            return TryAddCach(type, inherit);
        }

        //static bool ContainsCachKey(object type, bool inherit)
        //{
        //    if (inherit)
        //        return InheritCachedCustomAttributes.ContainsKey(type);
        //    else
        //        return CachedCustomAttributes.ContainsKey(type);
        //}

        //static object[] GetCachValues(object type, bool inherit)
        //{
        //    object[] result = null;
        //    if (inherit)
        //        InheritCachedCustomAttributes.TryGetValue(type, out result);
        //    else
        //        CachedCustomAttributes.TryGetValue(type, out result);
        //    return result;
        //}

        /// <summary>Private helper for searching attributes.</summary>
        /// <param name="type">The type which is searched for the attribute.</param>
        /// <param name="attributeType">The type of attribute to search for.</param>
        /// <param name="inherit">Specifies whether to search this member's inheritance chain to find the attribute. Interfaces will be searched, too.</param>
        /// <returns>An array that contains all the custom attributes, or an array with zero elements if no attributes are defined.</returns>
        private static object[] GetCustomAttributes(Type type, Type attributeType, bool inherit)
        {
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
            var typeInfo = type.GetTypeInfo();
#else
            Type typeInfo = type;
#endif
            //if (ContainsCachKey(type, inherit))
            //    return GetCachValues(type, inherit).Where(x => x.GetType() == attributeType).ToArray();
            return TryAddCach(type, inherit).Where(x => x.GetType() == attributeType || x.GetType().IsInstancedOfType(attributeType)).ToArray();
        }

        private static object[] GetCustomAttributes(FieldInfo type, Type attributeType, bool inherit)
        {
            //if (ContainsCachKey(type, inherit))
            //    return GetCachValues(type, inherit).Where(x => x.GetType() == attributeType).ToArray();
            return TryAddCach(type, inherit).Where(x => x.GetType() == attributeType || x.GetType().IsInstancedOfType(attributeType)).ToArray();
            //if (!inherit)
            //{
            //    object[] cach = null;
            //    cach = GetCustomAttributes(type, attributeType, false);
            //    if (!ContainsCachKey(type, inherit))
            //    {
            //        foreach (var item in cach)
            //            AddCach(type, item, inherit);
            //    }
            //    return cach;
            //}

            //var attributeCollection = new Collection<object>();
            //type.GetCustomAttributes(attributeType, true).Apply(attributeCollection.Add);

            //var attributeArray = new object[attributeCollection.Count];
            //attributeCollection.CopyTo(attributeArray, 0);
            //if (!ContainsCachKey(type, inherit))
            //{
            //    foreach (var item in attributeArray)
            //        AddCach(type, item, inherit);
            //}
            //return attributeArray;
        }

        private static object[] GetCustomAttributes(MethodInfo type, Type attributeType, bool inherit)
        {
            //if (ContainsCachKey(type, inherit))
            //    return GetCachValues(type, inherit).Where(x => x.GetType() == attributeType).ToArray();
            return TryAddCach(type, inherit).Where(x => x.GetType() == attributeType || x.GetType().IsInstancedOfType(attributeType)).ToArray();

            //if (!inherit)
            //{
            //    return AddCach(type, inherit);
            //}

            //var attributeCollection = new Collection<object>();
            //type.GetCustomAttributes(attributeType, true).Apply(attributeCollection.Add);

            //var attributeArray = new object[attributeCollection.Count];
            //attributeCollection.CopyTo(attributeArray, 0);
            //if (!ContainsCachKey(type, inherit))
            //{
            //    foreach (var item in attributeArray)
            //        AddCach(type, item, inherit);
            //}
            //return attributeArray;
        }

        private static object[] GetCustomAttributes(PropertyInfo type, Type attributeType, bool inherit)
        {
            return TryAddCach(type, inherit).Where(x => x.GetType() == attributeType || x.GetType().IsInstancedOfType(attributeType)).ToArray();
        }

        public static ConcurrentDictionary<Type, List<Type>> CachedTypesOfAttribute = new ConcurrentDictionary<Type, List<Type>>();
        public static List<Type> GetTypesByAttribute<T>(this Type type, Func<T, bool> canAdd, bool isManual = true) where T : Attribute
        {
            List<Type> result = new List<Type>();
            if (type == null)
                return result;
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
            var typeInfo = type.GetTypeInfo();
#else
            Type typeInfo = type;
#endif
            if (CachedTypesOfAttribute.ContainsKey(type))
                return CachedTypesOfAttribute[type];
            foreach (object attrib in typeInfo.GetCustomAttributes(false))
            {
                if (attrib.GetType() == typeof(T) && canAdd((T)attrib))
                {
                    if (!result.Contains(type))
                        result.Add(type);
                    break;
                }
            }

            foreach (Type interfaceType in type.GetListOfInterfaces())
            {
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
                var interfaceTypeInfo = interfaceType.GetTypeInfo();
#else
                Type interfaceTypeInfo = interfaceType;
#endif
                foreach (object attrib in interfaceTypeInfo.GetCustomAttributes(false))
                {
                    if (attrib.GetType() == typeof(T) && canAdd((T)attrib))
                    {
                        if (!result.Contains(interfaceType))
                            result.Add(interfaceType);
                        break;
                    }
                }
            }
            result.AddRange(typeInfo.BaseType.GetTypesByAttribute<T>(canAdd, isManual: false));
            if (isManual)
                CachedTypesOfAttribute[type] = result;
            return result;
        }

        /// <summary>Applies a function to every element of the list.</summary>
        private static void Apply<T>(this IEnumerable<T> enumerable, Action<T> function)
        {
            foreach (T item in enumerable)
            {
                function.Invoke(item);
            }
        }
    }

}
