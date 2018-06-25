using SignalGo.Shared.Helpers;
using SignalGo.Shared.Models;
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

        static object[] TryAddCach(object type, bool inherit)
        {

            if (inherit)
            {
                if (InheritCachedCustomAttributes.TryGetValue(type, out object[] result))
                    return result;
                if (type is Type resultType)
                {
                    List<object> items = new List<object>();
                    var baseType = resultType;
                    do
                    {
#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
                        items.AddRange(baseType.GetTypeInfo().GetCustomAttributes().Cast<object>());
#else
                        items.AddRange(baseType.GetCustomAttributes(true).Cast<object>());
#endif

                        baseType = baseType.GetBaseType();
                    }
                    while (baseType != null);

                    foreach (var interfaceType in resultType.GetListOfInterfaces())
                    {
#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
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
#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
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
#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
            var typeInfo = type.GetTypeInfo();
#else
            var typeInfo = type;
#endif
            //if (ContainsCachKey(type, inherit))
            //    return GetCachValues(type, inherit).Where(x => x.GetType() == attributeType).ToArray();
            return TryAddCach(type, inherit).Where(x => x.GetType() == attributeType).ToArray();
            //            if (!inherit)
            //            {
            ////                object[] cach = null;
            ////#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
            ////                cach = typeInfo.GetCustomAttributes(attributeType, false).Cast<object>().ToArray();
            ////#else
            ////                cach = typeInfo.GetCustomAttributes(attributeType, false);
            ////#endif
            ////                if (!ContainsCachKey(type, inherit))
            ////                {
            ////                    foreach (var item in cach)
            ////                        AddCach(type, item, inherit);
            ////                }
            //                return AddCach(type, inherit).Where(x => x.GetType() == attributeType).ToArray();
            //            }

            //            var attributeCollection = new Collection<object>();
            //            var baseType = typeInfo;

            //            do
            //            {
            //                baseType.GetCustomAttributes(attributeType, true).Apply(attributeCollection.Add);
            //#if (NETSTANDARD1_6 || NETCOREAPP1_1 ||PORTABLE)
            //                baseType = baseType.BaseType == null ? null : baseType.BaseType.GetTypeInfo();
            //#else
            //                baseType = baseType.BaseType;
            //#endif
            //            }
            //            while (baseType != null);

            //            foreach (var interfaceType in type.GetListOfInterfaces())
            //            {
            //                GetCustomAttributes(interfaceType, attributeType, true).Apply(attributeCollection.Add);
            //            }

            //            var attributeArray = new object[attributeCollection.Count];
            //            attributeCollection.CopyTo(attributeArray, 0);
            //            if (!ContainsCachKey(type, inherit))
            //            {
            //                foreach (var item in attributeArray)
            //                    AddCach(type, item, inherit);
            //            }

            //            return AddCach(type, item, inherit);
        }

        private static object[] GetCustomAttributes(FieldInfo type, Type attributeType, bool inherit)
        {
            //if (ContainsCachKey(type, inherit))
            //    return GetCachValues(type, inherit).Where(x => x.GetType() == attributeType).ToArray();
            return TryAddCach(type, inherit).Where(x => x.GetType() == attributeType).ToArray();
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
            return TryAddCach(type, inherit).Where(x => x.GetType() == attributeType).ToArray();

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
            //if (type.Name == "TokenPassword" && attributeType == typeof(SignalGo.Shared.DataTypes.CustomDataExchangerAttribute))
            //{
            //}
            //if (ContainsCachKey(type, inherit))
            //    return GetCachValues(type, inherit).Where(x => x.GetType() == attributeType).ToArray();
            return TryAddCach(type, inherit).Where(x => x.GetType() == attributeType).ToArray();

            //            if (!inherit)
            //            {
            //                object[] cach = null;
            //#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
            //                cach = type.GetCustomAttributes(attributeType, false).Cast<object>().ToArray();
            //#else
            //                cach = type.GetCustomAttributes(attributeType, false);
            //#endif
            //                if (!ContainsCachKey(type, inherit))
            //                {
            //                    foreach (var item in cach)
            //                        AddCach(type, item, inherit);
            //                }
            //                return cach;
            //            }

            //            var attributeCollection = new Collection<object>();
            //            type.GetCustomAttributes(attributeType, true).Apply(attributeCollection.Add);

            //            var attributeArray = new object[attributeCollection.Count];
            //            attributeCollection.CopyTo(attributeArray, 0);
            //            if (!ContainsCachKey(type, inherit))
            //            {
            //                foreach (var item in attributeArray)
            //                    AddCach(type, item, inherit);
            //            }
            //            return attributeArray;
        }

        public static ConcurrentDictionary<Type, List<Type>> CachedTypesOfAttribute = new ConcurrentDictionary<Type, List<Type>>();
        public static List<Type> GetTypesByAttribute<T>(this Type type, Func<T, bool> canAdd, bool isManual = true) where T : Attribute
        {
            List<Type> result = new List<Type>();
            if (type == null)
                return result;
#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
            var typeInfo = type.GetTypeInfo();
#else
            var typeInfo = type;
#endif
            if (CachedTypesOfAttribute.ContainsKey(type))
                return CachedTypesOfAttribute[type];
            foreach (var attrib in typeInfo.GetCustomAttributes(false))
            {
                if (attrib.GetType() == typeof(T) && canAdd((T)attrib))
                {
                    if (!result.Contains(type))
                        result.Add(type);
                    break;
                }
            }

            foreach (var interfaceType in type.GetListOfInterfaces())
            {
#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
                var interfaceTypeInfo = interfaceType.GetTypeInfo();
#else
                var interfaceTypeInfo = interfaceType;
#endif
                foreach (var attrib in interfaceTypeInfo.GetCustomAttributes(false))
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
            foreach (var item in enumerable)
            {
                function.Invoke(item);
            }
        }
    }

}
