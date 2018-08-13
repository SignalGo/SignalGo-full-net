using SignalGo.Server.DataTypes;
using SignalGo.Server.Helpers;
using SignalGo.Server.Models;
using SignalGo.Shared.Converters;
using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Events;
using SignalGo.Shared.Helpers;
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
        static internal void CallMethod(MethodCallInfo callInfo, ClientInfo client, string json, ServerBase serverBase)
        {
#if (NET40 || NET35)
            Task.Factory.StartNew(() =>
#else
            Task.Run(() =>
#endif
            {
                OperationContext.CurrentTaskServer = serverBase;
                object result = null;
                MethodInfo method = null;
                Exception exception = null;
                MethodCallbackInfo callback = new MethodCallbackInfo()
                {
                    Guid = callInfo.Guid
                };

                try
                {
                    serverBase.TaskOfClientInfoes.TryAdd(Task.CurrentId.GetValueOrDefault(), client.ClientId);
                    
                    if (!serverBase.RegisteredServiceTypes.TryGetValue(callInfo.ServiceName, out Type serviceType))
                        throw new Exception($"{client.IPAddress} {client.ClientId} Service {callInfo.ServiceName} not found");

                    var service = GetInstanceOfService(client, callInfo.ServiceName, serviceType, serverBase);
                    if (service == null)
                        throw new Exception($"{client.IPAddress} {client.ClientId} service {callInfo.ServiceName} not found");

                    List<SecurityContractAttribute> securityAttributes = new List<SecurityContractAttribute>();
                    List<CustomDataExchangerAttribute> customDataExchanger = new List<CustomDataExchangerAttribute>();
                    List<ClientLimitationAttribute> clientLimitationAttribute = new List<ClientLimitationAttribute>();
                    List<ConcurrentLockAttribute> concurrentLockAttributes = new List<ConcurrentLockAttribute>();

                    var allMethods = GetMethods(client, callInfo, serviceType, customDataExchanger, securityAttributes, clientLimitationAttribute, concurrentLockAttributes).ToList();
                    method = allMethods.FirstOrDefault();
                    if (method == null)
                    {
                        StringBuilder exceptionResult = new StringBuilder();
                        exceptionResult.AppendLine("<Exception>");
                        exceptionResult.AppendLine($"method {callInfo.MethodName} not found");
                        exceptionResult.AppendLine("<Parameters>");
                        foreach (var item in callInfo.Parameters)
                        {
                            exceptionResult.AppendLine((item.Value ?? "null;") + " type: " + (item.Type ?? "no type"));
                        }
                        exceptionResult.AppendLine("</Parameters>");
                        exceptionResult.AppendLine("<JSON>");
                        exceptionResult.AppendLine(json);
                        exceptionResult.AppendLine("</JSON>");
                        exceptionResult.AppendLine("</Exception>");
                        throw new Exception($"{client.IPAddress} {client.ClientId} " + exceptionResult.ToString());
                    }

                    List<object> parameters = new List<object>();
                    int index = 0;
                    var prms = method.GetParameters();
                    foreach (var item in callInfo.Parameters)
                    {
                        if (item.Value == null)
                            parameters.Add(DataExchangeConverter.GetDefault(prms[index].ParameterType));
                        else
                        {
                            var parameterDataExchanger = customDataExchanger.ToList();
                            parameterDataExchanger.AddRange(GetMethodParameterBinds(index, allMethods.ToArray()).Where(x => x.GetExchangerByUserCustomization(client)));
                            parameters.Add(ServerSerializationHelper.Deserialize(item.Value, prms[index].ParameterType, serverBase, customDataExchanger: parameterDataExchanger.ToArray(), client: client));
                        }
                        index++;
                    }

                    foreach (ClientLimitationAttribute attrib in clientLimitationAttribute)
                    {
                        var allowAddresses = attrib.GetAllowAccessIpAddresses();
                        if (allowAddresses != null && allowAddresses.Length > 0)
                        {
                            if (!allowAddresses.Contains(client.IPAddress))
                            {
                                serverBase.AutoLogger.LogText($"Client IP Have Not Access To Call Method: {client.IPAddress}");
                                return;
                            }
                        }
                        else
                        {
                            var denyAddresses = attrib.GetDenyAccessIpAddresses();
                            if (denyAddresses != null && denyAddresses.Length > 0)
                            {
                                if (denyAddresses.Contains(client.IPAddress))
                                {
                                    serverBase.AutoLogger.LogText($"Client IP Is Deny Access To Call Method: {client.IPAddress}");
                                    return;
                                }
                            }
                        }
                    }
                    
                    //when method have static locl attribute calling is going to lock
                    var concurrentLockAttribute = concurrentLockAttributes.FirstOrDefault();
                    
                    MethodsCallHandler.BeginMethodCallAction?.Invoke(client, callInfo.Guid, callInfo.ServiceName, method, callInfo.Parameters);

                    //check if client have permissions for call method
                    bool canCall = true;
                    foreach (SecurityContractAttribute attrib in securityAttributes)
                    {
                        if (!attrib.CheckPermission(client, service, method, parameters))
                        {
                            callback.IsAccessDenied = true;
                            canCall = false;
                            if (method.ReturnType != typeof(void))
                            {
                                object data = null;
                                data = attrib.GetValueWhenDenyPermission(client, service, method, parameters);
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
                                                result = method.Invoke(service, parameters.ToArray());
                                            }
                                            break;
                                        }
                                    case ConcurrentLockType.PerClient:
                                        {
                                            lock (client)
                                            {
                                                result = method.Invoke(service, parameters.ToArray());
                                            }
                                            break;
                                        }
                                    case ConcurrentLockType.PerIpAddress:
                                        {
                                            lock (client.IPAddress)
                                            {
                                                result = method.Invoke(service, parameters.ToArray());
                                            }
                                            break;
                                        }
                                    case ConcurrentLockType.PerMethod:
                                        {
                                            lock (method)
                                            {
                                                result = method.Invoke(service, parameters.ToArray());
                                            }
                                            break;
                                        }
                                }
                            }
                            else
                                result = method.Invoke(service, parameters.ToArray());
                            callback.Data = result == null ? null : ServerSerializationHelper.SerializeObject(result, serverBase, customDataExchanger: customDataExchanger.ToArray(), client: client);
                        }
                        catch (Exception ex)
                        {
                            if (serverBase.ErrorHandlingFunction != null)
                                result = serverBase.ErrorHandlingFunction(ex, serviceType, method);
                            exception = ex;
                            serverBase.AutoLogger.LogError(ex, $"{client.IPAddress} {client.ClientId} ServerBase CallMethod: {callInfo.MethodName}");
                            callback.IsException = true;
                            callback.Data = ServerSerializationHelper.SerializeObject(ex.ToString(), serverBase);
                        }
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                    serverBase.AutoLogger.LogError(ex, $"{client.IPAddress} {client.ClientId} ServerBase CallMethod 2: {callInfo.MethodName}");
                    callback.IsException = true;
                    callback.Data = ServerSerializationHelper.SerializeObject(ex.ToString(), serverBase);
                }
                finally
                {
                    serverBase.TaskOfClientInfoes.TryRemove(Task.CurrentId.GetValueOrDefault(), out string clientId);

                    try
                    {
                        MethodsCallHandler.EndMethodCallAction?.Invoke(client, callInfo.Guid, callInfo.ServiceName, method, callInfo.Parameters, callback?.Data, exception);
                    }
                    catch (Exception ex)
                    {
                        serverBase.AutoLogger.LogError(ex, $"{client.IPAddress} {client.ClientId} ServerBase CallMethod 2: {callInfo.MethodName}");
                    }
                }
                SendCallbackData(callback, client, serverBase);
                DataExchanger.Clear();
            });
        }

        /// <summary>
        /// send result of calling method from client
        /// client is waiting for get response from server when calling method
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="client"></param>
        /// <param name="serverBase"></param>
        static void SendCallbackData(MethodCallbackInfo callback, ClientInfo client, ServerBase serverBase)
        {
            try
            {
                if (client.IsWebSocket)
                {
                    string json = ServerSerializationHelper.SerializeObject(callback, serverBase);
                    if (json.Length > 30000)
                    {
                        var listOfParts = GeneratePartsOfData(json);
                        int i = 1;
                        foreach (var item in listOfParts)
                        {
                            var cb = callback.Clone();
                            cb.PartNumber = i == listOfParts.Count ? (short)-1 : (short)i;
                            cb.Data = item;
                            json = (int)DataType.ResponseCallMethod + "," + (int)CompressMode.None + "/" + ServerSerializationHelper.SerializeObject(cb, serverBase);
                            var result = Encoding.UTF8.GetBytes(json);
                            //if (ClientsSettings.ContainsKey(client))
                            //    result = EncryptBytes(result, client);
                            GoStreamWriter.WriteToStream(client.ClientStream, result, client.IsWebSocket);
                            i++;
                        }
                    }
                    else
                    {
                        json = (int)DataType.ResponseCallMethod + "," + (int)CompressMode.None + "/" + json;
                        var result = Encoding.UTF8.GetBytes(json);
                        //if (ClientsSettings.ContainsKey(client))
                        //    result = EncryptBytes(result, client);
                        GoStreamWriter.WriteToStream(client.ClientStream, result, client.IsWebSocket);
                    }

                }
                else
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

                    GoStreamWriter.WriteToStream(client.ClientStream, data.ToArray(), client.IsWebSocket);
                }

            }
            catch (Exception ex)
            {
                serverBase.AutoLogger.LogError(ex, $"{client.IPAddress} {client.ClientId} ServerBase SendCallbackData");
                if (!client.TcpClient.Connected)
                    serverBase.DisposeClient(client, "SendCallbackData exception");
            }
            finally
            {
                //ClientConnectedCallingCount--;
            }
        }

        static List<string> GeneratePartsOfData(string data)
        {
            int partCount = (int)Math.Ceiling((double)data.Length / 30000);
            List<string> partData = new List<string>();
            for (int i = 0; i < partCount; i++)
            {
                if (i != partCount - 1)
                {
                    partData.Add(data.Substring((i * 30000), 30000));
                }
                else
                {
                    partData.Add(data.Substring((i * 30000), data.Length - (i * 30000)));
                }
            }
            return partData;
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
            , MethodCallInfo callInfo
            , Type serviceType
            , List<CustomDataExchangerAttribute> customDataExchangerAttributes
            , List<SecurityContractAttribute> securityContractAttributes
            , List<ClientLimitationAttribute> clientLimitationAttributes
            , List<ConcurrentLockAttribute> concurrentLockAttributes)
        {
            var list = serviceType.GetTypesByAttribute<ServiceContractAttribute>(x => true).ToList();
            foreach (var item in list)
            {
                var pTypes = RuntimeTypeHelper.GetMethodTypes(serviceType, callInfo).ToArray();
                var method = item.GetMethod(callInfo.MethodName, pTypes);
                if (method == null)
                    method = FindMethodFromBase(item, callInfo.MethodName, pTypes);
                if (method != null && method.IsPublic && !method.IsStatic)
                {
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
        static System.Reflection.MethodInfo FindMethodFromBase(Type serviceType, string methodName, Type[] pTypes)
        {
            foreach (var item in serviceType.GetInterfaces())
            {
                var m = item.GetMethod(methodName, pTypes);
                if (m != null)
                    return m;
            }
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var parent = serviceType.GetTypeInfo().BaseType;
#else
            var parent = serviceType.BaseType;
#endif
            while (parent != null)
            {
                var m = parent.GetMethod(methodName, pTypes);
                if (m != null)
                    return m;

                foreach (var item in parent.GetInterfaces())
                {
                    m = item.GetMethod(methodName, pTypes);
                    if (m != null)
                        return m;
                }
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                parent = parent.GetTypeInfo().BaseType;
#else
                parent = parent.BaseType;
#endif
            }

            return null;
        }
    }
}
