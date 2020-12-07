using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SignalGo.Server.ServiceManager;
using SignalGo.Shared.Converters;
using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.Models;
using SignalGo.Shared.Models.ServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Server.Helpers
{
    public class ServerServicesManager
    {
        static Dictionary<Type, string> LoadedExamples { get; set; } = new Dictionary<Type, string>();

        static void LoadExamples(ServerBase serverBase)
        {
            if (LoadedExamples.Count != 0)
                return;
            foreach (var assembly in serverBase.TestExampleAssemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    var attribute = type.GetCustomAttributes<TestExampleAttribute>().FirstOrDefault();
                    if (attribute != null)
                    {
                        foreach (var typeMethod in type.GetListOfMethods())
                        {
                            if (!LoadedExamples.ContainsKey(typeMethod.ReturnType))
                                LoadedExamples.Add(typeMethod.ReturnType, JsonConvert.SerializeObject(typeMethod.Invoke(null, null), Formatting.Indented));
                        }
                    }
                }
            }
        }
        /// <summary>
        /// send detail of service
        /// </summary>
        /// <param name="client"></param>
        /// <param name="hostUrl">host url that client connected</param>
        private List<Type> skippedTypes = new List<Type>();
        internal ProviderDetailsInfo SendServiceDetail(string hostUrl, ServerBase serverBase)
        {
            LoadExamples(serverBase);
            //try
            //{
            if (!hostUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                hostUrl += "http://";
            if (Uri.TryCreate(hostUrl, UriKind.Absolute, out Uri uri))
                hostUrl = uri.Host + ":" + uri.Port;
            using (XmlCommentLoader xmlCommentLoader = new XmlCommentLoader())
            {
                List<Type> modelTypes = new List<Type>();
                int id = 1;
                ProviderDetailsInfo result = new ProviderDetailsInfo() { Id = id };
                foreach (KeyValuePair<string, Type> service in serverBase.RegisteredServiceTypes.Where(x => x.Value.IsServerService()))
                {

                    id++;
                    ServiceDetailsInfo serviceDetail = new ServiceDetailsInfo()
                    {
                        ServiceName = service.Key,
                        FullNameSpace = service.Value.FullName,
                        NameSpace = service.Value.Name,
                        Id = id
                    };
                    result.Services.Add(serviceDetail);
                    List<Type> types = new List<Type>();
                    types.Add(service.Value);
                    foreach (Type item in CSCodeInjection.GetListOfTypes(service.Value))
                    {
                        if (item.GetCustomAttributes<ServiceContractAttribute>(false).Length > 0 && !types.Contains(item))
                        {
                            types.Add(item);
                            types.AddRange(CSCodeInjection.GetListOfTypes(service.Value).Where(x => !types.Contains(x)));
                        }
                    }

                    foreach (Type serviceType in types)
                    {
                        if (serviceType == typeof(object))
                            continue;
                        //(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
                        //because of base classes of services
                        List<MethodInfo> methods = serviceType.GetListOfMethodsWithAllOfBases().Where(x => !(x.IsSpecialName && (x.Name.StartsWith("set_") || x.Name.StartsWith("get_"))) && x.DeclaringType != typeof(object)).Where(x => !x.IsOverride()).Where(x => x.IsPublic).ToList();
                        if (methods.Count == 0)
                            continue;
                        CommentOfClassInfo comment = xmlCommentLoader.GetComment(serviceType);
                        id++;
                        ServiceDetailsInterface interfaceInfo = new ServiceDetailsInterface()
                        {
                            NameSpace = serviceType.Name,
                            FullNameSpace = serviceType.FullName,
                            Comment = comment?.Summery,
                            Id = id
                        };
                        serviceDetail.Services.Add(interfaceInfo);
                        List<ServiceDetailsMethod> serviceMethods = new List<ServiceDetailsMethod>();
                        foreach (MethodInfo method in methods)
                        {
                            SerializeObjectType pType = SerializeHelper.GetTypeCodeOfObject(method.ReturnType);
                            if (pType == SerializeObjectType.Enum)
                            {
                                AddEnumAndNewModels(ref id, method.ReturnType, result, SerializeObjectType.Enum, xmlCommentLoader);
                            }
                            CommentOfMethodInfo methodComment = comment == null ? null : (from x in comment.Methods where x.Name == method.Name && x.Parameters.Count == method.GetParameters().Length select x).FirstOrDefault();
                            string exceptions = "";
                            if (methodComment?.Exceptions != null && methodComment?.Exceptions.Count > 0)
                            {
                                foreach (CommentOfExceptionInfo ex in methodComment.Exceptions)
                                {
                                    try
                                    {
                                        if (ex.RefrenceType.LastIndexOf('.') != -1)
                                        {
                                            string baseNameOfEnum = ex.RefrenceType.Substring(0, ex.RefrenceType.LastIndexOf('.'));
                                            Type type = GetEnumType(baseNameOfEnum);
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                                            if (type != null && type.GetTypeInfo().IsEnum)
#else
                                            if (type != null && type.IsEnum)
#endif
                                            {
                                                object value = Enum.Parse(type, ex.RefrenceType.Substring(ex.RefrenceType.LastIndexOf('.') + 1, ex.RefrenceType.Length - ex.RefrenceType.LastIndexOf('.') - 1));
                                                int exNumber = (int)value;
                                                exceptions += ex.RefrenceType + $" ({exNumber}) : " + ex.Comment + TextHelper.NewLine;
                                                continue;
                                            }
                                        }
                                    }
                                    catch
                                    {

                                    }

                                    exceptions += ex.RefrenceType + ":" + ex.Comment + TextHelper.NewLine;
                                }
                            }
                            id++;
                            ServiceDetailsMethod info = new ServiceDetailsMethod()
                            {
                                MethodName = method.Name,
#if (!NET35)
                                Requests = new System.Collections.ObjectModel.ObservableCollection<ServiceDetailsRequestInfo>() { new ServiceDetailsRequestInfo() { Name = "Default", Parameters = new List<ServiceDetailsParameterInfo>(), IsSelected = true } },
#endif
                                ReturnType = method.ReturnType.GetFriendlyName(),
                                Comment = methodComment?.Summery,
                                ReturnComment = methodComment?.Returns,
                                ExceptionsComment = exceptions,
                                Id = id
                            };
                            GenerateRequestAndResponseOfMethodTestExamples(info, method);
                            RuntimeTypeHelper.GetListOfUsedTypes(method.ReturnType, ref modelTypes);
                            foreach (System.Reflection.ParameterInfo paramInfo in method.GetParameters())
                            {
                                pType = SerializeHelper.GetTypeCodeOfObject(paramInfo.ParameterType);
                                if (pType == SerializeObjectType.Enum)
                                {
                                    AddEnumAndNewModels(ref id, paramInfo.ParameterType, result, SerializeObjectType.Enum, xmlCommentLoader);
                                }
                                string parameterComment = "";
                                if (methodComment != null)
                                    parameterComment = (from x in methodComment.Parameters where x.Name == paramInfo.Name select x.Comment).FirstOrDefault();
                                id++;
                                ServiceDetailsParameterInfo p = new ServiceDetailsParameterInfo()
                                {
                                    Name = paramInfo.Name,
                                    Type = paramInfo.ParameterType.GetFriendlyName(),
                                    FullTypeName = paramInfo.ParameterType.FullName,
                                    Comment = parameterComment,
                                    Id = id
                                };
                                info.Parameters.Add(new ParameterReferenceInfo()
                                {
                                    Name = paramInfo.Name,
                                    Comment = parameterComment,
                                    TypeName = paramInfo.ParameterType.GetFriendlyName(),
                                });
#if (!NET35)
                                info.Requests.First().Parameters.Add(p);
#endif
                                RuntimeTypeHelper.GetListOfUsedTypes(paramInfo.ParameterType, ref modelTypes);
                            }
                            serviceMethods.Add(info);
                        }
                        interfaceInfo.Methods.AddRange(serviceMethods);
                    }
                }





                foreach (KeyValuePair<string, Type> service in serverBase.RegisteredServiceTypes.Where(x => x.Value.IsClientService()))
                {
                    id++;
                    CallbackServiceDetailsInfo serviceDetail = new CallbackServiceDetailsInfo()
                    {
                        ServiceName = service.Key,
                        FullNameSpace = service.Value.FullName,
                        NameSpace = service.Value.Name,
                        Id = id
                    };

                    result.Callbacks.Add(serviceDetail);
                    List<Type> types = new List<Type>();
                    types.Add(service.Value);
                    foreach (Type item in CSCodeInjection.GetListOfTypes(service.Value))
                    {
                        if (item.GetCustomAttributes<ServiceContractAttribute>(false).Length > 0 && !types.Contains(item))
                        {
                            types.Add(item);
                            types.AddRange(CSCodeInjection.GetListOfTypes(service.Value).Where(x => !types.Contains(x)));
                        }
                    }
                    //(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
                    //because of base classes of services
                    List<MethodInfo> methods = service.Value.GetListOfMethodsWithAllOfBases().Where(x => !(x.IsSpecialName && (x.Name.StartsWith("set_") || x.Name.StartsWith("get_"))) && x.DeclaringType != typeof(object)).Where(x => !x.IsOverride()).Where(x => x.IsPublic).ToList();
                    if (methods.Count == 0)
                        continue;
                    CommentOfClassInfo comment = xmlCommentLoader.GetComment(service.Value);
                    List<ServiceDetailsMethod> serviceMethods = new List<ServiceDetailsMethod>();
                    foreach (MethodInfo method in methods)
                    {
                        SerializeObjectType pType = SerializeHelper.GetTypeCodeOfObject(method.ReturnType);
                        if (pType == SerializeObjectType.Enum)
                        {
                            AddEnumAndNewModels(ref id, method.ReturnType, result, SerializeObjectType.Enum, xmlCommentLoader);
                        }
                        CommentOfMethodInfo methodComment = comment == null ? null : (from x in comment.Methods where x.Name == method.Name && x.Parameters.Count == method.GetParameters().Length select x).FirstOrDefault();
                        string exceptions = "";
                        if (methodComment?.Exceptions != null && methodComment?.Exceptions.Count > 0)
                        {
                            foreach (CommentOfExceptionInfo ex in methodComment.Exceptions)
                            {
                                try
                                {
                                    if (ex.RefrenceType.LastIndexOf('.') != -1)
                                    {
                                        string baseNameOfEnum = ex.RefrenceType.Substring(0, ex.RefrenceType.LastIndexOf('.'));
                                        Type type = GetEnumType(baseNameOfEnum);
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                                        if (type != null && type.GetTypeInfo().IsEnum)
#else
                                        if (type != null && type.IsEnum)
#endif
                                        {
                                            object value = Enum.Parse(type, ex.RefrenceType.Substring(ex.RefrenceType.LastIndexOf('.') + 1, ex.RefrenceType.Length - ex.RefrenceType.LastIndexOf('.') - 1));
                                            int exNumber = (int)value;
                                            exceptions += ex.RefrenceType + $" ({exNumber}) : " + ex.Comment + TextHelper.NewLine;
                                            continue;
                                        }
                                    }
                                }
                                catch
                                {

                                }

                                exceptions += ex.RefrenceType + ":" + ex.Comment + TextHelper.NewLine;
                            }
                        }
                        id++;
                        ServiceDetailsMethod info = new ServiceDetailsMethod()
                        {
                            MethodName = method.Name,
#if (!NET35)
                            Requests = new System.Collections.ObjectModel.ObservableCollection<ServiceDetailsRequestInfo>() { new ServiceDetailsRequestInfo() { Name = "Default", Parameters = new List<ServiceDetailsParameterInfo>(), IsSelected = true } },
#endif
                            ReturnType = method.ReturnType.GetFriendlyName(),
                            Comment = methodComment?.Summery,
                            ReturnComment = methodComment?.Returns,
                            ExceptionsComment = exceptions,
                            Id = id
                        };
                        GenerateRequestAndResponseOfMethodTestExamples(info, method);
                        RuntimeTypeHelper.GetListOfUsedTypes(method.ReturnType, ref modelTypes);
                        foreach (System.Reflection.ParameterInfo paramInfo in method.GetParameters())
                        {
                            pType = SerializeHelper.GetTypeCodeOfObject(paramInfo.ParameterType);
                            if (pType == SerializeObjectType.Enum)
                            {
                                AddEnumAndNewModels(ref id, paramInfo.ParameterType, result, SerializeObjectType.Enum, xmlCommentLoader);
                            }
                            string parameterComment = "";
                            if (methodComment != null)
                                parameterComment = (from x in methodComment.Parameters where x.Name == paramInfo.Name select x.Comment).FirstOrDefault();
                            id++;
                            ServiceDetailsParameterInfo p = new ServiceDetailsParameterInfo()
                            {
                                Name = paramInfo.Name,
                                Type = paramInfo.ParameterType.GetFriendlyName(),
                                FullTypeName = paramInfo.ParameterType.FullName,
                                Comment = parameterComment,
                                Id = id
                            };
#if (!NET35)
                            info.Requests.First().Parameters.Add(p);
#endif
                            RuntimeTypeHelper.GetListOfUsedTypes(paramInfo.ParameterType, ref modelTypes);
                        }
                        serviceMethods.Add(info);
                    }
                    serviceDetail.Methods.AddRange(serviceMethods);
                }



                foreach (KeyValuePair<string, Type> httpServiceType in serverBase.RegisteredServiceTypes.Where(x => x.Value.IsHttpService()))
                {
                    id++;
                    HttpControllerDetailsInfo controller = new HttpControllerDetailsInfo()
                    {
                        Id = id,
                        Url = httpServiceType.Value.GetCustomAttributes<ServiceContractAttribute>(true).Length > 0 ? httpServiceType.Value.GetCustomAttributes<ServiceContractAttribute>(true)[0].Name : httpServiceType.Key,
                    };
                    id++;
                    result.WebApiDetailsInfo.Id = id;
                    result.WebApiDetailsInfo.HttpControllers.Add(controller);
                    //(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
                    //because of base classes of services
                    List<MethodInfo> methods = httpServiceType.Value.GetListOfMethodsWithAllOfBases().Where(x => !(x.IsSpecialName && (x.Name.StartsWith("set_") || x.Name.StartsWith("get_"))) && x.DeclaringType != typeof(object)).Where(x => !x.IsOverride()).Where(x => x.IsPublic).ToList();
                    if (methods.Count == 0)
                        continue;
                    CommentOfClassInfo comment = xmlCommentLoader.GetComment(httpServiceType.Value);
                    List<ServiceDetailsMethod> serviceMethods = new List<ServiceDetailsMethod>();
                    foreach (MethodInfo method in methods)
                    {
                        SerializeObjectType pType = SerializeHelper.GetTypeCodeOfObject(method.ReturnType);
                        if (pType == SerializeObjectType.Enum)
                        {
                            AddEnumAndNewModels(ref id, method.ReturnType, result, SerializeObjectType.Enum, xmlCommentLoader);
                        }
                        CommentOfMethodInfo methodComment = comment == null ? null : (from x in comment.Methods where x.Name == method.Name && x.Parameters.Count == method.GetParameters().Length select x).FirstOrDefault();
                        string exceptions = "";
                        if (methodComment?.Exceptions != null && methodComment?.Exceptions.Count > 0)
                        {
                            foreach (CommentOfExceptionInfo ex in methodComment.Exceptions)
                            {
                                try
                                {
                                    if (ex.RefrenceType.LastIndexOf('.') != -1)
                                    {
                                        string baseNameOfEnum = ex.RefrenceType.Substring(0, ex.RefrenceType.LastIndexOf('.'));
                                        Type type = GetEnumType(baseNameOfEnum);
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                                        if (type != null && type.GetTypeInfo().IsEnum)
#else
                                        if (type != null && type.IsEnum)
#endif
                                        {
                                            object value = Enum.Parse(type, ex.RefrenceType.Substring(ex.RefrenceType.LastIndexOf('.') + 1, ex.RefrenceType.Length - ex.RefrenceType.LastIndexOf('.') - 1));
                                            int exNumber = (int)value;
                                            exceptions += ex.RefrenceType + $" ({exNumber}) : " + ex.Comment + TextHelper.NewLine;
                                            continue;
                                        }
                                    }
                                }
                                catch
                                {

                                }

                                exceptions += ex.RefrenceType + ":" + ex.Comment + TextHelper.NewLine;
                            }
                        }
                        id++;
                        ServiceDetailsMethod info = new ServiceDetailsMethod()
                        {
                            Id = id,
                            MethodName = method.Name,
#if (!NET35)
                            Requests = new System.Collections.ObjectModel.ObservableCollection<ServiceDetailsRequestInfo>() { new ServiceDetailsRequestInfo() { Name = "Default", Parameters = new List<ServiceDetailsParameterInfo>(), IsSelected = true } },
#endif
                            ReturnType = method.ReturnType.GetFriendlyName(),
                            Comment = methodComment?.Summery,
                            ReturnComment = methodComment?.Returns,
                            ExceptionsComment = exceptions,
                            TestExample = hostUrl + "/" + controller.Url + "/" + method.Name
                        };
                        GenerateRequestAndResponseOfMethodTestExamples(info, method);
                        RuntimeTypeHelper.GetListOfUsedTypes(method.ReturnType, ref modelTypes);
                        string testExampleParams = "";
                        foreach (System.Reflection.ParameterInfo paramInfo in method.GetParameters())
                        {
                            pType = SerializeHelper.GetTypeCodeOfObject(paramInfo.ParameterType);
                            if (pType == SerializeObjectType.Enum)
                            {
                                AddEnumAndNewModels(ref id, paramInfo.ParameterType, result, SerializeObjectType.Enum, xmlCommentLoader);
                            }
                            string parameterComment = "";
                            if (methodComment != null)
                                parameterComment = (from x in methodComment.Parameters where x.Name == paramInfo.Name select x.Comment).FirstOrDefault();
                            id++;
                            ServiceDetailsParameterInfo p = new ServiceDetailsParameterInfo()
                            {
                                Id = id,
                                Name = paramInfo.Name,
                                Type = paramInfo.ParameterType.Name,
                                FullTypeName = paramInfo.ParameterType.FullName,
                                Comment = parameterComment
                            };
#if (!NET35)
                            info.Requests.First().Parameters.Add(p);
#endif
                            if (string.IsNullOrEmpty(testExampleParams))
                                testExampleParams += "?";
                            else
                                testExampleParams += "&";
                            testExampleParams += paramInfo.Name + "=" + DataExchangeConverter.GetDefault(paramInfo.ParameterType) ?? "null";
                            RuntimeTypeHelper.GetListOfUsedTypes(paramInfo.ParameterType, ref modelTypes);
                        }
                        info.TestExample += testExampleParams;
                        serviceMethods.Add(info);
                    }
                    controller.Methods = serviceMethods;
                }

                foreach (Type type in modelTypes)
                {
                    try
                    {
                        SerializeObjectType pType = SerializeHelper.GetTypeCodeOfObject(type);
                        AddEnumAndNewModels(ref id, type, result, pType, xmlCommentLoader);
                        //                                var mode = SerializeHelper.GetTypeCodeOfObject(type);
                        //                                if (mode == SerializeObjectType.Object)
                        //                                {
                        //                                    if (type.Name.Contains("`") || type == typeof(CustomAttributeTypedArgument) || type == typeof(CustomAttributeNamedArgument) ||
                        //#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                        //                                        type.GetTypeInfo().BaseType == typeof(Attribute))
                        //#else
                        //                                    type.BaseType == typeof(Attribute))
                        //#endif
                        //                                        continue;

                        //                                    var instance = Activator.CreateInstance(type);
                        //                                    string jsonResult = JsonConvert.SerializeObject(instance, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Include });
                        //                                    var refactorResult = (JObject)JsonConvert.DeserializeObject(jsonResult);
                        //                                    foreach (var item in refactorResult.Properties())
                        //                                    {
                        //                                        var find = type.GetProperties().FirstOrDefault(x => x.Name == item.Name);
                        //                                        refactorResult[item.Name] = find.PropertyType.FullName;
                        //                                    }
                        //                                    jsonResult = JsonConvert.SerializeObject(refactorResult, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Include });

                        //                                    if (jsonResult == "{}" || jsonResult == "[]")
                        //                                        continue;
                        //                                    var comment = xmlCommentLoader.GetComment(type);
                        //                                    id++;
                        //                                    result.ProjectDomainDetailsInfo.Id = id;
                        //                                    id++;
                        //                                    result.ProjectDomainDetailsInfo.Models.Add(new ModelDetailsInfo()
                        //                                    {
                        //                                        Id = id,
                        //                                        Comment = comment?.Summery,
                        //                                        Name = type.Name,
                        //                                        FullNameSpace = type.FullName,
                        //                                        ObjectType = mode,
                        //                                        JsonTemplate = jsonResult
                        //                                    });
                        //                                    foreach (var property in type.GetProperties())
                        //                                    {
                        //                                        var pType = SerializeHelper.GetTypeCodeOfObject(property.PropertyType);
                        //                                        if (pType == SerializeObjectType.Enum)
                        //                                        {
                        //                                            AddEnumAndNewModels(ref id, property.PropertyType, result, SerializeObjectType.Enum, xmlCommentLoader);
                        //                                        }
                        //                                    }
                        //                                }
                    }
                    catch (Exception ex)
                    {
                        serverBase.AutoLogger.LogError(ex, "Model Type Add error: " + ex.ToString());
                    }
                }

                return result;
                //string json = ServerSerializationHelper.SerializeObject(result, serverBase);
                //List<byte> bytes = new List<byte>
                // {
                //                (byte)DataType.GetServiceDetails,
                //                (byte)CompressMode.None
                // };
                //byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
                //byte[] dataLen = BitConverter.GetBytes(jsonBytes.Length);
                //bytes.AddRange(dataLen);
                //bytes.AddRange(jsonBytes);
                //await client.StreamHelper.WriteToStreamAsync(client.ClientStream, bytes.ToArray());
            }
            //}
            //catch (Exception ex)
            //{
            //    try
            //    {
            //        string json = ServerSerializationHelper.SerializeObject(new Exception(ex.ToString()), serverBase);
            //        List<byte> bytes = new List<byte>
            //         {
            //            (byte)DataType.GetServiceDetails,
            //            (byte)CompressMode.None
            //         };
            //        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
            //        byte[] dataLen = BitConverter.GetBytes(jsonBytes.Length);
            //        bytes.AddRange(dataLen);
            //        bytes.AddRange(jsonBytes);
            //        await client.StreamHelper.WriteToStreamAsync(client.ClientStream, bytes.ToArray());

            //    }
            //    catch (Exception)
            //    {

            //    }
            //    serverBase.AutoLogger.LogError(ex, $"{client.IPAddress} {client.ClientId} ServerBase CallMethod");
            //}
            //finally
            //{
            //    skippedTypes.Clear();
            //}

            void AddEnumAndNewModels(ref int id, Type type, ProviderDetailsInfo result, SerializeObjectType objType, XmlCommentLoader xmlCommentLoader)
            {
                if (result.ProjectDomainDetailsInfo.Models.Any(x => x.FullNameSpace == type.FullName) || skippedTypes.Contains(type))
                    return;
#if (!NETSTANDARD1_6)
                if (type.Module.ScopeName == "System.Private.CoreLib.dll" || type.Module.ScopeName == "CommonLanguageRuntimeLibrary")
                    return;
#endif
                id++;
                result.ProjectDomainDetailsInfo.Id = id;
                id++;
                if (objType == SerializeObjectType.Enum)
                {
                    List<string> items = new List<string>();
                    List<ParameterReferenceInfo> properties = new List<ParameterReferenceInfo>();
                    foreach (Enum obj in Enum.GetValues(type))
                    {
                        int x = Convert.ToInt32(obj); // x is the integer value of enum
                        items.Add(obj.ToString() + " = " + x);
                        var valueComment = xmlCommentLoader.GetComment(type, obj.ToString());
                        properties.Add(new ParameterReferenceInfo()
                        {
                            Name = obj.ToString(),
                            TypeName = x.ToString(),
                            Comment = valueComment?.Summery
                        });
                    }
                    CommentOfClassInfo comment = xmlCommentLoader.GetComment(type);
                    result.ProjectDomainDetailsInfo.Models.Add(new ModelDetailsInfo()
                    {
                        Id = id,
                        Name = type.Name,
                        FullNameSpace = type.FullName,
                        ObjectType = objType,
                        JsonTemplate = JsonConvert.SerializeObject(items, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Include }),
                        IsEnum = true,
                        Properties = properties,
                        Comment = comment?.Summery,
                        JsonExample = GenerateTestExamples(type)
                    });
                }
                else
                {
                    try
                    {
                        if (type.Name.Contains("`") || type == typeof(CustomAttributeTypedArgument) || type == typeof(CustomAttributeNamedArgument) ||
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                                                type.GetTypeInfo().BaseType == typeof(Attribute) || type.GetTypeInfo().BaseType == null)
#else
                                                type.BaseType == typeof(Attribute) || type.BaseType == null)
#endif
                        {
                            skippedTypes.Add(type);
                            return;
                        }

                        List<ParameterReferenceInfo> properties = new List<ParameterReferenceInfo>();

                        object instance = Activator.CreateInstance(type);
                        string jsonResult = JsonConvert.SerializeObject(instance, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Include });
                        JObject refactorResult = (JObject)JsonConvert.DeserializeObject(jsonResult);
                        foreach (JProperty item in refactorResult.Properties())
                        {
                            PropertyInfo find = type.GetProperties().FirstOrDefault(x => x.Name == item.Name);
                            if (find != null)
                                refactorResult[item.Name] = find.PropertyType.GetFriendlyName();
                            var proeprtyComment = xmlCommentLoader.GetComment(find);

                            properties.Add(new ParameterReferenceInfo()
                            {
                                Name = find.Name,
                                TypeName = find.PropertyType.GetFriendlyName(),
                                Comment = proeprtyComment?.Summery
                            });
                        }

                        jsonResult = JsonConvert.SerializeObject(refactorResult, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Include });

                        if (jsonResult == "{}" || jsonResult == "[]")
                        {
                            skippedTypes.Add(type);
                            return;
                        }
                        CommentOfClassInfo comment = xmlCommentLoader.GetComment(type);
                        id++;
                        result.ProjectDomainDetailsInfo.Id = id;
                        id++;
                        result.ProjectDomainDetailsInfo.Models.Add(new ModelDetailsInfo()
                        {
                            Id = id,
                            Comment = comment?.Summery,
                            Name = type.Name,
                            FullNameSpace = type.FullName,
                            ObjectType = objType,
                            JsonTemplate = jsonResult,
                            Properties = properties
                        });
                    }
                    catch
                    {
                        skippedTypes.Add(type);
                    }
                }

                foreach (Type item in type.GetListOfGenericArguments())
                {
                    SerializeObjectType pType = SerializeHelper.GetTypeCodeOfObject(item);
                    AddEnumAndNewModels(ref id, item, result, pType, xmlCommentLoader);
                }

                foreach (Type item in type.GetListOfInterfaces())
                {
                    SerializeObjectType pType = SerializeHelper.GetTypeCodeOfObject(item);
                    AddEnumAndNewModels(ref id, item, result, pType, xmlCommentLoader);
                }

                foreach (Type item in type.GetListOfNestedTypes())
                {
                    SerializeObjectType pType = SerializeHelper.GetTypeCodeOfObject(item);
                    AddEnumAndNewModels(ref id, item, result, pType, xmlCommentLoader);
                }

                foreach (Type item in type.GetListOfBaseTypes())
                {
                    SerializeObjectType pType = SerializeHelper.GetTypeCodeOfObject(item);
                    AddEnumAndNewModels(ref id, item, result, pType, xmlCommentLoader);
                }
                foreach (PropertyInfo property in type.GetProperties())
                {
                    SerializeObjectType pType = SerializeHelper.GetTypeCodeOfObject(property.PropertyType);
                    AddEnumAndNewModels(ref id, property.PropertyType, result, pType, xmlCommentLoader);

                }
            }
        }

        void GenerateRequestAndResponseOfMethodTestExamples(ServiceDetailsMethod serviceDetailsMethod, MethodInfo method)
        {
            Type returnType = method.ReturnType;
            if (returnType.GetBaseType() == typeof(Task))
            {
                returnType = method.ReturnType.GetListOfGenericArguments().FirstOrDefault();
            }

            if (LoadedExamples.TryGetValue(returnType, out string responseJson))
            {
                serviceDetailsMethod.ResponseJsonExample = responseJson;
            }

            StringBuilder requestJson = new StringBuilder();
            requestJson.AppendLine("{");
            bool isFirst = true;
            foreach (var parameter in method.GetParameters())
            {
                if (!isFirst)
                    requestJson.AppendLine("    ,");
                if (LoadedExamples.TryGetValue(parameter.ParameterType, out string parameterJson))
                {
                    requestJson.AppendLine($"   \"{parameter.Name}\": {parameterJson}");
                }
                else
                {
                    requestJson.AppendLine($"   \"{parameter.Name}\":");
                    requestJson.AppendLine("    {");
                    requestJson.AppendLine("    }");
                }
                isFirst = false;
            }
            requestJson.AppendLine("}");
            serviceDetailsMethod.RequestJsonExample = requestJson.ToString();
        }

        string GenerateTestExamples(Type type)
        {
            Type returnType = type;
            if (returnType.GetBaseType() == typeof(Task))
            {
                returnType = type.GetListOfGenericArguments().FirstOrDefault();
            }

            if (LoadedExamples.TryGetValue(returnType, out string responseJson))
            {
                return responseJson;
            }
            return null;
        }

        internal string SendMethodParameterDetail(Type serviceType, MethodParameterDetails detail, ServerBase serverBase)
        {
            string json = null;
            foreach (MethodInfo method in serviceType.GetListOfMethodsWithAllOfBases().Where(x => x.IsPublic))
            {
                if (method.IsSpecialName && (method.Name.StartsWith("set_") || method.Name.StartsWith("get_")))
                    continue;
                if (method.Name == detail.MethodName && detail.ParametersCount == method.GetParameters().Length)
                {
                    Type parameterType = method.GetParameters()[detail.ParameterIndex].ParameterType;
                    if (detail.IsFull)
                        json = TypeToJsonString(parameterType);
                    else
                        json = SimpleTypeToJsonString(parameterType);
                    break;
                }
            }
            if (json == null)
                throw new Exception("method or parameter not found");

            return json;
        }

        internal static Type GetEnumType(string enumName)
        {
#if (NETSTANDARD)
            foreach (var assembly in SignalGo.Shared.Helpers.AppDomain.CurrentDomain.GetAssemblies())
#else
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
#endif
            {
                Type type = assembly.GetType(enumName);
                if (type == null)
                    continue;
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                if (type.GetTypeInfo().IsEnum)
                    return type;
#else
                if (type.IsEnum)
                    return type;
#endif
            }
            return null;
        }

        private string TypeToJsonString(Type type)
        {
            List<Type> createdInstance = new List<Type>();
            return ServerSerializationHelper.SerializeObject(CreateInstances(type, createdInstance), null, NullValueHandling.Include);
            object CreateInstances(Type newType, List<Type> items)
            {
                if (items.Contains(newType))
                    return DataExchangeConverter.GetDefault(newType);
                items.Add(newType);

                object result = null;
                SerializeObjectType typeCode = SerializeHelper.GetTypeCodeOfObject(newType);
                if (typeCode == SerializeObjectType.Object)
                {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                    if (!newType.GetTypeInfo().IsInterface)
#else
                    if (!newType.IsInterface)
#endif
                    {
                        try
                        {
                            result = Activator.CreateInstance(newType);
                            foreach (PropertyInfo item in newType.GetProperties())
                            {
                                item.SetValue(result, CreateInstances(item.PropertyType, items), null);
                            }
                        }
                        catch (Exception)
                        {


                        }
                    }
                    else if (newType.GetGenericTypeDefinition() == typeof(ICollection<>)
                        || newType.GetGenericTypeDefinition() == typeof(IList<>))
                    {
                        try
                        {
                            Type gType = newType.GetListOfGenericArguments().FirstOrDefault();
                            Type listType = typeof(List<>).MakeGenericType(gType);
                            result = Activator.CreateInstance(listType);
                        }
                        catch
                        {

                        }
                    }
                }
                else
                {
                    result = DataExchangeConverter.GetDefault(newType);
                }
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                if (newType.GetTypeInfo().IsGenericType && result != null)
#else
                if (newType.IsGenericType && result != null)
#endif
                {
                    if (newType.GetGenericTypeDefinition() == typeof(List<>) || newType.GetGenericTypeDefinition() == typeof(ICollection<>)
                        || newType.GetGenericTypeDefinition() == typeof(IList<>))
                    {
                        Type gType = newType.GetListOfGenericArguments().FirstOrDefault();
                        if (gType != null)
                        {
                            try
                            {
                                object gResult = Activator.CreateInstance(gType);
                                foreach (PropertyInfo item in gType.GetProperties())
                                {
                                    item.SetValue(gResult, CreateInstances(item.PropertyType, items), null);
                                }
                                result.GetType().GetMethod("Add").Invoke(result, new object[] { gResult });
                                //col.Add(gResult);
                            }
                            catch
                            {


                            }
                        }
                    }
                }
                return result;
            }


        }

        private string SimpleTypeToJsonString(Type type)
        {
            object instance = null;

            try
            {
                instance = Activator.CreateInstance(type);
            }
            catch
            {
                if (type == typeof(string))
                    instance = "";
                else
                    instance = DataExchangeConverter.GetDefault(type);
            }
            if (instance == null)
                return "cannot create instance of this type!";
            return ServerSerializationHelper.SerializeObject(instance, null, NullValueHandling.Include);
        }

    }
}
