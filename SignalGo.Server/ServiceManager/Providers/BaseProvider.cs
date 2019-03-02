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
        internal static async Task<MethodCallbackInfo> CallMethod(MethodCallInfo callInfo, ClientInfo client, string json, ServerBase serverBase)
        {
            try
            {
                CallMethodResultInfo<OperationContext> result = await CallMethod(callInfo.ServiceName, callInfo.Guid, callInfo.MethodName, callInfo.Parameters.ToArray(), null, client, json, serverBase, null, null);
                return result.CallbackInfo;
            }
            finally
            {
            }
        }

        public static bool ExistService(string serviceName, ServerBase serverBase)
        {
            if (serviceName == null)
                return false;
            serviceName = serviceName.ToLower();
            return serverBase.RegisteredServiceTypes.ContainsKey(serviceName);
        }

        public static Task<CallMethodResultInfo<OperationContext>> CallMethod(string serviceName, string guid, string methodName, SignalGo.Shared.Models.ParameterInfo[] parameters, string jsonParameters, ClientInfo client, string json, ServerBase serverBase, HttpPostedFileInfo fileInfo, Func<MethodInfo, bool> canTakeMethod)
        {
            return Task.Run(async () =>
            {
                int taskId = Task.CurrentId.GetValueOrDefault();
                serverBase.AddTask(taskId, client.ClientId);

                object result = null;
                MethodInfo method = null;//4
                Type serviceType = null;//3
                object service = null;//5
                Exception exception = null;
                FileActionResult fileActionResult = null;//6
                IStreamInfo streamInfo = null;//1
                MethodCallbackInfo callback = null;
                List<HttpKeyAttribute> httpKeyAttributes = new List<HttpKeyAttribute>();//2
                OperationContext context = OperationContext.Current;
                bool? isEnabledReferenceResolver = null;
                bool? isEnabledReferenceResolverForArray = null;
                try
                {
                    serviceName = serviceName.ToLower();
                    OperationContext.CurrentTaskServer = serverBase;

                    callback = new MethodCallbackInfo()
                    {
                        Guid = guid
                    };
                    if (!serverBase.RegisteredServiceTypes.TryGetValue(serviceName, out serviceType))
                    {
                        if (!serverBase.RegisteredServiceTypes.TryGetValue("", out serviceType))
                            throw new Exception($"{client.IPAddress} {client.ClientId} Service {serviceName} not found");
                        else
                        {
                            if (parameters == null || parameters.Length == 0)
                                parameters = new Shared.Models.ParameterInfo[] { new Shared.Models.ParameterInfo() { Value = methodName } };
                            methodName = serviceName;
                            serviceName = "";
                            methodName = methodName.Split('.').FirstOrDefault();
                        }
                    }
                    else if (string.IsNullOrEmpty(serviceName))
                    {
                        methodName = methodName.Split('.').FirstOrDefault();
                    }
                    service = await GetInstanceOfService(client, serviceName, serviceType, serverBase);
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

                    IEnumerable<MethodInfo> allMethods = GetMethods(client, methodName, parameters, serviceType, customDataExchanger, securityAttributes, clientLimitationAttribute, concurrentLockAttributes, canTakeMethod);
                    method = allMethods.FirstOrDefault();
                    //if (method == null && !string.IsNullOrEmpty(jsonParameters))
                    //{
                    //    parameters = new Shared.Models.ParameterInfo[] { new Shared.Models.ParameterInfo() { Name = "", Value = jsonParameters } };
                    //    allMethods = GetMethods(client, methodName, parameters, serviceType, customDataExchanger, securityAttributes, clientLimitationAttribute, concurrentLockAttributes, canTakeMethod).ToList();
                    //    method = allMethods.FirstOrDefault();
                    //}

                    if (method == null)
                    {
                        //find method with sended parameters
                        method = GetMethods(client, methodName, null, serviceType, customDataExchanger, securityAttributes, clientLimitationAttribute, concurrentLockAttributes, canTakeMethod).FirstOrDefault();
                    }

                    if (method == null)
                    {
                        StringBuilder exceptionResult = new StringBuilder();
                        exceptionResult.AppendLine("<Exception>");
                        exceptionResult.AppendLine($"method {methodName} not found from service {serviceName}");
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
                    ActivityReferenceResolverAttribute referenceAttribute = method.GetCustomAttributes<ActivityReferenceResolverAttribute>(true).FirstOrDefault();

                    if (referenceAttribute != null)
                    {
                        isEnabledReferenceResolver = referenceAttribute.IsEnabledReferenceResolver;
                        isEnabledReferenceResolverForArray = referenceAttribute.IsEnabledReferenceResolverForArray;
                    }


                    string keyParameterValue = null;
                    if (parameters == null)
                        parameters = new Shared.Models.ParameterInfo[0];
                    List<object> parametersValues = FixParametersCount(taskId, service, method, parameters.ToList(), serverBase, client, allMethods, customDataExchanger, jsonParameters, out List<BaseValidationRuleInfoAttribute> validationErrors, ref keyParameterValue);

                    if (validationErrors != null && validationErrors.Count > 0)
                    {
                        if (serverBase.ValidationResultHandlingFunction == null)
                        {
                            StringBuilder exceptionMessageBuilder = new StringBuilder();
                            exceptionMessageBuilder.AppendLine("Validation Exception detected!");
                            foreach (BaseValidationRuleInfoAttribute validation in validationErrors)
                            {
                                object errorValue = BaseValidationRuleInfoAttribute.GetErrorValue(validation);
                                if (errorValue == null)
                                    throw new Exception("validation error value cannot be null");
                                exceptionMessageBuilder.Append("Validation Exception:");
                                exceptionMessageBuilder.AppendLine(ServerSerializationHelper.SerializeObject(errorValue, serverBase, customDataExchanger: customDataExchanger.ToArray(), client: client, isEnabledReferenceResolver: isEnabledReferenceResolver, isEnabledReferenceResolverForArray: isEnabledReferenceResolverForArray));
                            }
                            throw new Exception(exceptionMessageBuilder.ToString());
                        }
                        else
                        {
                            result = serverBase.ValidationResultHandlingFunction(validationErrors, service, method);
                            callback.Data = result == null ? null : ServerSerializationHelper.SerializeObject(result, serverBase, customDataExchanger: customDataExchanger.ToArray(), client: client, isEnabledReferenceResolver: isEnabledReferenceResolver, isEnabledReferenceResolverForArray: isEnabledReferenceResolverForArray);
                        }
                    }
                    else
                    {
                        if (OperationContextBase.SavedKeyParametersNameSettings.Count > 0 && client is HttpClientInfo httpClient)
                        {
                            //Shared.Models.ParameterInfo find = parameters.FirstOrDefault(x => OperationContextBase.SavedKeyParametersNameSettings.ContainsKey(x.Name.ToLower()));
                            //if (find == null && !string.IsNullOrEmpty(jsonParameters))
                            //{
                            //    try
                            //    {
                            //        JToken token = JToken.Parse(jsonParameters);
                            //        httpClient.HttpKeyParameterValue = token["key"].ToString();
                            //    }
                            //    catch (Exception ex)
                            //    {
                            //        string decode = Uri.UnescapeDataString(jsonParameters);
                            //        if (decode.ToLower().Contains("key="))
                            //        {
                            //            string[] split = Regex.Split(decode, "key=", RegexOptions.IgnoreCase);
                            //            string data = split.LastOrDefault().TrimStart('"').TrimEnd('"');
                            //            httpClient.HttpKeyParameterValue = data;
                            //        }
                            //    }
                            //}
                            //if (find != null)
                            //httpClient.HttpKeyParameterValue = find.Value;
                            httpClient.HttpKeyParameterValue = keyParameterValue;
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
                                    return new CallMethodResultInfo<OperationContext>(callback, streamInfo, httpKeyAttributes, serviceType, method, service, fileActionResult, context, null);
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
                                        return new CallMethodResultInfo<OperationContext>(callback, streamInfo, httpKeyAttributes, serviceType, method, service, fileActionResult, context, null);
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
                                    callback.Data = data == null ? null : ServerSerializationHelper.SerializeObject(data, serverBase, customDataExchanger: customDataExchanger.ToArray(), client: client, isEnabledReferenceResolver: isEnabledReferenceResolver, isEnabledReferenceResolverForArray: isEnabledReferenceResolverForArray);
                                }
                                break;
                            }
                        }
                        foreach (object item in parametersValues)
                        {
                            if (item is BaseStreamInfo stream)
                            {
                                stream.Stream = client.ClientStream;
                            }
                        }
                        //var data = (IStreamInfo)parametersValues.FirstOrDefault(x => x.GetType() == typeof(StreamInfo) || (x.GetType().GetIsGenericType() && x.GetType().GetGenericTypeDefinition() == typeof(StreamInfo<>)));
                        //var upStream = new UploadStreamGo(stream);
                        if (canCall)
                        {
                            try
                            {
                                Task taskResult = null;
                                if (concurrentLockAttribute != null)
                                {
                                    switch (concurrentLockAttribute.Type)
                                    {
                                        case ConcurrentLockType.Full:
                                            {
                                                try
                                                {
                                                    await serverBase.LockWaitToRead.WaitAsync();
                                                    if (IsTask(method))
                                                        taskResult = (Task)method.Invoke(service, parametersValues.ToArray());
                                                    else
                                                        result = method.Invoke(service, parametersValues.ToArray());
                                                }
                                                finally
                                                {
                                                    serverBase.LockWaitToRead.Release();
                                                }
                                                break;
                                            }
                                        case ConcurrentLockType.PerClient:
                                            {
                                                try
                                                {
                                                    await client.LockWaitToRead.WaitAsync();
                                                    if (IsTask(method))
                                                        taskResult = (Task)method.Invoke(service, parametersValues.ToArray());
                                                    else
                                                        result = method.Invoke(service, parametersValues.ToArray());
                                                }
                                                finally
                                                {
                                                    client.LockWaitToRead.Release();
                                                }
                                                break;
                                            }
                                        case ConcurrentLockType.PerIpAddress:
                                            {
                                                lock (client.IPAddress)
                                                {
                                                    if (IsTask(method))
                                                        taskResult = (Task)method.Invoke(service, parametersValues.ToArray());
                                                    else
                                                        result = method.Invoke(service, parametersValues.ToArray());
                                                }
                                                break;
                                            }
                                        case ConcurrentLockType.PerMethod:
                                            {
                                                lock (method)
                                                {
                                                    if (IsTask(method))
                                                        taskResult = (Task)method.Invoke(service, parametersValues.ToArray());
                                                    else
                                                        result = method.Invoke(service, parametersValues.ToArray());
                                                }
                                                break;
                                            }
                                        case ConcurrentLockType.PerSingleInstanceService:
                                            {
                                                lock (service)
                                                {
                                                    if (IsTask(method))
                                                        taskResult = (Task)method.Invoke(service, parametersValues.ToArray());
                                                    else
                                                        result = method.Invoke(service, parametersValues.ToArray());
                                                }
                                                break;
                                            }
                                    }
                                }
                                else
                                {
                                    if (IsTask(method))
                                        taskResult = (Task)method.Invoke(service, parametersValues.ToArray());
                                    else
                                        result = method.Invoke(service, parametersValues.ToArray());
                                }
                                if (taskResult != null)
                                {
                                    await taskResult;
                                    if (taskResult.GetType() != typeof(Task))
                                        result = taskResult.GetType().GetProperty("Result").GetValue(taskResult);
                                }

                                if (result != null && result.GetType() == typeof(Task))
                                {
                                    await (Task)result;
                                    result = null;
                                }
                                //this is async function
                                else if (result != null && result.GetType().GetBaseType() == typeof(Task))
                                {
                                    Task task = ((Task)result);
                                    await task;
                                    result = task.GetType().GetProperty("Result").GetValue(task, null);
                                }

                                if (result is FileActionResult fResult)
                                    fileActionResult = fResult;
                                else
                                {
                                    if (result is IStreamInfo iSResult)
                                        streamInfo = iSResult;
                                    if (result == null)
                                        callback.Data = null;
                                    else
                                        callback.Data = ServerSerializationHelper.SerializeObject(result, serverBase, customDataExchanger: customDataExchanger.ToArray(), client: client, isEnabledReferenceResolver: isEnabledReferenceResolver, isEnabledReferenceResolverForArray: isEnabledReferenceResolverForArray);


                                }
                            }
                            catch (Exception ex)
                            {
                                if (serverBase.ErrorHandlingFunction != null)
                                    result = serverBase.ErrorHandlingFunction(ex, serviceType, method, client);
                                exception = ex;
                                serverBase.AutoLogger.LogError(ex, $"{client.IPAddress} {client.ClientId} ServerBase CallMethod: {methodName}");
                                callback.IsException = true;
                                callback.Data = ServerSerializationHelper.SerializeObject(ex.ToString(), serverBase, isEnabledReferenceResolver: isEnabledReferenceResolver, isEnabledReferenceResolverForArray: isEnabledReferenceResolverForArray);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                    serverBase.AutoLogger.LogError(ex, $"{client.IPAddress} {client.ClientId} ServerBase CallMethod 2: {methodName} serviceName: {serviceName}");
                    callback.IsException = true;
                    if (serverBase.ErrorHandlingFunction != null)
                    {
                        callback.Data = ServerSerializationHelper.SerializeObject(serverBase.ErrorHandlingFunction(ex, serviceType, method, client));
                    }
                    else
                        callback.Data = ServerSerializationHelper.SerializeObject(ex.ToString(), serverBase);
                }
                finally
                {
                    //serverBase.TaskOfClientInfoes.TryRemove(Task.CurrentId.GetValueOrDefault(), out string clientId);
                    serverBase.RemoveTask(taskId);
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
                return new CallMethodResultInfo<OperationContext>(callback, streamInfo, httpKeyAttributes, serviceType, method, service, fileActionResult, context, result);
            });
        }

        /// <summary>
        /// deserialize parameter data from a json value
        /// </summary>
        /// <param name="methodParameter"></param>
        /// <param name="userParameterInfo"></param>
        /// <param name="parameterIndex"></param>
        /// <param name="customDataExchanger"></param>
        /// <param name="allMethods"></param>
        /// <param name="serverBase"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        private static object DeserializeParameterValue(System.Reflection.ParameterInfo methodParameter, SignalGo.Shared.Models.ParameterInfo userParameterInfo, int parameterIndex, List<CustomDataExchangerAttribute> customDataExchanger, IEnumerable<MethodInfo> allMethods, ServerBase serverBase, ClientInfo client)
        {
            List<CustomDataExchangerAttribute> parameterDataExchanger = customDataExchanger.ToList();
            parameterDataExchanger.AddRange(GetMethodParameterBinds(parameterIndex, allMethods.ToArray()).Where(x => x.GetExchangerByUserCustomization(client)));
            if (userParameterInfo.Value == null)
                return GetDefaultValueOfParameter(methodParameter);
            //fix for deserialize from json
            if (SerializeHelper.GetTypeCodeOfObject(methodParameter.ParameterType) != SerializeObjectType.Object && !userParameterInfo.Value.StartsWith("\""))
                userParameterInfo.Value = "\"" + userParameterInfo.Value + "\"";

            return ServerSerializationHelper.Deserialize(userParameterInfo.Value, methodParameter.ParameterType, serverBase, customDataExchanger: parameterDataExchanger.ToArray(), client: client);
        }

        /// <summary>
        /// get default value of method parameter
        /// </summary>
        /// <param name="methodParameter"></param>
        /// <returns></returns>
        private static object GetDefaultValueOfParameter(System.Reflection.ParameterInfo methodParameter)
        {
            if (!methodParameter.HasDefaultValue)
                return DataExchangeConverter.GetDefault(methodParameter.ParameterType);
#if (!NETSTANDARD1_6)
            if (Convert.IsDBNull(methodParameter.DefaultValue))
                return DataExchangeConverter.GetDefault(methodParameter.ParameterType);
            else
#endif
                return methodParameter.DefaultValue;
        }
        /// <summary>
        /// fix mistake parameters to call server method
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="service"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <param name="serverBase"></param>
        /// <param name="client"></param>
        /// <param name="allMethods"></param>
        /// <param name="customDataExchanger"></param>
        /// <param name="validationErrors"></param>
        /// <returns></returns>
        private static List<object> FixParametersCount(int taskId, object service, MethodInfo method, List<SignalGo.Shared.Models.ParameterInfo> parameters, ServerBase serverBase, ClientInfo client, IEnumerable<MethodInfo> allMethods, List<CustomDataExchangerAttribute> customDataExchanger, string jsonParameters, out List<BaseValidationRuleInfoAttribute> validationErrors, ref string keyParameterValue)
        {

            List<object> parametersValues = new List<object>();
            System.Reflection.ParameterInfo[] methodParameters = method.GetParameters();
            Dictionary<string, object> parametersKeyValues = new Dictionary<string, object>();
            if (OperationContextBase.SavedKeyParametersNameSettings.Count > 0 && string.IsNullOrEmpty(keyParameterValue))
            {
                Shared.Models.ParameterInfo findKeyParameter = parameters.FirstOrDefault(x => x.Name != null && OperationContextBase.SavedKeyParametersNameSettings.ContainsKey(x.Name.ToLower()));
                if (findKeyParameter != null)
                {
                    parameters.Remove(findKeyParameter);
                    keyParameterValue = findKeyParameter.Value.TrimStart('"').TrimEnd('"');
                }
            }

            if (!string.IsNullOrEmpty(jsonParameters) && methodParameters.Length == 1 && (parameters.Count == 0 || parameters.Any(x => !methodParameters.Any(y => x.Name.Equals(y.Name, StringComparison.OrdinalIgnoreCase)))))
            {
                parameters.Clear();
                parameters.Add(new Shared.Models.ParameterInfo() { Value = jsonParameters });
                return FixParametersCount(taskId, service, method, parameters, serverBase, client, allMethods, customDataExchanger, null, out validationErrors, ref keyParameterValue);
            }

            //validation of methods
            foreach (BaseValidationRuleInfoAttribute item in method.GetCustomAttributes<BaseValidationRuleInfoAttribute>(true))
            {
                item.Initialize(service, method, parametersKeyValues, null, null, null, null);
                serverBase.ValidationRuleInfoManager.AddRule(taskId, item);
            }

            //for stop finding noname to make faster
            bool hasNoName = true;

            for (int i = 0; i < methodParameters.Length; i++)
            {
                //method parameter of server service
                System.Reflection.ParameterInfo methodParameter = methodParameters[i];
                string methodParameterName = methodParameter.Name.ToLower();
                //parameter came from client side
                Shared.Models.ParameterInfo userParameterInfo = parameters.FirstOrDefault(x => x.Name != null && x.Name.ToLower() == methodParameterName);
                object value = null;
                if (userParameterInfo != null)
                {
                    value = DeserializeParameterValue(methodParameter, userParameterInfo, i, customDataExchanger, allMethods, serverBase, client);
                    parametersValues.Add(value);
                    parametersKeyValues[methodParameterName] = value;
                }
                else if (hasNoName)
                {
                    Shared.Models.ParameterInfo findNoNameParameter = parameters.FirstOrDefault(x => string.IsNullOrEmpty(x.Name));
                    if (findNoNameParameter != null)
                    {
                        parameters.Remove(findNoNameParameter);
                        value = DeserializeParameterValue(methodParameter, findNoNameParameter, i, customDataExchanger, allMethods, serverBase, client);
                        if (value == null)
                            parametersValues.Add(findNoNameParameter.Value.Trim('"'));
                        else
                            parametersValues.Add(value);
                        parametersKeyValues[methodParameterName] = value;
                    }
                    else if (!string.IsNullOrEmpty(jsonParameters))
                    {
                        hasNoName = false;
                        value = DeserializeParameterValue(methodParameter, new Shared.Models.ParameterInfo() { Value = jsonParameters }, i, customDataExchanger, allMethods, serverBase, client);
                        parametersValues.Add(value);
                        parametersKeyValues[methodParameterName] = value;
                        jsonParameters = null;
                    }
                    else
                    {
                        hasNoName = false;
                        value = GetDefaultValueOfParameter(methodParameter);
                        parametersValues.Add(value);
                    }
                }
                else
                {
                    value = GetDefaultValueOfParameter(methodParameter);
                    parametersValues.Add(value);
                }
                //validation of parameters
                foreach (BaseValidationRuleInfoAttribute item in methodParameter.GetCustomAttributes<BaseValidationRuleInfoAttribute>(true))
                {
                    item.Initialize(service, method, parametersKeyValues, null, methodParameter, null, value);
                    serverBase.ValidationRuleInfoManager.AddRule(taskId, item);
                }
                if (SerializeHelper.GetTypeCodeOfObject(methodParameter.ParameterType) == SerializeObjectType.Object)
                    serverBase.ValidationRuleInfoManager.AddObjectPropertyAsChecked(taskId, methodParameter.ParameterType, value, null, null, null);
            }
            //get list of validation errors of calls
            validationErrors = serverBase.ValidationRuleInfoManager.CalculateValidationsOfTask((parameterName, newValue) =>
            {
                parameterName = parameterName.ToLower();
                //change property value, get value from validation and change it to property
                if (parametersKeyValues.ContainsKey(parameterName))
                    parametersKeyValues[parameterName] = newValue;
            }, (validation) =>
            {
                //initialize validations service method and parameters
                validation.Initialize(service, method, parametersKeyValues, null, null, null, null);
            }).ToList();
            return parametersValues;
        }

        private static async Task<object> GetInstanceOfService(ClientInfo client, string serviceName, Type serviceType, ServerBase serverBase)
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
                try
                {
                    await client.LockWaitToRead.WaitAsync();
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
                finally
                {
                    client.LockWaitToRead.Release();
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
                    if (item != serviceType)
                    {
                        MethodInfo newMethod = FindMethod(serviceType, methodName, parameters, canTakeMethod);
                        if (newMethod != null)
                        {
                            securityContractAttributes.AddRange(newMethod.GetCustomAttributes(typeof(SecurityContractAttribute), true).Cast<SecurityContractAttribute>());
                            customDataExchangerAttributes.AddRange(newMethod.GetCustomAttributes(typeof(CustomDataExchangerAttribute), true).Cast<CustomDataExchangerAttribute>().Where(x => x.GetExchangerByUserCustomization(client)));
                            clientLimitationAttributes.AddRange(newMethod.GetCustomAttributes(typeof(ClientLimitationAttribute), true).Cast<ClientLimitationAttribute>());
                            concurrentLockAttributes.AddRange(newMethod.GetCustomAttributes(typeof(ConcurrentLockAttribute), true).Cast<ConcurrentLockAttribute>());
                        }
                    }
                    yield return method;
                }
            }
        }

        private static bool IsTask(MethodInfo methodInfo)
        {
            return methodInfo.ReturnType == typeof(Task) || methodInfo.ReturnType.GetBaseType() == typeof(Task);
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

        internal static ConcurrentDictionary<string, MethodInfo> CachedMethods { get; set; } = new ConcurrentDictionary<string, MethodInfo>();

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
                int fakeCount = method.GetCustomAttributes<FakeParameterAttribute>().Count();

                System.Reflection.ParameterInfo[] param = method.GetParameters();
                bool hasError = false;
                if (parameters != null)
                {
                    foreach (Shared.Models.ParameterInfo p in parameters)
                    {
                        if (!string.IsNullOrEmpty(p.Name) && !param.Any(x => x.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase) && param.IndexOf(x) == parameters.IndexOf(p)))
                        {
                            if (fakeCount > 0 && parameters.LastOrDefault() == p)
                                break;
                            else
                            {
                                hasError = true;
                                break;
                            }
                        }
                    }
#if (!NET35 && !NET40)
                    if (!hasError && param.Length != parameters.Length)
                    {
                        foreach (System.Reflection.ParameterInfo p in param)
                        {
                            if (!parameters.Any(x => x.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
                            {
                                if (fakeCount > 0 && param.LastOrDefault() == p)
                                    break;
                                else if (!p.HasDefaultValue)
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
            string name = serviceType.FullName + methodName;
            if (parameters != null)
            {
                foreach (Shared.Models.ParameterInfo item in parameters)
                {
                    name += " " + item.Name + " ";
                }
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
                MethodCallbackInfo result = await callback;
#endif
                await SendCallbackData(result, client, serverBase);
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
        internal static Task SendCallbackData(MethodCallbackInfo callback, ClientInfo client, ServerBase serverBase)
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

            return client.StreamHelper.WriteToStreamAsync(client.ClientStream, data.ToArray());
        }
    }
}
