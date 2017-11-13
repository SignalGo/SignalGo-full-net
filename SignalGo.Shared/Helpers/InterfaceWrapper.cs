using SignalGo.Shared.DataTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
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
        public static TService Wrap<TService>(Func<string, MethodInfo, object[], object> CallMethodAction)
            where TService : class
        {
            return (TService)Wrap(typeof(TService), CallMethodAction);
        }

        static List<Type> GetFullTypes(Type type)
        {
            List<Type> result = new List<Type>();
            var baseTypes = type.GetListOfBaseTypes();
            var interfaces = type.GetListOfInterfaces();
            result.AddRange(baseTypes);
            foreach (var item in interfaces)
            {
                if (!result.Contains(item))
                    result.Add(item);
            }
            return result;
        }

        public static Object Wrap(Type serviceInterfaceType, Func<string, MethodInfo, object[], object> CallMethodAction)
        {
            ///this method load GetCurrentMethod for xamarin linked assembly
            //var fix = System.Reflection.MethodInfo.GetCurrentMethod

            AssemblyName assemblyName = new AssemblyName(String.Format("tmp_{0}", serviceInterfaceType.FullName));
            String moduleName = String.Format("{0}.dll", assemblyName.Name);
            String ns = serviceInterfaceType.Namespace;
            if (!String.IsNullOrEmpty(ns))
                ns += ".";
            var attrib = serviceInterfaceType.GetCustomAttributes<ServiceContractAttribute>().FirstOrDefault();

#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
            var assembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName,
             AssemblyBuilderAccess.Run);
#else
            var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
#endif
#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
            var module = assembly.DefineDynamicModule(moduleName);
#else
            var module = assembly.DefineDynamicModule(moduleName, false);
#endif
            var type = module.DefineType(String.Format("{0}InterfaceWrapper_{1}", ns, serviceInterfaceType.Name),
                TypeAttributes.Class |
                TypeAttributes.AnsiClass |
                TypeAttributes.Sealed |
                TypeAttributes.NotPublic);
            type.AddInterfaceImplementation(serviceInterfaceType);

            // Define _Service0..N-1 private service fields
            FieldBuilder[] fields = new FieldBuilder[1];
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var cab = new CustomAttributeBuilder(
                typeof(DebuggerBrowsableAttribute).GetTypeInfo().GetConstructor(new Type[] { typeof(DebuggerBrowsableState) }),
                new Object[] { DebuggerBrowsableState.Never });
#elif(PORTABLE)
            var cab = new CustomAttributeBuilder(
                typeof(DebuggerBrowsableAttribute).GetTypeInfo().DeclaredConstructors.FirstOrDefault(),
                new Object[] { DebuggerBrowsableState.Never });
#else
            var cab = new CustomAttributeBuilder(
                typeof(DebuggerBrowsableAttribute).GetConstructor(new Type[] { typeof(DebuggerBrowsableState) }),
                new Object[] { DebuggerBrowsableState.Never });
#endif

            fields[0] = type.DefineField(String.Format("_Service{0}", 0),
                serviceInterfaceType, FieldAttributes.Public);

            // Ensure the field don't show up in the debugger tooltips
            fields[0].SetCustomAttribute(cab);

            // Define a simple constructor that takes all our services as arguments
            var ctor = type.DefineConstructor(MethodAttributes.Public,
                CallingConventions.HasThis,
                new Type[] { CallMethodAction.GetType() });//Sequences.Repeat(serviceInterfaceType, services.Length)
            var generator = ctor.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Stfld, fields[0]);
            generator.Emit(OpCodes.Ret);
            foreach (var serviceType in GetFullTypes(serviceInterfaceType))
            {
                // Implement all the methods of the interface
                foreach (var method in serviceType.GetListOfMethods())
                {
                    //generator.Emit(OpCodes.Pop);
                    var args = method.GetParameters();
                    var methodImpl = type.DefineMethod(method.Name,
                        MethodAttributes.Public | MethodAttributes.Virtual,
                        method.ReturnType, (from arg in args select arg.ParameterType).ToArray());
                    type.DefineMethodOverride(methodImpl, method);

                    // Generate code to simply call down into each service object
                    // Any return values are discarded, except the last one, which is returned
                    generator = methodImpl.GetILGenerator();

                    var invoke = CallMethodAction.GetType().FindMethod("Invoke");
                    generator.Emit(OpCodes.Ldarg_0);//stack [this]
                    generator.Emit(OpCodes.Ldfld, fields[0]);//stack
                    if (attrib == null)
                        throw new Exception("attrib not found");
                    generator.Emit(OpCodes.Ldstr, attrib.Name);
                    var getCurgntMethod = typeof(MethodBase).FindMethod("GetCurrentMethod");
                    if (getCurgntMethod == null)
                        throw new Exception("GetCurrentMethod not found");
                    generator.Emit(OpCodes.Call, getCurgntMethod);


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
                    generator.Emit(OpCodes.Ret);
                }
            }
#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
            var newType = type.CreateTypeInfo();
            return Activator.CreateInstance(newType.AsType(), CallMethodAction);
#else
            var newType = type.CreateType();
            return Activator.CreateInstance(newType, CallMethodAction);
#endif

        }
    }
}
