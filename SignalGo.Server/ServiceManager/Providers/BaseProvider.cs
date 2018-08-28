using SignalGo.Server.DataTypes;
using SignalGo.Server.Helpers;
using SignalGo.Server.Models;
using SignalGo.Shared.Converters;
using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Events;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.Http;
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
        internal static Task<MethodCallbackInfo> CallMethod(MethodCallInfo callInfo, ClientInfo client, string json, ServerBase serverBase)
        {
#if (NET40 || NET35)
            return Task.Factory.StartNew(() =>
#else
            return Task.Run(() =>
#endif
            {
                serverBase.AddTask(Task.CurrentId.GetValueOrDefault(), client.ClientId);
                try
                {
                    return CallMethod(callInfo.ServiceName, callInfo.Guid, callInfo.MethodName, callInfo.Parameters.ToArray(), client, json, serverBase, null, null, out IStreamInfo streamInfo, out List<HttpKeyAttribute> httpKeyAttributes, out Type serviceType, out MethodInfo method, out object serviceInsatnce, out FileActionResult fileActionResult);
                }
                finally
                {
                    serverBase.RemoveTask(Task.CurrentId.GetValueOrDefault());
                }
                //SendCallbackData(callback, client, serverBase);
            });
        }

        internal static MethodCallbackInfo CallMethod(string serviceName, string guid, string methodName, SignalGo.Shared.Models.ParameterInfo[] parameters, ClientInfo client, string json, ServerBase serverBase, HttpPostedFileInfo fileInfo, Func<MethodInfo, bool> canTakeMethod, out IStreamInfo streamInfo, out List<HttpKeyAttribute> httpKeyAttributes, out Type serviceType, out MethodInfo method, out object service, out FileActionResult fileActionResult)
        {
            serviceName = serviceName.ToLower();
            httpKeyAttributes = new List<HttpKeyAttribute>();
            OperationContext.CurrentTaskServer = serverBase;
            object result = null;
            method = null;
            serviceType = null;
            service = null;
            Exception exception = null;
            fileActionResult = null;
            streamInfo = null;
            MethodCallbackInfo callback = new MethodCallbackInfo()
            {
                Guid = guid
            };

            try
            {

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

                List<MethodInfo> allMethods = GetMethods(client, methodName, parameters, serviceType, customDataExchanger, securityAttributes, clientLimitationAttribute, concurrentLockAttributes, canTakeMethod).ToList();
                method = allMethods.FirstOrDefault();
                if (method == null)
                {
                    StringBuilder exceptionResult = new StringBuilder();
                    exceptionResult.AppendLine("<Exception>");
                    exceptionResult.AppendLine($"method {methodName} not found");
                    exceptionResult.AppendLine("<Parameters>");
                    if (parameters != null)
                    {
                        foreach (Shared.Models.ParameterInfo item in parameters)
                        {
                            exceptionResult.AppendLine((item.Value ?? "null;") + " name: " + (item.Name ?? "no name"));
                        }
                    }
                    exceptionResult.AppendLine("</Parameters>");
                    exceptionResult.AppendLine("<JSON>");
                    exceptionResult.AppendLine(json);
                    exceptionResult.AppendLine("</JSON>");
                    exceptionResult.AppendLine("</Exception>");
                    throw new Exception($"{client.IPAddress} {client.ClientId} " + exceptionResult.ToString());
                }

                List<object> parametersValues = new List<object>();
                if (parameters != null)
                {
                    int index = 0;
                    System.Reflection.ParameterInfo[] prms = method.GetParameters();
                    foreach (Shared.Models.ParameterInfo item in parameters)
                    {
                        if (item.Value == null)
                            parametersValues.Add(DataExchangeConverter.GetDefault(prms[index].ParameterType));
                        else
                        {
                            List<CustomDataExchangerAttribute> parameterDataExchanger = customDataExchanger.ToList();
                            parameterDataExchanger.AddRange(GetMethodParameterBinds(index, allMethods.ToArray()).Where(x => x.GetExchangerByUserCustomization(client)));
                            object resultJson = ServerSerializationHelper.Deserialize(item.Value, prms[index].ParameterType, serverBase, customDataExchanger: parameterDataExchanger.ToArray(), client: client);

                            if (resultJson == null)
                            {
                                if (string.IsNullOrEmpty(item.Value))
                                    parametersValues.Add(null);
                                else
                                    parametersValues.Add(item.Value);
                            }
                            else
                            {
                                parametersValues.Add(resultJson);
                                if (resultJson is IStreamInfo _streamInfo)
                                    _streamInfo.Stream = client.ClientStream;
                            }
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
                }

                foreach (ClientLimitationAttribute attrib in clientLimitationAttribute)
                {
                    string[] allowAddresses = attrib.GetAllowAccessIpAddresses();
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
                        string[] denyAddresses = attrib.GetDenyAccessIpAddresses();
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
                ConcurrentLockAttribute concurrentLockAttribute = concurrentLockAttributes.FirstOrDefault();

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
                //var data = (IStreamInfo)parametersValues.FirstOrDefault(x => x.GetType() == typeof(StreamInfo) || (x.GetType().GetIsGenericType() && x.GetType().GetGenericTypeDefinition() == typeof(StreamInfo<>)));
                //var upStream = new UploadStreamGo(stream);
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

                        HttpKeyAttribute httpKeyOnMethod = (HttpKeyAttribute)method.GetCustomAttributes(typeof(HttpKeyAttribute), true).FirstOrDefault();
                        if (httpKeyOnMethod != null)
                            httpKeyAttributes.Add(httpKeyOnMethod);
                        if (serverBase.ProviderSetting.HttpKeyResponses != null)
                        {
                            httpKeyAttributes.AddRange(serverBase.ProviderSetting.HttpKeyResponses);
                        }

                        if (result != null && result.GetType() == typeof(Task))
                        {
                            Task taskResult = (Task)result;
#if (NET40 || NET35)
                            taskResult.Wait();
#else
                            taskResult.Wait();
#endif
                            result = null;
                        }
                        //this is async function
                        else if (result != null && result.GetType().GetBaseType() == typeof(Task))
                        {
#if (NET40 || NET35)
                            Task task = (Task)result;
                            task.Wait();
                            result = task.GetType().GetProperty("Result").GetValue(task, null);
#else
                            var task = ((Task)result);
                            task.Wait();
                            result = task.GetType().GetProperty("Result").GetValue(task, null);
#endif
                        }

                        if (result is FileActionResult fResult)
                            fileActionResult = fResult;
                        else
                        {
                            if (result is IStreamInfo iSResult)
                                streamInfo = iSResult;
                            callback.Data = result == null ? null : ServerSerializationHelper.SerializeObject(result, serverBase, customDataExchanger: customDataExchanger.ToArray(), client: client);
                        }
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
                //serverBase.TaskOfClientInfoes.TryRemove(Task.CurrentId.GetValueOrDefault(), out string clientId);

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

        private static object GetInstanceOfService(ClientInfo client, string serviceName, Type serviceType, ServerBase serverBase)
        {
            ServiceContractAttribute attribute = serviceType.GetServerServiceAttribute(serviceName, false);

            if (attribute.InstanceType == InstanceType.SingleInstance)
            {
                //single instance services must create instance when server starting so this must always true
                if (!serverBase.SingleInstanceServices.TryGetValue(attribute.Name, out object result))
                {
                    result = Activator.CreateInstance(serviceType);
                    serverBase.SingleInstanceServices.TryAdd(attribute.Name, result);
                }
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
                        object service = Activator.CreateInstance(serviceType);
                        result.TryAdd(client.ClientId, service);
                        return service;
                    }
                }
            }
        }

        private static IEnumerable<MethodInfo> GetMethods(ClientInfo client
            , string methodName
            , Shared.Models.ParameterInfo[] parameters
            , Type serviceType
            , List<CustomDataExchangerAttribute> customDataExchangerAttributes
            , List<SecurityContractAttribute> securityContractAttributes
            , List<ClientLimitationAttribute> clientLimitationAttributes
            , List<ConcurrentLockAttribute> concurrentLockAttributes
            , Func<MethodInfo, bool> canTakeMethod)
        {
            List<Type> list = serviceType.GetTypesByAttribute<ServiceContractAttribute>(x => true).ToList();
            foreach (Type item in list)
            {
                MethodInfo method = FindMethod(item, methodName, parameters, canTakeMethod);
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

        private static CustomDataExchangerAttribute[] GetMethodParameterBinds(int parameterIndex, params MethodInfo[] methodInfoes)
        {
            List<CustomDataExchangerAttribute> result = new List<CustomDataExchangerAttribute>();
            foreach (MethodInfo method in methodInfoes)
            {
                System.Reflection.ParameterInfo parameter = method.GetParameters()[parameterIndex];
                List<CustomDataExchangerAttribute> items = new List<CustomDataExchangerAttribute>();
                foreach (CustomDataExchangerAttribute find in parameter.GetCustomAttributes(typeof(CustomDataExchangerAttribute), true).Cast<CustomDataExchangerAttribute>())
                {
                    find.Type = parameter.ParameterType;
                    items.Add(find);
                }
                result.AddRange(items);
            }

            return result.ToArray();
        }

        private static ConcurrentDictionary<string, MethodInfo> CachedMethods { get; set; } = new ConcurrentDictionary<string, MethodInfo>();

        private static System.Reflection.MethodInfo FindMethod(Type serviceType, string methodName, Shared.Models.ParameterInfo[] parameters, Func<MethodInfo, bool> canTakeMethod)
        {
            methodName = methodName.ToLower();
            string key = GenerateMethodKey(serviceType, methodName, parameters);
            if (CachedMethods.TryGetValue(key, out MethodInfo methodInfo))
                return methodInfo;

            MethodInfo method = FindMethodByType(serviceType, methodName, parameters, canTakeMethod);
            if (method != null)
            {
                CachedMethods.TryAdd(key, method);
                return method;
            }

            foreach (Type item in serviceType.GetInterfaces())
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
            Type parent = serviceType.BaseType;
#endif
            while (parent != null)
            {
                method = FindMethodByType(parent, methodName, parameters, canTakeMethod);
                if (method != null)
                {
                    CachedMethods.TryAdd(key, method);
                    return method;
                }

                foreach (Type item in parent.GetInterfaces())
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

        private static System.Reflection.MethodInfo FindMethodByType(Type serviceType, string methodName, Shared.Models.ParameterInfo[] parameters, Func<MethodInfo, bool> canTakeMethod)
        {
            IEnumerable<MethodInfo> query = null;
            if (methodName == "-noname-" && canTakeMethod != null)
            {
                query = serviceType.GetMethods().Where(x => canTakeMethod(x) && !(x.IsSpecialName && (x.Name.StartsWith("set_") || x.Name.StartsWith("get_"))));
            }
            else
            {
                query = serviceType.GetMethods().Where(x => x.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase) && !(x.IsSpecialName && (x.Name.StartsWith("set_") || x.Name.StartsWith("get_"))));
            }
            foreach (MethodInfo method in query)
            {
                System.Reflection.ParameterInfo[] param = method.GetParameters();
                bool hasError = false;
                if (parameters != null)
                {
                    foreach (Shared.Models.ParameterInfo p in parameters)
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
                }
                if (hasError)
                    continue;
                else
                    return method;
            }
            return null;
        }

        private static string GenerateMethodKey(Type serviceType, string methodName, Shared.Models.ParameterInfo[] parameters)
        {
            if (parameters == null)
                return "";
            string name = serviceType.FullName + methodName;
            foreach (Shared.Models.ParameterInfo item in parameters)
            {
                name += " " + item.Name + " ";
            }
            return name;
        }


        /// <summary>
        /// send result of calling method from client
        /// client is waiting for get response from server when calling method
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="client"></param>
        /// <param name="serverBase"></param>
#if (NET35 || NET40)
        internal static void SendCallbackDataAsync(Task<MethodCallbackInfo> callback, ClientInfo client, ServerBase serverBase)
#else
        internal static async void SendCallbackDataAsync(Task<MethodCallbackInfo> callback, ClientInfo client, ServerBase serverBase)
#endif
        {
            try
            {
#if (NET35 || NET40)
                MethodCallbackInfo result = callback.Result;
#else
                var result = await callback;
#endif
                SendCallbackData(result, client, serverBase);
            }
            catch (Exception ex)
            {
                serverBase.AutoLogger.LogError(ex, $"{client.IPAddress} {client.ClientId} ServerBase SendCallbackData");
                //if (!client.TcpClient.Connected)
                //    serverBase.DisposeClient(client, "SendCallbackData exception");
            }
            finally
            {
                //ClientConnectedCallingCount--;
            }
        }


        /// <summary>
        /// send result of calling method from client
        /// client is waiting for get response from server when calling method
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="client"></param>
        /// <param name="serverBase"></param>
        internal static void SendCallbackData(MethodCallbackInfo callback, ClientInfo client, ServerBase serverBase)
        {
            string json = ServerSerializationHelper.SerializeObject(callback, serverBase);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            //if (ClientsSettings.ContainsKey(client))
            //    bytes = EncryptBytes(bytes, client);
            byte[] len = BitConverter.GetBytes(bytes.Length);
            List<byte> data = new List<byte>
                    {
                        (byte)DataType.ResponseCallMethod,
                        (byte)CompressMode.None
                    };
            data.AddRange(len);
            data.AddRange(bytes);
            if (data.Count > serverBase.ProviderSetting.MaximumSendDataBlock)
                throw new Exception($"{client.IPAddress} {client.ClientId} SendCallbackData data length exceeds MaximumSendDataBlock");

            client.StreamHelper.WriteToStream(client.ClientStream, data.ToArray());
        }
    }
}
