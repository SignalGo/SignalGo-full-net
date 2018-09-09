using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SignalGo.Shared.Helpers
{
    /// <summary>
    /// helper of reflection methods
    /// </summary>
    public static class ReflectionHelper
    {

        public static IEnumerable<Shared.Models.ParameterInfo> MethodToParameters(this MethodInfo methodInfo, Func<object, string> serialize, params object[] args)
        {
            ParameterInfo[] methodParams = methodInfo.GetParameters();

            for (int i = 0; i < args.Length; i++)
            {
                yield return new Shared.Models.ParameterInfo() { Name = methodParams[i].Name, Value = serialize(args[i]) };
            }
        }

#if (!NET35)
        public static IEnumerable<Shared.Models.ParameterInfo> MethodToParameters(this System.Dynamic.InvokeMemberBinder methodInfo, Func<object, string> serialize, params object[] args)
        {
            System.Collections.ObjectModel.ReadOnlyCollection<string> methodParams = methodInfo.CallInfo.ArgumentNames;

            for (int i = 0; i < args.Length; i++)
            {
                yield return new Shared.Models.ParameterInfo() { Name = methodParams[i], Value = serialize(args[i]) };
            }
        }
#endif
        /// <summary>
        /// get all project Assemblies
        /// </summary>
        /// <returns></returns>
        public static List<Assembly> GetReferencedAssemblies(Assembly assembly)
        {
#if (PORTABLE)
            throw new NotSupportedException("not support auto register services for portable class library");
#else
            List<Assembly> result = new List<Assembly>();
            result.Add(assembly);


            foreach (AssemblyName reference in assembly.GetReferencedAssemblies())
            {
                Assembly loaded = Assembly.Load(reference);
                result.Add(loaded);
            }
            return result;
            //var list = new List<string>();
            //var stack = new Stack<Assembly>();
            //stack.Push(assembly);
            //do
            //{
            //    var asm = stack.Pop();
            //    yield return asm;
            //    foreach (var reference in asm.GetReferencedAssemblies())
            //        if (!list.Contains(reference.FullName))
            //        {
            //            stack.Push(Assembly.Load(reference));
            //            list.Add(reference.FullName);
            //        }
            //}
            //while (stack.Count > 0);
#endif
        }

        /// <summary>
        /// get Nested Types
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetListOfNestedTypes(this Type type)
        {
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
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
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
                parent = parent.GetTypeInfo().BaseType;
#else
                parent = parent.BaseType;
#endif
            }
            return result;
        }

        public static Assembly GetAssembly(this Type type)
        {

#if (NETSTANDARD || NETCOREAPP || PORTABLE)
            return type.GetTypeInfo().Assembly;
#else
            return type.Assembly;
#endif
        }

        /// <summary>
        /// interfaces
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetListOfInterfaces(this Type type)
        {
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
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
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
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
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
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
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
                .GetTypeInfo()
#endif
#if (PORTABLE)
                .GetDeclaredProperty(name);
#else
                .GetProperty(name, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
#endif
        }
#if (!PORTABLE)
        /// <summary>
        /// get property
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name">property name</param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static PropertyInfo GetPropertyInfo(this Type type, string name, BindingFlags flags)
        {
            return type
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
                .GetTypeInfo()
#endif
                .GetProperty(name, flags | BindingFlags.IgnoreCase);
        }
#endif
        /// <summary>
        /// get method
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static MethodInfo FindMethod(this Type type, string name)
        {
            return type
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
                .GetTypeInfo()
#endif
#if (PORTABLE)
                .GetDeclaredMethod(name);
#else
                .GetMethod(name, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
#endif
        }
        /// <summary>
        /// get method
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static MethodInfo FindMethod(this Type type, string name, Type[] parameterTypes)
        {
            return type
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
                .GetTypeInfo()
#endif
                .GetMethods(BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).FirstOrDefault(x => x.Name == name && x.GetParameters().Count(y => parameterTypes.Any(j => j == y.ParameterType)) == parameterTypes.Length);
        }
#if (!PORTABLE)
        /// <summary>
        /// get method
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static MethodInfo FindMethod(this Type type, string name, BindingFlags flags)
        {
            return type
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
                .GetTypeInfo()
#endif
                .GetMethod(name, flags | BindingFlags.IgnoreCase);
        }
#endif
        /// <summary>
        /// get list of properties
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetListOfProperties(this Type type)
        {
            return type
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
                .GetTypeInfo()
#endif
#if (PORTABLE)
                .DeclaredProperties;
#else
                .GetProperties(BindingFlags.Public |
                          BindingFlags.Instance | BindingFlags.IgnoreCase);
#endif
        }

        /// <summary>
        /// get list of declared properties
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetListOfDeclaredProperties(this Type type)
        {
            return type
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
                .GetTypeInfo()
#endif
#if (PORTABLE)
                .DeclaredProperties;
#else
                .GetProperties(BindingFlags.Public |
                          BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly);
#endif
        }

        /// <summary>
        /// get list of fields
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<FieldInfo> GetListOfFields(this Type type)
        {
            return type
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
                .GetTypeInfo()
#endif
#if (PORTABLE)
                .DeclaredFields;
#else
                .GetFields(BindingFlags.Public |
                          BindingFlags.Instance | BindingFlags.IgnoreCase);
#endif
        }

        public static FieldInfo GetFieldInfo(this Type type, string name)
        {
            return type
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
                .GetTypeInfo()
#endif
#if (PORTABLE)
                .GetDeclaredField(name);
#else
                .GetField(name, BindingFlags.Public |
                          BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.NonPublic);
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
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
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
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
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
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
                .GetTypeInfo()
#endif
#if (PORTABLE)
                .IsInterface;
#else
                .IsInterface;
#endif
        }

        public static ConstructorInfo[] GetListOfConstructors(this Type type)
        {
            return type
#if (PORTABLE)
                .GetTypeInfo()
#endif
#if (PORTABLE)
                .DeclaredConstructors.ToArray();
#else
                .GetConstructors();
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
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
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
        public static IEnumerable<MethodInfo> GetListOfDeclaredMethods(this Type type)
        {
            return type
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
                .GetTypeInfo()
#endif
#if (PORTABLE || NETSTANDARD || NETCOREAPP)
                .DeclaredMethods;
#else
                .GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
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
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
                .GetTypeInfo()
#endif
#if (PORTABLE || NETSTANDARD || NETCOREAPP)
                .DeclaredMethods;
#else
                .GetMethods();
#endif

        }

        public static List<MethodInfo> GetListOfMethodsWithAllOfBases(this Type type)
        {
            List<MethodInfo> methods = new List<MethodInfo>();
            foreach (Type item in type.GetListOfInterfaces())
            {
                methods.AddRange(item.GetListOfMethods());
            }
            foreach (Type item in type.GetListOfBaseTypes())
            {
                methods.AddRange(item.GetListOfMethods());
            }
            return methods;
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
#elif (NETSTANDARD || NETCOREAPP)
            return type.GetTypeInfo().GetGenericArguments();
#else
            return type.GetGenericArguments();
#endif
        }

        public static Delegate CreateDelegate(Type type, MethodInfo method)
        {
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
            return method.CreateDelegate(type);
#else
            return Delegate.CreateDelegate(type, method);
#endif
        }
    }
}
