using SignalGo.Shared.DataTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace SignalGo.Shared.Helpers
{
    public static class InterfaceWrapper
    {
        private static void EmitInt32(ILGenerator il, int value)
        {
            switch (value)
            {
                case -1: il.Emit(OpCodes.Ldc_I4_M1); break;
                case 0: il.Emit(OpCodes.Ldc_I4_0); break;
                case 1: il.Emit(OpCodes.Ldc_I4_1); break;
                case 2: il.Emit(OpCodes.Ldc_I4_2); break;
                case 3: il.Emit(OpCodes.Ldc_I4_3); break;
                case 4: il.Emit(OpCodes.Ldc_I4_4); break;
                case 5: il.Emit(OpCodes.Ldc_I4_5); break;
                case 6: il.Emit(OpCodes.Ldc_I4_6); break;
                case 7: il.Emit(OpCodes.Ldc_I4_7); break;
                case 8: il.Emit(OpCodes.Ldc_I4_8); break;
                default:
                    if (value >= -128 && value <= 127)
                    {
                        il.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldc_I4, value);
                    }
                    break;
            }
        }
        public static TService Wrap<TService>(Func<string, System.Reflection.MethodInfo, object[], object> CallMethodAction, Func<string, System.Reflection.MethodInfo, object[], Task<object>> CallMethodAsyncAction)
            where TService : class
        {
            return (TService)Wrap(typeof(TService), CallMethodAction, CallMethodAsyncAction);
        }

        private static List<Type> GetFullTypes(Type type)
        {
            List<Type> result = new List<Type>();
            IEnumerable<Type> baseTypes = type.GetListOfBaseTypes();
            IEnumerable<Type> interfaces = type.GetListOfInterfaces();
            result.AddRange(baseTypes);
            foreach (Type item in interfaces)
            {
                if (!result.Contains(item))
                    result.Add(item);
            }
            return result;
        }

        internal static object Wrap(Type serviceInterfaceType, Func<string, System.Reflection.MethodInfo, object[], object> CallMethodAction, Func<string, System.Reflection.MethodInfo, object[], Task<object>> CallMethodAsyncAction)
        {
            //this method load GetCurrentMethod for xamarin linked assembly
            //System.Reflection.MethodBase fix = System.Reflection.MethodInfo.GetCurrentMethod();

            System.Reflection.AssemblyName assemblyName = new System.Reflection.AssemblyName(string.Format("tmp_{0}", serviceInterfaceType.FullName.Replace("`","").Replace("[","").Replace("]","").Replace(",","").Replace("=", "")));
            string moduleName = string.Format("{0}.dll", assemblyName.Name);
            string ns = serviceInterfaceType.Namespace;
            if (!string.IsNullOrEmpty(ns))
                ns += ".";
            ServiceContractAttribute attrib = serviceInterfaceType.GetCustomAttributes<ServiceContractAttribute>(true).Where(x => x.ServiceType == ServiceType.ServerService || x.ServiceType == ServiceType.ClientService || x.ServiceType == ServiceType.StreamService).FirstOrDefault();

#if (NET35)
            AssemblyBuilder assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName,
                        AssemblyBuilderAccess.Run);
#elif (NET40)
            var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName,
                        AssemblyBuilderAccess.RunAndCollect);
#else
            AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName,
                        AssemblyBuilderAccess.RunAndCollect);
#endif
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
            ModuleBuilder module = assembly.DefineDynamicModule(moduleName);
#else
            ModuleBuilder module = assembly.DefineDynamicModule(moduleName, false);
#endif
            TypeBuilder type = module.DefineType(string.Format("{0}InterfaceWrapper_{1}", ns, serviceInterfaceType.Name),
                System.Reflection.TypeAttributes.Class |
                System.Reflection.TypeAttributes.AnsiClass |
                System.Reflection.TypeAttributes.Sealed |
                System.Reflection.TypeAttributes.NotPublic);
            type.AddInterfaceImplementation(serviceInterfaceType);

            // Define _Service0..N-1 private service fields
            FieldBuilder[] fields = new FieldBuilder[2];
#if (NETSTANDARD || NETCOREAPP)
            CustomAttributeBuilder cab = new CustomAttributeBuilder(
               System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(DebuggerBrowsableAttribute)).GetConstructor(new Type[] { typeof(DebuggerBrowsableState) }),
                new object[] { DebuggerBrowsableState.Never });
#else
            CustomAttributeBuilder cab = new CustomAttributeBuilder(
                typeof(DebuggerBrowsableAttribute).GetConstructor(new Type[] { typeof(DebuggerBrowsableState) }),
                new object[] { DebuggerBrowsableState.Never });
