using SignalGo.Server.DataTypes;
using SignalGo.Server.Helpers;
using SignalGo.Server.Models;
using SignalGo.Shared.Converters;
using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Events;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.Http;
using SignalGo.Shared.IO;
using SignalGo.Shared.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Server.ServiceManager.Providers
{
    /// <summary>
    /// base of providers
    /// </summary>
    public abstract class BaseProvider
    {
        static internal Task<MethodCallbackInfo> CallMethod(MethodCallInfo callInfo, ClientInfo client, string json, ServerBase serverBase)
        {
#if (NET40 || NET35)
            return Task.Factory.StartNew(() =>
#else
            return Task.Run(() =>
#endif
            {
                return CallMethod(callInfo.ServiceName, callInfo.Guid, callInfo.MethodName, callInfo.Parameters.ToArray(), client, json, serverBase, null, null, out List<HttpKeyAttribute> httpKeyAttributes, out Type serviceType, out MethodInfo method, out object serviceInsatnce);
                //SendCallbackData(callback, client, serverBase);
            });
        }

        static internal MethodCallbackInfo CallMethod(string serviceName, string guid, string methodName, SignalGo.Shared.Models.ParameterInfo[] parameters, ClientInfo client, string json, ServerBase serverBase, HttpPostedFileInfo fileInfo, Func<MethodInfo, bool> canTakeMethod, out List<HttpKeyAttribute> httpKeyAttributes, out Type serviceType, out MethodInfo method, out object service)
        {
            serviceName = serviceName.ToLower();
            httpKeyAttributes = new List<HttpKeyAttribute>();
            OperationContext.CurrentTaskServer = serverBase;
            object result = null;
            method = null;
            serviceType = null;
            service = null;
            Exception exception = null;
            MethodCallbackInfo callback = new MethodCallbackInfo()
            {
                Guid = guid
            };

            try
            {
                serverBase.TaskOfClientInfoes.TryAdd(Task.CurrentId.GetValueOrDefault(), client.ClientId);

                if (!serverBase.RegisteredServiceTypes.TryGetValue(serviceName, out serviceType))
                    throw new Exception($"{client.IPAddress} {client.ClientId} Service {serviceName} not found");

                service = GetInstanceOfService(client, serviceName, serviceType, serverBase);
                if (service == null)
                    throw new Exception($"{client.IPAddress} {client.ClientId} service {serviceName} not found");

                if (fileInfo != null)
                {
                    if (service is IHttpClientInfo serviceClientInfo)
                    {
                        serviceClientInfo.SetFirstFile(fileInfo);
                    }
                    else if (client is HttpClientInfo httpClientInfo)
                    {
                        httpClientInfo.SetFirstFile(fileInfo);
                    }
                }
                List<SecurityContractAttribute> securityAttributes = new List<SecurityContractAttribute>();
                List<CustomDataExchangerAttribute> customDataExchanger = new List<CustomDataExchangerAttribute>();
                List<ClientLimitationAttribute> clientLimitationAttribute = new List<ClientLimitationAttribute>();
                List<ConcurrentLockAttribute> concurrentLockAttributes = new List<ConcurrentLockAttribute>();

                var allMethods = GetMethods(client, methodName, parameters, serviceType, customDataExchanger, securityAttributes, clientLimitationAttribute, concurrentLockAttributes, canTakeMethod).ToList();
                method = allMethods.FirstOrDefault();
                if (method == null)
                {
                    StringBuilder exceptionResult = new StringBuilder();
                    exceptionResult.AppendLine("<Exception>");
                    exceptionResult.AppendLine($"method {methodName} not found");
                    exceptionResult.AppendLine("<Parameters>");
                    foreach (var item in parameters)
                    {
                        exceptionResult.AppendLine((item.Value ?? "null;") + " name: " + (item.Name ?? "no name"));
                    }
                    exceptionResult.AppendLine("</Parameters>");
                    exceptionResult.AppendLine("<JSON>");
                    exceptionResult.AppendLine(json);
                    exceptionResult.AppendLine("</JSON>");
                    exceptionResult.AppendLine("</Exception>");
                    throw new Exception($"{client.IPAddress} {client.ClientId} " + exceptionResult.ToString());
                }

                List<object> parametersValues = new List<object>();
                int index = 0;
                var prms = method.GetParameters();
                foreach (var item in parameters)
                {
                    if (item.Value == null)
                        parametersValues.Add(DataExchangeConverter.GetDefault(prms[index].ParameterType));
                    else
                    {
                        var parameterDataExchanger = customDataExchanger.ToList();
                        parameterDataExchanger.AddRange(GetMethodParameterBinds(index, allMethods.ToArray()).Where(x => x.GetExchangerByUserCustomization(client)));
                        parametersValues.Add(ServerSerializationHelper.Deserialize(item.Value, prms[index].ParameterType, serverBase, customDataExchanger: parameterDataExchanger.ToArray(), client: client));
                    }
                    index++;
                }
                if (parameters.Length != prms.Length)
                {
                    for (int i = 0; i < prms.Length; i++)
                    {
                        if (parameters.Length <= i || prms[i].Name != parameters[i].Name)
                        {
                            parametersValues.Insert(i, prms[i].DefaultValue);
                        }
                    }
                }

                foreach (ClientLimitationAttribute attrib in clientLimitationAttribute)
                {
                    var allowAddresses = attrib.GetAllowAccessIpAddresses();
                    if (allowAddresses != null && allowAddresses.Length > 0)
                    {
                        if (!allowAddresses.Contains(client.IPAddress))
                        {
                            string msg = $"Client IP Have Not Access To Call Method: {client.IPAddress}";
                            serverBase.AutoLogger.LogText(msg);
                            callback.IsException = true;
                            callback.Data = msg;
                            return callback;
                        }
                    }
                    else
                    {
                        var denyAddresses = attrib.GetDenyAccessIpAddresses();
                        if (denyAddresses != null && denyAddresses.Length > 0)
                        {
                            if (denyAddresses.Contains(client.IPAddress))
                            {
                                string msg = $"Client IP Is Deny Access To Call Method: {client.IPAddress}";
                                serverBase.AutoLogger.LogText(msg);
                                callback.IsException = true;
                                callback.Data = msg;
                                serverBase.AutoLogger.LogText(msg);
                                return callback;
                            }
                        }
                    }
                }

                //when method have static locl attribute calling is going to lock
                var concurrentLockAttribute = concurrentLockAttributes.FirstOrDefault();

                MethodsCallHandler.BeginMethodCallAction?.Invoke(client, guid, serviceName, method, parameters);

                //check if client have permissions for call method
                bool canCall = true;
                foreach (SecurityContractAttribute attrib in securityAttributes)
                {
                    if (!attrib.CheckPermission(client, service, method, parametersValues))
                    {
                        callback.IsAccessDenied = true;
                        canCall = false;
                        if (method.ReturnType != typeof(void))
                        {
                            object data = null;
                            data = attrib.GetValueWhenDenyPermission(client, service, method, parametersValues);
                            callback.Data = data == null ? null : ServerSerializationHelper.SerializeObject(data, serverBase, customDataExchanger: customDataExchanger.ToArray(), client: client);
                        }
                        break;
                    }
                }

                if (canCall)
                {
                    try
                    {
                        if (concurrentLockAttribute != null)
                        {
                            switch (concurrentLockAttribute.Type)
                            {
                                case ConcurrentLockType.Full:
                                    {
                                        lock (serverBase)
                                        {
                                            result = method.Invoke(service, parametersValues.ToArray());
                                        }
                                        break;
                                    }
                                case ConcurrentLockType.PerClient:
                                    {
                                        lock (client)
                                        {
                                            result = method.Invoke(service, parametersValues.ToArray());
                                        }
                                        break;
                                    }
                                case ConcurrentLockType.PerIpAddress:
                                    {
                                        lock (client.IPAddress)
                                        {
                                            result = method.Invoke(service, parametersValues.ToArray());
                                        }
                                        break;
                                    }
                                case ConcurrentLockType.PerMethod:
                                    {
                                        lock (method)
                                        {
                                            result = method.Invoke(service, parametersValues.ToArray());
                                        }
                                        break;
                                    }
                            }
                        }
                        else
                            result = method.Invoke(service, parametersValues.ToArray());

                        var httpKeyOnMethod = (HttpKeyAttribute)method.GetCustomAttributes(typeof(HttpKeyAttribute), true).FirstOrDefault();
                        if (httpKeyOnMethod != null)
                            httpKeyAttributes.Add(httpKeyOnMethod);
                        if (serverBase.ProviderSetting.HttpKeyResponses != null)
                        {
                            httpKeyAttributes.AddRange(serverBase.ProviderSetting.HttpKeyResponses);
                        }

                        callback.Data = result == null ? null : ServerSerializationHelper.SerializeObject(result, serverBase, customDataExchanger: customDataExchanger.ToArray(), client: client);
                    }
                    catch (Exception ex)
                    {
                        if (serverBase.ErrorHandlingFunction != null)
                            result = serverBase.ErrorHandlingFunction(ex, serviceType, method);
                        exception = ex;
                        serverBase.AutoLogger.LogError(ex, $"{client.IPAddress} {client.ClientId} ServerBase CallMethod: {methodName}");
                        callback.IsException = true;
                        callback.Data = ServerSerializationHelper.SerializeObject(ex.ToString(), serverBase);
                    }
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                serverBase.AutoLogger.LogError(ex, $"{client.IPAddress} {client.ClientId} ServerBase CallMethod 2: {methodName}");
                callback.IsException = true;
                callback.Data = ServerSerializationHelper.SerializeObject(ex.ToString(), serverBase);
            }
            finally
            {
                serverBase.TaskOfClientInfoes.TryRemove(Task.CurrentId.GetValueOrDefault(), out string clientId);

                try
                {
                    MethodsCallHandler.EndMethodCallAction?.Invoke(client, guid, serviceName, method, parameters, callback?.Data, exception);
                }
                catch (Exception ex)
                {
                    serverBase.AutoLogger.LogError(ex, $"{client.IPAddress} {client.ClientId} ServerBase CallMethod 2: {methodName}");
                }
            }
            DataExchanger.Clear();
            return callback;
        }


        static object GetInstanceOfService(ClientInfo client, string serviceName, Type serviceType, ServerBase serverBase)
        {
            ServiceContractAttribute attribute = serviceType.GetServerServiceAttribute(serviceName);

            if (attribute.InstanceType == InstanceType.SingleInstance)
            {
                //single instance services must create instance when server starting so this must always true
                serverBase.SingleInstanceServices.TryGetValue(attribute.Name, out object result);
                return result;
            }
            else
            {
                lock (client)
                {
                    //finx service from multi instance services
                    if (serverBase.MultipleInstanceServices.TryGetValue(attribute.Name, out ConcurrentDictionary<string, object> result))
                    {
                        if (result.TryGetValue(client.ClientId, out object service))
                        {
                            return service;
                        }
                        else
                        {
                            service = Activator.CreateInstance(serviceType);
                            result.TryAdd(client.ClientId, service);
                            return service;
                        }
                    }
                    else
                    {
                        result = new ConcurrentDictionary<string, object>();
                        serverBase.MultipleInstanceServices.TryAdd(attribute.Name, result);
                        var service = Activator.CreateInstance(serviceType);
                        result.TryAdd(client.ClientId, service);
                        return service;
                    }
                }
            }
        }

        static IEnumerable<MethodInfo> GetMethods(ClientInfo client
            , string methodName
            , Shared.Models.ParameterInfo[] parameters
            , Type serviceType
            , List<CustomDataExchangerAttribute> customDataExchangerAttributes
            , List<SecurityContractAttribute> securityContractAttributes
            , List<ClientLimitationAttribute> clientLimitationAttributes
            , List<ConcurrentLockAttribute> concurrentLockAttributes
            , Func<MethodInfo, bool> canTakeMethod)
        {
            var list = serviceType.GetTypesByAttribute<ServiceContractAttribute>(x => true).ToList();
            foreach (var item in list)
            {
                var method = FindMethod(item, methodName, parameters, canTakeMethod);
                if (method != null && method.IsPublic && !method.IsStatic)
                {
                    if (canTakeMethod != null && !canTakeMethod(method))
                        continue;
                    securityContractAttributes.AddRange(method.GetCustomAttributes(typeof(SecurityContractAttribute), true).Cast<SecurityContractAttribute>());
                    customDataExchangerAttributes.AddRange(method.GetCustomAttributes(typeof(CustomDataExchangerAttribute), true).Cast<CustomDataExchangerAttribute>().Where(x => x.GetExchangerByUserCustomization(client)));
                    clientLimitationAttributes.AddRange(method.GetCustomAttributes(typeof(ClientLimitationAttribute), true).Cast<ClientLimitationAttribute>());
                    concurrentLockAttributes.AddRange(method.GetCustomAttributes(typeof(ConcurrentLockAttribute), true).Cast<ConcurrentLockAttribute>());
                    yield return method;
                }
            }
        }

        static CustomDataExchangerAttribute[] GetMethodParameterBinds(int parameterIndex, params MethodInfo[] methodInfoes)
        {
            List<CustomDataExchangerAttribute> result = new List<CustomDataExchangerAttribute>();
            foreach (var method in methodInfoes)
            {
                var parameter = method.GetParameters()[parameterIndex];
                List<CustomDataExchangerAttribute> items = new List<CustomDataExchangerAttribute>();
                foreach (var find in parameter.GetCustomAttributes(typeof(CustomDataExchangerAttribute), true).Cast<CustomDataExchangerAttribute>())
                {
                    find.Type = parameter.ParameterType;
                    items.Add(find);
                }
                result.AddRange(items);
            }

            return result.ToArray();
        }

        static ConcurrentDictionary<string, MethodInfo> CachedMethods { get; set; } = new ConcurrentDictionary<string, MethodInfo>();

        static System.Reflection.MethodInfo FindMethod(Type serviceType, string methodName, Shared.Models.ParameterInfo[] parameters, Func<MethodInfo, bool> canTakeMethod)
        {
            methodName = methodName.ToLower();
            var key = GenerateMethodKey(serviceType, methodName, parameters);
            if (CachedMethods.TryGetValue(key, out MethodInfo methodInfo))
                return methodInfo;

            var method = FindMethodByType(serviceType, methodName, parameters, canTakeMethod);
            if (method != null)
            {
                CachedMethods.TryAdd(key, method);
                return method;
            }

            foreach (var item in serviceType.GetInterfaces())
            {
                method = FindMethodByType(item, methodName, parameters, canTakeMethod);
                if (method != null)
                {
                    CachedMethods.TryAdd(key, method);
                    return method;
                }
            }
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var parent = serviceType.GetTypeInfo().BaseType;
#else
            var parent = serviceType.BaseType;
#endif
            while (parent != null)
            {
                method = FindMethodByType(parent, methodName, parameters, canTakeMethod);
                if (method != null)
                {
                    CachedMethods.TryAdd(key, method);
                    return method;
                }

                foreach (var item in parent.GetInterfaces())
                {
                    method = FindMethodByType(item, methodName, parameters, canTakeMethod);
                    if (method != null)
                    {
                        CachedMethods.TryAdd(key, method);
                        return method;
                    }
                }
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                parent = parent.GetTypeInfo().BaseType;
#else
                parent = parent.BaseType;
#endif
            }

            return null;
        }

        static System.Reflection.MethodInfo FindMethodByType(Type serviceType, string methodName, Shared.Models.ParameterInfo[] parameters, Func<MethodInfo, bool> canTakeMethod)
        {
            IEnumerable<MethodInfo> query = null;
            if (methodName == "-noName-" && canTakeMethod != null)
            {
                query = serviceType.GetMethods().Where(x => canTakeMethod(x) && !(x.IsSpecialName && (x.Name.StartsWith("set_") || x.Name.StartsWith("get_"))));
            }
            else
            {
                query = serviceType.GetMethods().Where(x => x.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase) && !(x.IsSpecialName && (x.Name.StartsWith("set_") || x.Name.StartsWith("get_"))));
            }
            foreach (var method in query)
            {
                var param = method.GetParameters();
                bool hasError = false;
                foreach (var p in parameters)
                {
                    if (!string.IsNullOrEmpty(p.Name) && !param.Any(x => x.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase) && param.IndexOf(x) == parameters.IndexOf(p)))
                    {
                        hasError = true;
                        break;
                    }
                }
#if (!NET35 && !NET40)
                if (!hasError && param.Length != parameters.Length)
                {
                    foreach (var p in param)
                    {
                        if (!parameters.Any(x => x.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            if (!p.HasDefaultValue)
                                hasError = true;
                            break;
                        }
                    }
                }
#endif
                if (hasError)
                    continue;
                else
                    return method;
            }
            return null;
        }

        static string GenerateMethodKey(Type serviceType, string methodName, Shared.Models.ParameterInfo[] parameters)
        {
            var name = serviceType.FullName + methodName;
            foreach (var item in parameters)
            {
                name += " " + item.Name + " ";
            }
            return name;
        }
    }
}
