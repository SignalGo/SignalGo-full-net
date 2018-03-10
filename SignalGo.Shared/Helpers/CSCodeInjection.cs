#if (!NETSTANDARD1_6 && !NETCOREAPP1_1 && !PORTABLE)
using Microsoft.CSharp;
#endif
using SignalGo.Shared.DataTypes;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SignalGo.Shared.Helpers
{
    /// <summary>
    /// inject runtime code to your types
    /// </summary>
    public static class CSCodeInjection
    {
        /// <summary>
        /// invoke action for void methods call
        /// </summary>
        public static Action<object, MethodInfo, object[]> InvokedClientMethodAction { get; set; }
        /// <summary>
        /// invoke function for non-void methods call
        /// </summary>
        public static Func<object, MethodInfo, object[], object> InvokedClientMethodFunction { get; set; }

        /// <summary>
        /// invoke action for void methods call
        /// </summary>
        public static Action<object, MethodInfo, object[]> InvokedServerMethodAction { get; set; }
        /// <summary>
        /// invoke function for non-void methods call
        /// </summary>
        public static Func<object, MethodInfo, object[], object> InvokedServerMethodFunction { get; set; }
#if (!NETSTANDARD1_6 && !NETCOREAPP1_1 && !PORTABLE)
        /// <summary>
        /// generate a class from an interface type
        /// </summary>
        /// <param name="type">interafce type</param>
        /// <param name="inter">a class or interface must inherited</param>
        /// <param name="assemblyTypes">types of assembly</param>
        /// <returns>return a new type genearted by code injection</returns>
        public static Type GenerateInterfaceType(Type type, Type inter, List<Type> assemblyTypes, bool isServer)
        {
            return GenerateInterfaceServiceType(type, inter, assemblyTypes, isServer);
        }
#endif
        public static T InstanceServerInterface<T>(Type type, List<Type> assemblyTypes)
        {
            var callback = Activator.CreateInstance(type);
            var field = callback.GetType()
                .GetPropertyInfo("InvokedServerMethodAction");

            field.SetValue(callback, new Action<object, MethodInfo, object[]>((t, method, param) =>
            {
                InvokedServerMethodAction?.Invoke(t, method, param);
            }), null);

            var field2 = callback.GetType()
                .GetPropertyInfo("InvokedServerMethodFunction");
            field2.SetValue(callback, new Func<object, MethodInfo, object[], object>((t, method, param) =>
            {
                return InvokedServerMethodFunction?.Invoke(t, method, param);
            }), null);

            return (T)callback;
        }

        public static object InstanceServerInterface(Type type, List<Type> assemblyTypes)
        {
            var callback = Activator.CreateInstance(type);
            var field = callback.GetType()
                .GetPropertyInfo("InvokedServerMethodAction");

            field.SetValue(callback, new Action<object, MethodInfo, object[]>((t, method, param) =>
            {
                InvokedServerMethodAction?.Invoke(t, method, param);
            }), null);

            var field2 = callback.GetType()
                .GetPropertyInfo("InvokedServerMethodFunction");
            field2.SetValue(callback, new Func<object, MethodInfo, object[], object>((t, method, param) =>
            {
                return InvokedServerMethodFunction?.Invoke(t, method, param);
            }), null);

            return callback;
        }
#if (!NETSTANDARD1_6 && !NETCOREAPP1_1 && !PORTABLE)

        static Type GenerateInterfaceServiceType(Type type, Type inter, List<Type> assemblyTypes, bool isServer)
        {
            if (!type.IsInterface)
                throw new Exception("type must be interface");
            var attribs = type.GetCustomAttributes<ServiceContractAttribute>(true).Where(x => x.ServiceType == ServiceType.SeverService);
            bool isServiceContract = false;
            ServiceContractAttribute attrib = attribs.FirstOrDefault();
            isServiceContract = attrib != null;

            if (!isServiceContract)
                throw new Exception("your class is not used ServiceContractAttribute that have ServiceType.SeverService");

            return GenerateType(type, attrib.Name, inter, assemblyTypes, isServer);
        }

        static Type GenerateType(Type type, string className, Type inter, List<Type> assemblyTypes, bool isServer)
        {
            string actionMethodName = isServer ? "InvokedServerMethodAction" : "InvokedClientMethodAction";
            string functionMethodName = isServer ? "InvokedServerMethodFunction" : "InvokedClientMethodFunction";

            string bodyGenerate = " public Action<object,MethodInfo, object[]> " + actionMethodName + " { get; set; } public Func<object,MethodInfo, object[], object> " + functionMethodName + " { get; set; }";
            List<string> foundedNameSpaces = new List<string>();
            List<Type> types = new List<Type>();
            if (assemblyTypes != null)
                types.AddRange(assemblyTypes);
            types.Add(type);
            types.Add(inter);
            if (inter != null)
            {
                foreach (var property in inter.GetProperties())
                {
                    var nameSpace = property.PropertyType.Namespace;
                    if (!foundedNameSpaces.Contains(nameSpace))
                        foundedNameSpaces.Add(nameSpace);
                    if (!types.Contains(property.PropertyType))
                        types.Add(property.PropertyType);
                    bodyGenerate += "public " + GetGenecricTypeString(property.PropertyType, ref types) + " " + property.Name + "{get;set;}";
                }
                foreach (var methd in inter.GetMethods())
                {
                    if (methd.IsSpecialName && (methd.Name.StartsWith("set_") || methd.Name.StartsWith("get_")))
                        continue;
                    var nameSpace = methd.ReturnType.Namespace;
                    if (!foundedNameSpaces.Contains(nameSpace))
                        foundedNameSpaces.Add(nameSpace);
                    if (!types.Contains(methd.ReturnType))
                        types.Add(methd.ReturnType);
                    bodyGenerate += "public " + GetGenecricTypeString(methd.ReturnType, ref types) + " " + methd.Name + "(";
                    var parameters = methd.GetParameters();
                    foreach (var p in parameters)
                    {
                        nameSpace = p.ParameterType.Namespace;
                        if (!foundedNameSpaces.Contains(nameSpace))
                            foundedNameSpaces.Add(nameSpace);
                        if (!types.Contains(p.ParameterType))
                            types.Add(p.ParameterType);
                        bodyGenerate += GetGenecricTypeString(p.ParameterType, ref types) + " " + p.Name + ",";
                    }
                    bodyGenerate = bodyGenerate.Trim(',') + "){}";
                    bodyGenerate = bodyGenerate.Replace("public System.Void", "public void");
                    bodyGenerate = bodyGenerate.Replace("public Void", "public void");
                }
            }

            var interfaces = type.GetInterfaces();
            List<Type> allTypes = new List<Type>();
            allTypes.Add(type);
            allTypes.AddRange(interfaces);
            foreach (var referenceType in allTypes)
            {
                foreach (var method in referenceType.GetMethods())
                {
                    var nameSpace = method.ReturnType.Namespace;
                    if (!foundedNameSpaces.Contains(nameSpace))
                        foundedNameSpaces.Add(nameSpace);
                    string body = "public " + GetGenecricTypeString(method.ReturnType, ref types) + " " + method.Name + "(";
                    string parameterBody = "";
                    var listOfParameters = method.GetParameters().ToList();
                    string invokeBody = "object[] signalGoParametersitems = new  object[" + listOfParameters.Count + "];";

                    foreach (var p in listOfParameters)
                    {
                        nameSpace = p.ParameterType.Namespace;
                        if (!foundedNameSpaces.Contains(nameSpace))
                            foundedNameSpaces.Add(nameSpace);
                        if (!types.Contains(p.ParameterType))
                            types.Add(p.ParameterType);
                        parameterBody += GetGenecricTypeString(p.ParameterType, ref types) + " " + p.Name + ",";
                        invokeBody += "signalGoParametersitems[" + p.Position + "] = " + p.Name + ";";
                    }
                    string returnBody = "";
                    if (method.ReturnType != typeof(void))
                    {
                        nameSpace = method.ReturnType.Namespace;
                        if (!foundedNameSpaces.Contains(nameSpace))
                            foundedNameSpaces.Add(nameSpace);
                        if (!types.Contains(method.ReturnType))
                            types.Add(method.ReturnType);
                        returnBody = "  if (" + functionMethodName + " != null) return (" + GetGenecricTypeString(method.ReturnType, ref types) + ") " + functionMethodName + ".Invoke(this,(MethodInfo)MethodBase.GetCurrentMethod(),signalGoParametersitems); else return default(" + GetGenecricTypeString(method.ReturnType, ref types) + ");";
                    }
                    else
                    {
                        returnBody = "if (" + actionMethodName + " != null) " + actionMethodName + ".Invoke(this,(MethodInfo)MethodBase.GetCurrentMethod(),signalGoParametersitems);";
                        body = body.Replace("public System.Void", "public void");
                        body = body.Replace("public Void", "public void");
                    }
                    body += parameterBody.Trim(',');
                    body += ") {" + invokeBody + " " + returnBody + " }";
                    bodyGenerate += body;
                }
            }


            string[] nameSpaces = { type.Namespace, "System.Reflection", "System" };
            var nameS = "namespace SignalGo.Shared.Runtime {";
            foreach (var item in nameSpaces)
            {
                nameS += "using " + item + "; ";
            }
            string source = nameS + "public class " + className + " : " + type.Namespace + (inter == null ? "" : ("." + type.Name + " , " + inter.Namespace + "." + inter.Name)) + " {" +
                bodyGenerate + "}}";
            var assembly = CompileSource(source, GetTypesOfType(types));
            return assembly.GetType("SignalGo.Shared.Runtime." + className);
        }
#endif
        static string GetGenecricTypeString(Type type, ref List<Type> types)
        {
            string result = type.Namespace + "." + (type.Name.Contains("`") ? type.Name.Remove(type.Name.IndexOf('`')) : type.Name);
            if (type.GetIsGenericType())
            {
                result += "<";
                foreach (var item in type.GetListOfGenericArguments())
                {
                    result += GetGenecricTypeString(item, ref types) + ",";
                    if (!types.Contains(item))
                        types.Add(item);
                }

                result = result.TrimEnd(',');
                result += ">";
            }

            return result;
        }

        //static string GetAttributesOfClass(Type type)
        //{
        //    string attributes = "[";
        //    foreach (CustomAttributeData item in type.GetCustomAttributesData())
        //    {
        //        var attribType = item.Constructor.DeclaringType;
        //        attributes += attribType.Namespace + "." + attribType.Name;
        //        if (item.ConstructorArguments.Count >= 1)
        //        {
        //            string conValues = "";
        //            string typeName = item.Constructor.DeclaringType.Name;
        //            if (typeName.EndsWith("Attribute"))
        //                typeName = typeName.Substring(0, typeName.Length - 9);
        //            foreach (var ca in item.ConstructorArguments)
        //            {
        //                if (ca.ArgumentType == typeof(string))
        //                    conValues += "\"" + ca.Value + "\"";
        //                if (ca.ArgumentType == typeof(char))
        //                    conValues += "'" + ca.Value + "'";

        //            }

        //        }
        //        else
        //        {
        //            attributes += "()]" + Environment.NewLine;
        //        }
        //    }
        //    if (attributes == "[")
        //        return "";
        //    return attributes;
        //}
#if (!NETSTANDARD1_6 && !NETCOREAPP1_1 && !PORTABLE)
        public static Action<CompilerResults, List<Type>, string> OnErrorAction { get; set; }
        private static Assembly CompileSource(string sourceCode, List<Type> types)
        {
            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateInMemory = true;
            parameters.GenerateExecutable = false;

            List<string> assemblies = new List<string>();

            //parameters.ReferencedAssemblies.Add("mscorlib.dll");
            //parameters.ReferencedAssemblies.Add("system.dll");
            foreach (var item in types)
            {
                if (!assemblies.Contains(item.Assembly.Location))
                    assemblies.Add(item.Assembly.Location);
            }
            //if (assemblies != null)
            //{
            //    foreach (var item in assemblies)
            //    {
            //        parameters.ReferencedAssemblies.Add(item);
            //    }
            //}
            //var asm = assem.Assembly.Location;
            foreach (var asm in assemblies)
            {
                parameters.ReferencedAssemblies.Add(asm);
            }
            CompilerResults results = codeProvider.CompileAssemblyFromSource(parameters, sourceCode);
            if (!results.Errors.HasErrors)
            {
                return results.CompiledAssembly;
            }
            else
            {
                OnErrorAction?.Invoke(results, types, sourceCode);
                return null;
            }
        }
#endif


        static List<Type> GetTypesOfType(List<Type> types)
        {
            List<Type> all = new List<Type>();

            foreach (var type in types)
            {
                var nestedTypes = type.GetListOfNestedTypes();
                foreach (var ex in nestedTypes)
                {
                    if (!all.Contains(ex))
                        all.Add(ex);
                }
                var interfaces = type.GetListOfInterfaces();
                foreach (var ex in interfaces)
                {
                    if (!all.Contains(ex))
                        all.Add(ex);
                }
                if (!all.Contains(type))
                    all.Add(type);
            }
            return all;
        }

        public static List<Type> GetListOfTypes(Type type)
        {
            List<Type> all = new List<Type>();
            foreach (var ex in type.GetListOfNestedTypes())
            {
                if (!all.Contains(ex))
                    all.Add(ex);
            }
            foreach (var ex in type.GetListOfInterfaces())
            {
                if (!all.Contains(ex))
                    all.Add(ex);
            }
            var parent = type.GetBaseType();

            while (parent != null)
            {
                all.Add(parent);
                parent = parent.GetBaseType();
            }
            return all;
        }

    }
}