#endif

            fields[0] = type.DefineField(string.Format("_Service{0}", 0),
                serviceInterfaceType, System.Reflection.FieldAttributes.Public);

            fields[1] = type.DefineField(string.Format("_Service{0}", 1),
                serviceInterfaceType, System.Reflection.FieldAttributes.Public);

            // Ensure the field don't show up in the debugger tooltips
            fields[0].SetCustomAttribute(cab);
            fields[1].SetCustomAttribute(cab);

            // Define a simple constructor that takes all our services as arguments
            ConstructorBuilder ctor = type.DefineConstructor(System.Reflection.MethodAttributes.Public,
                System.Reflection.CallingConventions.HasThis,
                new Type[] { CallMethodAction.GetType(), CallMethodAsyncAction.GetType() });
            ILGenerator generator = ctor.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Stfld, fields[0]);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_2);
            generator.Emit(OpCodes.Stfld, fields[1]);
            generator.Emit(OpCodes.Ret);
            foreach (Type serviceType in GetFullTypes(serviceInterfaceType))
            {
                // Implement all the methods of the interface
                foreach (System.Reflection.MethodInfo method in serviceType.GetListOfMethods())
                {
                    //generator.Emit(OpCodes.Pop);
                    System.Reflection.ParameterInfo[] args = method.GetParameters();
                    MethodBuilder methodImpl = type.DefineMethod(method.Name,
                        System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.Virtual,
                        method.ReturnType, (from arg in args select arg.ParameterType).ToArray());
                    for (int i = 0; i < args.Length; i++)
                    {
                        ParameterBuilder parameterBuilder = methodImpl.DefineParameter(i + 1, System.Reflection.ParameterAttributes.None, args[i].Name);
                    }
                    // Generate code to simply call down into each service object
                    // Any return values are discarded, except the last one, which is returned
                    generator = methodImpl.GetILGenerator();

                    System.Reflection.MethodInfo invoke = null;
                    if (method.ReturnType.GetBaseType() == typeof(Task) || method.ReturnType == typeof(Task))
                    {
                        invoke = CallMethodAsyncAction.GetType().FindMethod("Invoke");
                        generator.Emit(OpCodes.Ldarg_0);//stack [this]
                        generator.Emit(OpCodes.Ldfld, fields[1]);//stack
                    }
                    else
                    {
                        invoke = CallMethodAction.GetType().FindMethod("Invoke");
                        generator.Emit(OpCodes.Ldarg_0);//stack [this]
                        generator.Emit(OpCodes.Ldfld, fields[0]);//stack
                    }


                    if (attrib == null)
                        throw new Exception("attrib not found");
                    string serviceName = attrib.Name;
                    if (attrib.ServiceType == ServiceType.ClientService)
                        serviceName = attrib.GetServiceName(false);
                    serviceName = ServiceContractExtensions.GetServiceNameWithGeneric(serviceType, serviceName);

                    //add name of service
                    generator.Emit(OpCodes.Ldstr, serviceName);
                    System.Reflection.MethodInfo getCurgntMethod = typeof(System.Reflection.MethodBase).FindMethod("GetCurrentMethod");
                    if (getCurgntMethod == null)
                        throw new Exception("GetCurrentMethod not found");
                    //add current method info
                    generator.Emit(OpCodes.Call, getCurgntMethod);

                    //add obj[] argumants
                    if (args.Length > 0)
                    {
                        EmitInt32(generator, args.Length);
                        generator.Emit(OpCodes.Newarr, typeof(object));
                        for (int index = 0; index < args.Length; index++)
                        {
                            generator.Emit(OpCodes.Dup);
                            EmitInt32(generator, index);
                            switch (index)
                            {
                                case 0: generator.Emit(OpCodes.Ldarg_1); break;
                                case 1: generator.Emit(OpCodes.Ldarg_2); break;
                                case 2: generator.Emit(OpCodes.Ldarg_3); break;
                                default:
                                    generator.Emit(index < 255 ? OpCodes.Ldarg_S
                                        : OpCodes.Ldarg, index + 1);
                                    break;
                            }
                            //generator.Emit(OpCodes.Ldstr, args[index].Name);
                            generator.Emit(OpCodes.Box, args[index].ParameterType);
                            generator.Emit(OpCodes.Stelem_Ref);
                        }
                    }
                    else
                    {
                        EmitInt32(generator, 0);
                        generator.Emit(OpCodes.Newarr, typeof(object));
                    }
                    if (invoke == null)
                        throw new Exception("invoke not found");
                    generator.EmitCall(OpCodes.Call, invoke, null);

                    if (method.ReturnType == typeof(void))
                        generator.Emit(OpCodes.Pop);
                    else
                    {
                        generator.Emit(OpCodes.Castclass, method.ReturnType);
                        generator.Emit(OpCodes.Unbox_Any, method.ReturnType);
                    }
                    //generator.Emit(OpCodes.Castclass, method.ReturnType);
                    generator.Emit(OpCodes.Ret);
                }
            }
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
            System.Reflection.TypeInfo newType = type.CreateTypeInfo();
            return Activator.CreateInstance(newType.AsType(), CallMethodAction, CallMethodAsyncAction);
#else
            Type newType = type.CreateType();
            return Activator.CreateInstance(newType, CallMethodAction, CallMethodAsyncAction);
#endif
        }
    }
}
