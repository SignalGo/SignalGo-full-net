using SignalGo.Server.ServiceManager;
using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.Models;
using SignalGo.Shared.Models.ServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SignalGo.Server.Helpers
{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public class ServiceReferenceHelper
    //    {
    //        /// <summary>
    //        /// 
    //        /// </summary>
    //        public ServiceReferenceHelper()
    //        {
    //            GenerateBase();
    //        }

    //        List<string> Usings { get; set; } = new List<string>();
    //        List<Assembly> ModellingReferencesAssemblies { get; set; }
    //        /// <summary>
    //        /// type add to this list after generated and dont need to duplicate generate
    //        /// </summary>
    //        List<Type> ModelsCodeGenerated { get; set; } = new List<Type>();
    //        /// <summary>
    //        /// this types must generate after generate this will remove from list
    //        /// </summary>
    //        List<Type> TypeToGenerate { get; set; } = new List<Type>();

    //        string BaseAssemblyPath { get; set; }

    //        private void GenerateBase()
    //        {
    //            BaseAssemblyPath = Environment.GetEnvironmentVariable("SystemRoot");
    //            if (string.IsNullOrEmpty(BaseAssemblyPath))
    //            {
    //#if (NETSTANDARD1_6 || NETCOREAPP1_1)
    //                var assm = typeof(string).GetTypeInfo().Assembly;
    //#else
    //                var assm = typeof(string).Assembly;
    //#endif
    //                var codeBase = assm.CodeBase;
    //                if (codeBase.Length < 32)
    //                    BaseAssemblyPath = codeBase;
    //                else
    //                    BaseAssemblyPath = codeBase.Substring(0, 32);
    //            }
    //            else
    //            {
    //                BaseAssemblyPath = "file:///" + System.IO.Path.Combine(BaseAssemblyPath, "Microsoft.Net").Replace("\\", "/").ToLower();
    //            }
    //        }

    //        internal bool CanGenerateAssemblyTypes(Type type)
    //        {
    //#if (NETSTANDARD1_6 || NETCOREAPP1_1)
    //            var assm = type.GetTypeInfo().Assembly;
    //#else
    //            var assm = type.Assembly;
    //#endif
    //            if (assm.CodeBase.ToLower() == BaseAssemblyPath || assm.CodeBase.ToLower().StartsWith(BaseAssemblyPath))
    //                return true;
    //            return false;
    //        }
    //        /// <summary>
    //        /// 
    //        /// </summary>
    //        /// <param name="nameSpace"></param>
    //        /// <param name="serverBase"></param>
    //        /// <returns></returns>
    //        public byte[] GetServiceReferenceCSharpCode(string nameSpace, ServerBase serverBase)
    //        {
    //            ModellingReferencesAssemblies = serverBase.ModellingReferencesAssemblies;
    //            AddUsingIfNotExist(typeof(ServiceContractAttribute));
    //            AddUsingIfNotExist(typeof(System.Threading.Tasks.Task));
    //            AddUsingIfNotExist(typeof(NotifyPropertyChangedBase));

    //            StringBuilder builder = new StringBuilder();
    //            builder.AppendLine("namespace " + nameSpace);
    //            builder.AppendLine("{");

    //            foreach (var serviceInfo in serverBase.RegisteredServiceTypes)
    //            {
    //                GenerateServiceClass(serviceInfo.Key, serviceInfo.Value, "    ", builder);
    //            }

    //            foreach (var serviceInfo in serverBase.RegisteredHttpServiceTypes)
    //            {
    //                GenerateHttpServiceClass(serviceInfo.Key, serviceInfo.Value, "    ", builder);
    //            }

    //            Type find = null;
    //            while ((find = TypeToGenerate.FirstOrDefault()) != null)
    //            {
    //                if (find.GetIsClass())
    //                    GenerateModelClass(find, "    ", builder);
    //                else if (find.GetIsEnum())
    //                    GenerateModelEnum(find, "    ", builder);


    //                TypeToGenerate.Remove(find);
    //            }
    //            builder.AppendLine("}");

    //            StringBuilder resultBuild = new StringBuilder();
    //            foreach (var item in Usings)
    //            {
    //                resultBuild.AppendLine("using " + item + ";");
    //            }
    //            resultBuild.AppendLine();
    //            resultBuild.AppendLine(builder.ToString());
    //            var text = resultBuild.ToString();

    //            return Encoding.UTF8.GetBytes(text);
    //        }


    //        void AddToGenerate(Type type)
    //        {
    //            if (!TypeToGenerate.Contains(type) && !CanGenerateAssemblyTypes(type) && !type.IsGenericParameter)
    //                TypeToGenerate.Add(type);
    //        }


    //        List<Type> typeGenerated = new List<Type>();
    //        List<MethodInfo> methodGenerated = new List<MethodInfo>();
    //        void GenerateServiceClass(string serviceName, Type type, string prefix, StringBuilder builder)
    //        {
    //            string serviceAttribute = $@"[ServiceContract(""{serviceName}"", InstanceType.SingleInstance)]";
    //            builder.AppendLine(prefix + serviceAttribute);
    //            builder.AppendLine(prefix + "public interface I" + type.Name);
    //            builder.AppendLine(prefix + "{");
    //            foreach (var serviceType in type.GetTypesByAttribute<ServiceContractAttribute>().ToList())
    //            {
    //                if (typeGenerated.Contains(serviceType))
    //                    continue;
    //                typeGenerated.Add(serviceType);
    //                foreach (var methodInfo in serviceType.GetListOfMethodsWithAllOfBases())
    //                {
    //                    GenerateMethod(methodInfo, prefix + prefix, builder);
    //                    GenerateAsyncMethod(methodInfo, prefix + prefix, builder);
    //                    methodGenerated.Add(methodInfo);
    //                }
    //            }
    //            builder.AppendLine(prefix + "}");
    //        }

    //        void GenerateHttpServiceClass(string serviceName, Type type, string prefix, StringBuilder builder)
    //        {
    //            builder.AppendLine(prefix + "public class " + type.Name);
    //            builder.AppendLine(prefix + "{");
    //            foreach (var serviceType in type.GetTypesByAttribute<HttpSupportAttribute>().ToList())
    //            {
    //                if (typeGenerated.Contains(serviceType))
    //                    continue;
    //                typeGenerated.Add(serviceType);
    //                foreach (var methodInfo in serviceType.GetListOfDeclaredMethods().Where(x => !(x.IsSpecialName && (x.Name.StartsWith("set_") || x.Name.StartsWith("get_")))))
    //                {
    //                    GenerateMethod(methodInfo, prefix + prefix, builder, false);
    //                    GenerateAsyncMethod(methodInfo, prefix + prefix, builder, false);
    //                    methodGenerated.Add(methodInfo);
    //                }
    //            }
    //            builder.AppendLine(prefix + "}");
    //        }

    //        void GenerateModelEnum(Type type, string prefix, StringBuilder builder)
    //        {
    //            if (ModelsCodeGenerated.Contains(type) || CanGenerateAssemblyTypes(type))
    //                return;
    //            ModelsCodeGenerated.Add(type);

    //            var fullName = type.Name;
    //            var baseEumType = Enum.GetUnderlyingType(type);
    //            builder.AppendLine(prefix + "public enum " + fullName + " : " + GetFullNameOfType(baseEumType));
    //            builder.AppendLine(prefix + "{");
    //            foreach (var name in Enum.GetValues(type))
    //            {
    //                object res = Convert.ChangeType(name, baseEumType);
    //                string value = "";
    //                if (res != null)
    //                    value = " = " + res.ToString();
    //                builder.AppendLine($"{prefix + prefix}{name}{value},");
    //            }
    //            builder.AppendLine(prefix + "}");
    //            builder.AppendLine();
    //        }

    //        string GenerateModelClass(Type type, string prefix, StringBuilder builder)
    //        {
    //            if (ModelsCodeGenerated.Contains(type) || CanGenerateAssemblyTypes(type))
    //                return " : " + GetFullNameOfType(type);

    //            ModelsCodeGenerated.Add(type);
    //            var baseTypes = type.GetListOfBaseTypes().Where(x => x != typeof(object)).Reverse().ToList();
    //            bool isGenerated = false;
    //            string lastBase = "";
    //            foreach (var item in baseTypes)
    //            {
    //                var fullName = GetFullNameOfType(item);
    //                if (type != item && (ModelsCodeGenerated.Contains(item) || CanGenerateAssemblyTypes(item)))
    //                {
    //                    lastBase = " : " + fullName;
    //                    continue;
    //                }
    //                var isGeneric = item.GetIsGenericType();
    //                if (isGeneric && item.GetGenericTypeDefinition() != item)
    //                {
    //                    lastBase = GenerateModelClass(item.GetGenericTypeDefinition(), prefix, builder);
    //                    continue;
    //                }
    //                if (string.IsNullOrEmpty(lastBase) && baseTypes.Count == 1)
    //                    lastBase = " : " + typeof(NotifyPropertyChangedBase).Name;
    //                isGenerated = true;
    //                ModelsCodeGenerated.Add(item);
    //                builder.AppendLine(prefix + "public class " + fullName + lastBase);
    //                builder.AppendLine(prefix + "{");
    //                foreach (var propertyInfo in item.GetListOfDeclaredProperties())
    //                {
    //                    GenerateProperty(propertyInfo, prefix + prefix, true, builder);
    //                }
    //                builder.AppendLine(prefix + "}");
    //                lastBase = " : " + fullName;
    //            }

    //            if (isGenerated)
    //                builder.AppendLine();
    //            return lastBase;
    //        }

    //        void GenerateMethod(MethodInfo methodInfo, string prefix, StringBuilder builder, bool doSemicolon = true)
    //        {
    //            if (methodGenerated.Contains(methodInfo) || methodInfo.GetParameters().Any(x => x.IsOut || x.IsRetval))
    //                return;
    //            AddToGenerate(methodInfo.ReturnType);
    //            string returnType = "void";
    //            if (methodInfo.ReturnType != typeof(void))
    //                returnType = GetFullNameOfType(methodInfo.ReturnType);
    //            builder.AppendLine($"{prefix}{returnType} {methodInfo.Name}({GenerateMethodParameters(methodInfo)}){(doSemicolon ? ";" : "")}");
    //            //generate empty data
    //            if (!doSemicolon)
    //            {
    //                builder.AppendLine($"{prefix}{{");
    //                if (methodInfo.ReturnType != typeof(void))
    //                {
    //                    var result = (ServerBase.GetDefault(methodInfo.ReturnType) ?? "null").ToString();
    //                    result = result.Replace("False", "false").Replace("True", "true");
    //                    builder.AppendLine($"{prefix + prefix}return {result};");
    //                }

    //                builder.AppendLine($"{prefix}}}");
    //            }
    //        }

    //        void GenerateAsyncMethod(MethodInfo methodInfo, string prefix, StringBuilder builder, bool doSemicolon = true)
    //        {
    //            if (methodGenerated.Contains(methodInfo) || methodInfo.GetParameters().Any(x => x.IsOut || x.IsRetval))
    //                return;
    //            AddToGenerate(methodInfo.ReturnType);
    //            string returnType = "Task";
    //            if (methodInfo.ReturnType != typeof(void))
    //                returnType = "Task<" + GetFullNameOfType(methodInfo.ReturnType) + ">";
    //            builder.AppendLine($"{prefix}{returnType} {methodInfo.Name}Async({GenerateMethodParameters(methodInfo)}){(doSemicolon ? ";" : "")}");
    //            //generate empty data
    //            if (!doSemicolon)
    //            {
    //                builder.AppendLine($"{prefix}{{");
    //                if (methodInfo.ReturnType != typeof(void))
    //                {
    //                    var result = (ServerBase.GetDefault(methodInfo.ReturnType) ?? "null").ToString();
    //                    result = result.Replace("False", "false").Replace("True", "true");
    //                    builder.AppendLine($"{prefix + prefix}return System.Threading.Tasks.{returnType}.Factory.StartNew(() => {result});");
    //                }
    //                else
    //                    builder.AppendLine($"{prefix + prefix}return System.Threading.Tasks.{returnType}.Factory.StartNew(() => {{}});");
    //                builder.AppendLine($"{prefix}}}");
    //            }
    //        }

    //        void GenerateProperty(PropertyInfo propertyInfo, string prefix, bool generateOnPropertyChanged, StringBuilder builder)
    //        {
    //            AddToGenerate(propertyInfo.PropertyType);
    //            //create field
    //            builder.AppendLine($"{prefix}private {GetFullNameOfType(propertyInfo.PropertyType)} _{propertyInfo.Name};");
    //            builder.AppendLine($"{prefix}public {GetFullNameOfType(propertyInfo.PropertyType)} {propertyInfo.Name}");
    //            builder.AppendLine($"{prefix}{{");
    //            if (propertyInfo.CanRead)
    //            {
    //                builder.AppendLine($"{prefix + prefix}get");
    //                builder.AppendLine($"{prefix + prefix}{{");
    //                builder.AppendLine($"{prefix + prefix + prefix}return _{propertyInfo.Name};");
    //                builder.AppendLine($"{prefix + prefix}}}");
    //            }
    //            if (propertyInfo.CanWrite)
    //            {
    //                builder.AppendLine($"{prefix + prefix}set");
    //                builder.AppendLine($"{prefix + prefix}{{");
    //                builder.AppendLine($"{prefix + prefix + prefix}_{propertyInfo.Name} = value;");
    //                if (generateOnPropertyChanged)
    //                    builder.AppendLine($"{prefix + prefix + prefix}OnPropertyChanged(nameof({propertyInfo.Name}));");

    //                builder.AppendLine($"{prefix + prefix}}}");
    //            }
    //            builder.AppendLine($"{prefix}}}");
    //            builder.AppendLine();
    //        }

    //        string GenerateMethodParameters(MethodInfo methodInfo)
    //        {
    //            StringBuilder builder = new StringBuilder();
    //            int index = 0;
    //            foreach (var item in methodInfo.GetParameters())
    //            {
    //                AddToGenerate(item.ParameterType);
    //                if (index > 0)
    //                    builder.Append(", ");
    //                builder.Append($"{GetFullNameOfType(item.ParameterType)} {item.Name}");
    //                index++;
    //            }
    //            return builder.ToString();
    //        }

    //        void AddUsingIfNotExist(Type type)
    //        {
    //            if (!Usings.Contains(type.Namespace))
    //                Usings.Add(type.Namespace);
    //        }

    //        string GetFullNameOfType(Type type)
    //        {
    //            if (type == typeof(bool))
    //                return "bool";
    //            else if (type == typeof(short))
    //                return "short";
    //            else if (type == typeof(ushort))
    //                return "ushort";
    //            else if (type == typeof(byte))
    //                return "byte";
    //            else if (type == typeof(sbyte))
    //                return "sbyte";
    //            else if (type == typeof(int))
    //                return "int";
    //            else if (type == typeof(uint))
    //                return "uint";
    //            else if (type == typeof(long))
    //                return "long";
    //            else if (type == typeof(ulong))
    //                return "ulong";
    //            else if (type == typeof(string))
    //                return "string";
    //            else if (type == typeof(double))
    //                return "double";
    //            else if (type == typeof(float))
    //                return "float";
    //            else if (type == typeof(decimal))
    //                return "decimal";
    //            else if (type == typeof(object))
    //                return "object";
    //            if (CanGenerateAssemblyTypes(type))
    //                AddUsingIfNotExist(type);
    //            if (type.GetIsGenericType())
    //            {

    //                string generics = "";
    //                foreach (var item in type.GetListOfGenericArguments())
    //                {
    //                    AddToGenerate(item);
    //                    if (CanGenerateAssemblyTypes(item))
    //                        AddUsingIfNotExist(item);
    //                    if (!string.IsNullOrEmpty(generics))
    //                    {
    //                        generics += ", ";
    //                    }
    //                    generics += GetFullNameOfType(item);
    //                }
    //                string name = "";
    //                if (type.Name.IndexOf("`") != -1)
    //                {
    //                    if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
    //                    {
    //                        return generics + "?";
    //                    }
    //                    else
    //                    {
    //                        name = type.Name.Substring(0, type.Name.IndexOf("`"));
    //                    }
    //                }
    //                //add type.Namespace to name spaces
    //                return $"{name}<{generics}>";
    //            }
    //            else
    //                return type.Name;
    //        }
    //    }

    /// <summary>
    /// 
    /// </summary>
    public class ServiceReferenceHelper
    {
        /// <summary>
        /// 
        /// </summary>
        public ServiceReferenceHelper()
        {
            GenerateBase();
        }

        NamespaceReferenceInfo NamespaceReferenceInfo { get; set; } = new NamespaceReferenceInfo();

        List<Assembly> ModellingReferencesAssemblies { get; set; }
        /// <summary>
        /// Type adds to this list after generated so a duplicate generate is not needed
        /// </summary>
        List<Type> ModelsCodeGenerated { get; set; } = new List<Type>();
        /// <summary>
        /// This types must be generated. After generate this will remove from list
        /// </summary>
        List<Type> TypeToGenerate { get; set; } = new List<Type>();

        string BaseAssemblyPath { get; set; }

        private void GenerateBase()
        {
            BaseAssemblyPath = Environment.GetEnvironmentVariable("SystemRoot");
            if (string.IsNullOrEmpty(BaseAssemblyPath))
            {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                var assm = typeof(string).GetTypeInfo().Assembly;
#else
                var assm = typeof(string).Assembly;
#endif
                var codeBase = assm.CodeBase;
                if (codeBase.Length < 32)
                    BaseAssemblyPath = codeBase;
                else
                    BaseAssemblyPath = codeBase.Substring(0, 32);
            }
            else
            {
                BaseAssemblyPath = "file:///" + System.IO.Path.Combine(BaseAssemblyPath, "Microsoft.Net").Replace("\\", "/").ToLower();
            }
        }

        internal bool CannotGenerateAssemblyTypes(Type type)
        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var assm = type.GetTypeInfo().Assembly;
#else
            var assm = type.Assembly;
#endif
            if (ModellingReferencesAssemblies.Contains(type.GetAssembly()) || assm.CodeBase.ToLower() == BaseAssemblyPath || assm.CodeBase.ToLower().StartsWith(BaseAssemblyPath))
                return true;
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nameSpace"></param>
        /// <param name="serverBase"></param>
        /// <returns></returns>
        public NamespaceReferenceInfo GetServiceReferenceCSharpCode(string nameSpace, ServerBase serverBase)
        {
            NamespaceReferenceInfo.Name = nameSpace;
            ModellingReferencesAssemblies = serverBase.ModellingReferencesAssemblies;
            AddUsingIfNotExist(typeof(ServiceContractAttribute));
            AddUsingIfNotExist(typeof(System.Threading.Tasks.Task));
            AddUsingIfNotExist(typeof(NotifyPropertyChangedBase));
            ModellingReferencesAssemblies.Add(typeof(NotifyPropertyChangedBase).GetAssembly());
            ModellingReferencesAssemblies.Add(typeof(ServerBase).GetAssembly());

            foreach (var serviceInfo in serverBase.RegisteredServiceTypes)
            {
                GenerateServiceClass(serviceInfo.Key, serviceInfo.Value, ClassReferenceType.ServiceLevel, ServiceType.ServerService);
            }

            foreach (var serviceInfo in serverBase.RegisteredClientServicesTypes)
            {
                GenerateServiceClass(serviceInfo.Key, serviceInfo.Value, ClassReferenceType.CallbackLevel, ServiceType.ClientService);
            }


            foreach (var serviceInfo in serverBase.StreamServices)
            {
                GenerateServiceClass(serviceInfo.Key, serviceInfo.Value.GetType(), ClassReferenceType.StreamLevel, ServiceType.StreamService);
            }

            foreach (var serviceInfo in serverBase.RegisteredHttpServiceTypes)
            {
                GenerateHttpServiceClass(serviceInfo.Key, serviceInfo.Value);
            }

            foreach (var serviceInfo in serverBase.OneWayServices)
            {
                GenerateServiceClass(serviceInfo.Key, serviceInfo.Value.GetType(), ClassReferenceType.OneWayLevel, ServiceType.OneWayService);
            }

            Type find = null;
            while ((find = TypeToGenerate.FirstOrDefault()) != null)
            {
                if (find.GetIsClass())
                    GenerateModelClass(find);
                else if (find.GetIsEnum())
                    GenerateModelEnum(find);

                TypeToGenerate.Remove(find);
            }

            return NamespaceReferenceInfo;
        }


        void AddToGenerate(Type type)
        {
            if (!TypeToGenerate.Contains(type) && !CannotGenerateAssemblyTypes(type) && !type.IsGenericParameter)
                TypeToGenerate.Add(type);
        }


        List<Type> typeGenerated = new List<Type>();
        List<MethodInfo> methodGenerated = new List<MethodInfo>();
        void GenerateServiceClass(string serviceName, Type type, ClassReferenceType classReferenceType, ServiceType serviceTypeEnum)
        {
            ClassReferenceInfo classReferenceInfo = new ClassReferenceInfo
            {
                ServiceName = serviceName,
                Name = type.Name,
                Type = classReferenceType,
                NameSpace = type.Namespace
            };

            foreach (var serviceType in type.GetTypesByAttribute<ServiceContractAttribute>(x => x.ServiceType == serviceTypeEnum).ToList())
            {
                typeGenerated.Add(serviceType);
                foreach (var methodInfo in serviceType.GetListOfMethodsWithAllOfBases().Where(x => !(x.IsSpecialName && (x.Name.StartsWith("set_") || x.Name.StartsWith("get_")))))
                {
                    GenerateMethod(methodInfo, classReferenceInfo);
                    methodGenerated.Add(methodInfo);
                }
            }
            if (classReferenceInfo.Methods.Count > 0)
                NamespaceReferenceInfo.Classes.Add(classReferenceInfo);
        }

        void GenerateHttpServiceClass(string serviceName, Type type)
        {
            ClassReferenceInfo classReferenceInfo = new ClassReferenceInfo
            {
                ServiceName = serviceName,
                Name = type.Name,
                Type = ClassReferenceType.HttpServiceLevel,
                NameSpace = type.Namespace
            };

            foreach (var serviceType in type.GetTypesByAttribute<ServiceContractAttribute>(x => x.ServiceType == ServiceType.HttpService).ToList())
            {
                typeGenerated.Add(serviceType);
                foreach (var methodInfo in serviceType.GetListOfDeclaredMethods().Where(x => !(x.IsSpecialName && (x.Name.StartsWith("set_") || x.Name.StartsWith("get_")))))
                {
                    GenerateMethod(methodInfo, classReferenceInfo);
                    methodGenerated.Add(methodInfo);
                }
            }
            if (classReferenceInfo.Methods.Count > 0)
                NamespaceReferenceInfo.Classes.Add(classReferenceInfo);
        }

        void GenerateModelEnum(Type type)
        {
            if (ModelsCodeGenerated.Contains(type) || CannotGenerateAssemblyTypes(type))
                return;
            ModelsCodeGenerated.Add(type);
            EnumReferenceInfo enumReferenceInfo = new EnumReferenceInfo
            {
                Name = type.Name,
                NameSpace = type.Namespace
            };
            var baseEumType = Enum.GetUnderlyingType(type);
            enumReferenceInfo.TypeName = GetFullNameOfType(baseEumType, true);
            foreach (var name in Enum.GetValues(type))
            {
                object res = Convert.ChangeType(name, baseEumType);
                string value = "";
                if (res != null)
                    value = res.ToString();
                enumReferenceInfo.KeyValues.Add(new KeyValue<string, string>(name.ToString(), value));
            }
            NamespaceReferenceInfo.Enums.Add(enumReferenceInfo);
        }

        void GenerateModelClass(Type type)
        {
            if (type == typeof(object) || type == null)
                return;
            if (ModelsCodeGenerated.Contains(type) || CannotGenerateAssemblyTypes(type))
                return;
            var isGeneric = type.GetIsGenericType();
            if (isGeneric && type.GetGenericTypeDefinition() != type)
            {
                GenerateModelClass(type.GetGenericTypeDefinition());
                return;
            }
            ClassReferenceInfo classReferenceInfo = new ClassReferenceInfo
            {
                Type = ClassReferenceType.ModelLevel,
                NameSpace = type.Namespace
            };

            var baseType = type.GetBaseType();
            if (baseType != typeof(object) && baseType != null)
            {
                classReferenceInfo.BaseClassName = GetFullNameOfType(baseType, true);
            }
            else
                classReferenceInfo.BaseClassName = GetFullNameOfType(typeof(NotifyPropertyChangedBase), true);

            classReferenceInfo.Name = GetFullNameOfType(type, false);

            ModelsCodeGenerated.Add(type);

            foreach (var propertyInfo in type.GetListOfDeclaredProperties())
            {
                GenerateProperty(propertyInfo, classReferenceInfo);
            }
            GenerateModelClass(type.GetBaseType());
            NamespaceReferenceInfo.Classes.Add(classReferenceInfo);
        }

        void GenerateMethod(MethodInfo methodInfo, ClassReferenceInfo classReferenceInfo)
        {
            if (methodInfo.GetParameters().Any(x => x.IsOut || x.IsRetval) || methodInfo.DeclaringType == typeof(object))
                return;
            MethodReferenceInfo methodReferenceInfo = new MethodReferenceInfo();

            AddToGenerate(methodInfo.ReturnType);
            string returnType = "void";
            if (methodInfo.ReturnType != typeof(void))
                returnType = GetFullNameOfType(methodInfo.ReturnType, true);
            methodReferenceInfo.ReturnTypeName = returnType;
            methodReferenceInfo.Name = methodInfo.Name;
            GenerateMethodParameters(methodInfo, methodReferenceInfo);
            classReferenceInfo.Methods.Add(methodReferenceInfo);
        }

        void GenerateProperty(PropertyInfo propertyInfo, ClassReferenceInfo classReferenceInfo)
        {
            if (!propertyInfo.CanWrite || !propertyInfo.CanRead)
                return;
            AddToGenerate(propertyInfo.PropertyType);
            PropertyReferenceInfo propertyReferenceInfo = new PropertyReferenceInfo
            {
                Name = propertyInfo.Name,
                ReturnTypeName = GetFullNameOfType(propertyInfo.PropertyType, true)
            };

            classReferenceInfo.Properties.Add(propertyReferenceInfo);
        }

        void GenerateMethodParameters(MethodInfo methodInfo, MethodReferenceInfo methodReferenceInfo)
        {
            foreach (var item in methodInfo.GetParameters())
            {
                ParameterReferenceInfo parameterReferenceInfo = new ParameterReferenceInfo();
                AddToGenerate(item.ParameterType);
                parameterReferenceInfo.Name = item.Name;
                parameterReferenceInfo.TypeName = GetFullNameOfType(item.ParameterType, true);
                methodReferenceInfo.Parameters.Add(parameterReferenceInfo);
            }
        }

        void AddUsingIfNotExist(Type type)
        {
            if (!NamespaceReferenceInfo.Usings.Contains(type.Namespace))
                NamespaceReferenceInfo.Usings.Add(type.Namespace);
        }

        string GetFullNameOfType(Type type, bool withNameSpace)
        {
            if (type == typeof(bool))
                return "bool";
            else if (type == typeof(short))
                return "short";
            else if (type == typeof(ushort))
                return "ushort";
            else if (type == typeof(byte))
                return "byte";
            else if (type == typeof(sbyte))
                return "sbyte";
            else if (type == typeof(int))
                return "int";
            else if (type == typeof(uint))
                return "uint";
            else if (type == typeof(long))
                return "long";
            else if (type == typeof(ulong))
                return "ulong";
            else if (type == typeof(string))
                return "string";
            else if (type == typeof(double))
                return "double";
            else if (type == typeof(float))
                return "float";
            else if (type == typeof(decimal))
                return "decimal";
            else if (type == typeof(object))
                return "object";
            if (CannotGenerateAssemblyTypes(type))
                AddUsingIfNotExist(type);
            if (type.GetIsGenericType())
            {

                string generics = "";
                foreach (var item in type.GetListOfGenericArguments())
                {
                    AddToGenerate(item);
                    if (CannotGenerateAssemblyTypes(item))
                        AddUsingIfNotExist(item);
                    if (!string.IsNullOrEmpty(generics))
                    {
                        generics += ", ";
                    }
                    generics += GetFullNameOfType(item, true);
                }
                string name = "";
                if (type.Name.IndexOf("`") != -1)
                {
                    if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        return generics + "?";
                    }
                    else
                    {
                        name = type.Name.Substring(0, type.Name.IndexOf("`"));
                        if (withNameSpace)
                            name = type.Namespace + "." + name;
                    }
                }
                //add type.Namespace to name spaces
                return $"{name}<{generics}>";
            }
            else if (type.IsGenericParameter)
                return type.Name;
            else
            {
                if (withNameSpace)
                    return type.Namespace + "." + type.Name;
                else
                    return type.Name;
            }
        }
    }
}
