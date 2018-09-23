using SignalGo.Shared.DataTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
        public static TService Wrap<TService>(Func<string, MethodInfo, Shared.Models.ParameterInfo[], Task<object>> CallMethodAction)
            where TService : class
        {
            return (TService)Wrap(typeof(TService), CallMethodAction);
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

        internal static Object Wrap(Type serviceInterfaceType, Func<string, MethodInfo, Shared.Models.ParameterInfo[], Task<object>> CallMethodAction)
        {
            //this method load GetCurrentMethod for xamarin linked assembly
            //var fix = System.Reflection.MethodInfo.GetCurrentMethod();

            AssemblyName assemblyName = new AssemblyName(String.Format("tmp_{0}", serviceInterfaceType.FullName));
            String moduleName = String.Format("{0}.dll", assemblyName.Name);
            String ns = serviceInterfaceType.Namespace;
            if (!String.IsNullOrEmpty(ns))
                ns += ".";
            ServiceContractAttribute attrib = serviceInterfaceType.GetCustomAttributes<ServiceContractAttribute>(true).Where(x => x.ServiceType == ServiceType.ServerService || x.ServiceType == ServiceType.ClientService || x.ServiceType == ServiceType.StreamService).FirstOrDefault();

#if (NET35)
            var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName,
                        AssemblyBuilderAccess.Run);
#elif (NET40)
            var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName,
                        AssemblyBuilderAccess.RunAndCollect);
#else
            AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName,
                        AssemblyBuilderAccess.RunAndCollect);
#endif
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
            var module = assembly.DefineDynamicModule(moduleName);
#else
            ModuleBuilder module = assembly.DefineDynamicModule(moduleName, false);
#endif
            TypeBuilder type = module.DefineType(String.Format("{0}InterfaceWrapper_{1}", ns, serviceInterfaceType.Name),
                TypeAttributes.Class |
                TypeAttributes.AnsiClass |
                TypeAttributes.Sealed |
                TypeAttributes.NotPublic);
            type.AddInterfaceImplementation(serviceInterfaceType);

            // Define _Service0..N-1 private service fields
            FieldBuilder[] fields = new FieldBuilder[1];
#if (NETSTANDARD || NETCOREAPP)
            var cab = new CustomAttributeBuilder(
                typeof(DebuggerBrowsableAttribute).GetTypeInfo().GetConstructor(new Type[] { typeof(DebuggerBrowsableState) }),
                new Object[] { DebuggerBrowsableState.Never });
#elif(PORTABLE)
            var cab = new CustomAttributeBuilder(
                typeof(DebuggerBrowsableAttribute).GetTypeInfo().DeclaredConstructors.FirstOrDefault(),
                new Object[] { DebuggerBrowsableState.Never });
#else
            CustomAttributeBuilder cab = new CustomAttributeBuilder(
                typeof(DebuggerBrowsableAttribute).GetConstructor(new Type[] { typeof(DebuggerBrowsableState) }),
                new Object[] { DebuggerBrowsableState.Never });
#endif

            fields[0] = type.DefineField(String.Format("_Service{0}", 0),
                serviceInterfaceType, FieldAttributes.Public);

            // Ensure the field don't show up in the debugger tooltips
            fields[0].SetCustomAttribute(cab);

            // Define a simple constructor that takes all our services as arguments
            ConstructorBuilder ctor = type.DefineConstructor(MethodAttributes.Public,
                CallingConventions.HasThis,
                new Type[] { CallMethodAction.GetType() });//Sequences.Repeat(serviceInterfaceType, services.Length)
            ILGenerator generator = ctor.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Stfld, fields[0]);
            generator.Emit(OpCodes.Ret);
            foreach (Type serviceType in GetFullTypes(serviceInterfaceType))
            {
                // Implement all the methods of the interface
                foreach (MethodInfo method in serviceType.GetListOfMethods())
                {
                    //generator.Emit(OpCodes.Pop);
                    ParameterInfo[] args = method.GetParameters();
                    MethodBuilder methodImpl = type.DefineMethod(method.Name,
                        MethodAttributes.Public | MethodAttributes.Virtual,
                        method.ReturnType, (from arg in args select arg.ParameterType).ToArray());
                    for (int i = 0; i < args.Length; i++)
                    {
                        ParameterBuilder parameterBuilder = methodImpl.DefineParameter(i + 1, ParameterAttributes.None, args[i].Name);

                    }
                    // Generate code to simply call down into each service object
                    // Any return values are discarded, except the last one, which is returned
                    generator = methodImpl.GetILGenerator();

                    MethodInfo invoke = CallMethodAction.GetType().FindMethod("Invoke");
                    generator.Emit(OpCodes.Ldarg_0);//stack [this]
                    generator.Emit(OpCodes.Ldfld, fields[0]);//stack
                    if (attrib == null)
                        throw new Exception("attrib not found");
                    //add name of service
                    generator.Emit(OpCodes.Ldstr, attrib.Name);
                    MethodInfo getCurgntMethod = typeof(MethodBase).FindMethod("GetCurrentMethod");
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

                    generator.Emit(OpCodes.Castclass, method.ReturnType);
                    generator.Emit(OpCodes.Unbox_Any, method.ReturnType);

                    if (method.ReturnType == typeof(void))
                        generator.Emit(OpCodes.Pop);

                    //generator.Emit(OpCodes.Castclass, method.ReturnType);
                    generator.Emit(OpCodes.Ret);
                }
            }
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
            var newType = type.CreateTypeInfo();
            return Activator.CreateInstance(newType.AsType(), CallMethodAction);
#else
            Type newType = type.CreateType();
            return Activator.CreateInstance(newType, CallMethodAction);
#endif

        }
    }
}
