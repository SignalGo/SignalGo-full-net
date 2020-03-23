using SignalGo.Shared.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SignalGo.Shared.Helpers
{
    /// <summary>
    /// helper of reflection methods
    /// </summary>
    public static class ReflectionHelper
    {

//        public static IEnumerable<Shared.Models.ParameterInfo> MethodToParameters(this MethodInfo methodInfo, Func<object, string> serialize, params object[] args)
//        {
//            ParameterInfo[] methodParams = methodInfo.GetParameters();

//            for (int i = 0; i < args.Length; i++)
//            {
//                yield return new Shared.Models.ParameterInfo() { Name = methodParams[i].Name, Value = serialize(args[i]) };
//            }
//        }

//        public static IEnumerable<Shared.Models.ParameterInfo> MethodToParameters(this Models.ServiceDetailsMethod methodInfo, List<Models.ServiceDetailsParameterInfo> parameters, Func<object, string> serialize)
//        {
//            foreach (Models.ServiceDetailsParameterInfo parameterInfo in parameters)
//            {
//                yield return new Shared.Models.ParameterInfo() { Name = parameterInfo.Name, Value = serialize(parameterInfo.Value) };
//            }
//        }

//#if (!NET35)
//        public static IEnumerable<Shared.Models.ParameterInfo> MethodToParameters(this System.Dynamic.InvokeMemberBinder methodInfo, Func<object, string> serialize, params object[] args)
//        {
//            System.Collections.ObjectModel.ReadOnlyCollection<string> methodParams = methodInfo.CallInfo.ArgumentNames;

//            for (int i = 0; i < args.Length; i++)
//            {
//                yield return new Shared.Models.ParameterInfo() { Name = methodParams[i], Value = serialize(args[i]) };
//            }
//        }
//#endif
        /// <summary>
        /// get all project Assemblies
        /// </summary>
        /// <returns></returns>
        public static List<Assembly> GetReferencedAssemblies(Assembly assembly)
        {
            List<Assembly> result = new List<Assembly>();
            result.Add(assembly);


            foreach (AssemblyName reference in assembly.GetReferencedAssemblies())
            {
                Assembly loaded = Assembly.Load(reference);
                result.Add(loaded);
            }
            return result;
        }

        /// <summary>
        /// get Nested Types
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetListOfNestedTypes(this Type type)
        {
#if (NETSTANDARD || NETCOREAPP)
            var typeInfo = type.GetTypeInfo();
#else
            Type typeInfo = type;
#endif
            return typeInfo.GetNestedTypes();
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
#if (NETSTANDARD || NETCOREAPP)
                parent = parent.GetTypeInfo().BaseType;
#else
                parent = parent.BaseType;
#endif
            }
            return result;
        }

        public static Assembly GetAssembly(this Type type)
        {

#if (NETSTANDARD || NETCOREAPP)
            return type.GetTypeInfo().Assembly;
#else
            return type.Assembly;
#endif
        }

        /// <summary>
        /// get list of types of assemblies
        /// </summary>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetAllTypes(this IEnumerable<Assembly> assemblies)
        {
            foreach (Assembly asm in assemblies)
            {
                foreach (Type type in asm.GetTypes())
                {
                    yield return type;
                }
            }
        }

        /// <summary>
        /// interfaces
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetListOfInterfaces(this Type type)
        {
#if (NETSTANDARD || NETCOREAPP)
            var typeInfo = type.GetTypeInfo();
#else
            Type typeInfo = type;
#endif
            return typeInfo.GetInterfaces();
        }
        /// <summary>
        /// get base type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetBaseType(this Type type)
        {
#if (NETSTANDARD || NETCOREAPP)
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
#if (NETSTANDARD || NETCOREAPP)
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
#if (NETSTANDARD || NETCOREAPP)
                .GetTypeInfo()
#endif
                .GetProperty(name, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }
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
#if (NETSTANDARD || NETCOREAPP)
                .GetTypeInfo()
#endif
                .GetProperty(name, flags | BindingFlags.IgnoreCase);
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
#if (NETSTANDARD || NETCOREAPP)
                .GetTypeInfo()
#endif
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .Where(x => x.Name.ToLower() == name.ToLower()).FirstOrDefault();
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
#if (NETSTANDARD || NETCOREAPP)
                .GetTypeInfo()
#endif
                .GetMethods(BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).FirstOrDefault(x => x.Name == name && x.GetParameters().Count(y => parameterTypes.Any(j => j == y.ParameterType)) == parameterTypes.Length);
        }
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
#if (NETSTANDARD || NETCOREAPP)
                .GetTypeInfo()
#endif
                .GetMethod(name, flags | BindingFlags.IgnoreCase);
        }
        /// <summary>
        /// get list of properties
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetListOfProperties(this Type type)
        {
            return type
#if (NETSTANDARD || NETCOREAPP)
                .GetTypeInfo()
#endif
                .GetProperties(BindingFlags.Public |
                          BindingFlags.Instance | BindingFlags.IgnoreCase);
        }

        /// <summary>
        /// get list of declared properties
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetListOfDeclaredProperties(this Type type)
        {
            return type
#if (NETSTANDARD || NETCOREAPP)
                .GetTypeInfo()
#endif
                .GetProperties(BindingFlags.Public |
                          BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly);
        }

        /// <summary>
        /// get list of fields
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<FieldInfo> GetListOfFields(this Type type)
        {
            return type
#if (NETSTANDARD)
                .GetTypeInfo()
#endif
                .GetFields(BindingFlags.Public |
                          BindingFlags.Instance | BindingFlags.IgnoreCase);
        }

        public static FieldInfo GetFieldInfo(this Type type, string name)
        {
            return type
#if (NETSTANDARD)
                .GetTypeInfo()
#endif
                .GetField(name, BindingFlags.Public |
                          BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.NonPublic);
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
#if (NETSTANDARD)
                .GetTypeInfo()
#endif
                .IsAssignableFrom(newType);
        }

        /// <summary>
        /// is class
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool GetIsClass(this Type type)
        {
            return type
#if (NETSTANDARD)
                .GetTypeInfo()
#endif
                .IsClass;
        }
        /// <summary>
        /// is interface
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool GetIsInterface(this Type type)
        {
            return type
#if (NETSTANDARD)
                .GetTypeInfo()
#endif
                .IsInterface;
        }

        public static ConstructorInfo[] GetListOfConstructors(this Type type)
        {
            return type
                .GetConstructors();
        }
        /// <summary>
        /// IS ENUM
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool GetIsEnum(this Type type)
        {
            return type
#if (NETSTANDARD)
                .GetTypeInfo()
#endif
                .IsEnum;
        }
        /// <summary>
        /// get list of methods
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<MethodInfo> GetListOfDeclaredMethods(this Type type)
        {
            return type
#if (NETSTANDARD)
                .GetTypeInfo()
#endif
                .GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
        }

        /// <summary>
        /// get list of methods
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<MethodInfo> GetListOfMethods(this Type type)
        {
            return type
#if (NETSTANDARD)
                .GetTypeInfo()
#endif
#if (NETSTANDARD)
                .DeclaredMethods;
#else
                .GetMethods();
#endif

        }

        public static List<MethodInfo> GetListOfMethodsWithAllOfBases(this Type type)
        {
            List<MethodInfo> methods = new List<MethodInfo>();
            foreach (Type item in type.GetAllInheritances())
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
#if (NETSTANDARD)
            return type.GetTypeInfo().GetGenericArguments();
#else
            return type.GetGenericArguments();
#endif
        }

        public static Delegate CreateDelegate(Type type, MethodInfo method)
        {
#if (NETSTANDARD)
            return method.CreateDelegate(type);
#else
            return Delegate.CreateDelegate(type, method);
#endif
        }

        /// <summary>
        /// get all of interfaces and base classes
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetAllInheritances(this Type type)
        {
            List<Type> result = new List<Type>();
            foreach (Type item in type.GetListOfBaseTypes())
            {
                if (!result.Contains(item))
                    result.Add(item);
                foreach (Type face in item.GetListOfInterfaces())
                {
                    if (!result.Contains(face))
                        result.Add(face);
                }
            }
            List<MethodInfo> methods = new List<MethodInfo>();
            foreach (Type item in type.GetListOfInterfaces())
            {
                if (!result.Contains(item))
                    result.Add(item);
            }

            return result;
        }

        /// <summary>
        /// is inheritance of type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="newType"></param>
        /// <returns></returns>
        public static bool IsInstancedOfType(this Type type, Type newType)
        {
            return type.GetAllInheritances().Contains(newType);
        }

        /// <summary>
        /// get all of methods that client can access to them
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<MethodInfo> GetFullServiceLevelMethods(this Type type)
        {
            return type.GetAllInheritances().Where(x => x.GetCustomAttributes<ServiceContractAttribute>().Count() > 0).SelectMany(x => x.GetListOfDeclaredMethods()).Distinct()
                  .Where(x => !(x.IsSpecialName && (x.Name.StartsWith("set_") || x.Name.StartsWith("get_"))));
        }
    }
}
