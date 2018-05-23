using SignalGo.Server.DataTypes;
using SignalGo.Server.Helpers;
using SignalGo.Server.Models;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.IO;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SignalGo.Server.ServiceManager
{
    public static class OneWayProvider
    {
        public static void RunMethod(ServerBase serverBase, Stream stream, ClientInfo client)
        {
            MethodCallbackInfo callback = new MethodCallbackInfo();
            string guid = Guid.NewGuid().ToString();
            Exception exception = null;
            string serviceName = null;
            string methodName = null;
            string jsonResult = null;
            List<SignalGo.Shared.Models.ParameterInfo> values = null;
            try
            {
                var bytes = GoStreamReader.ReadBlockToEnd(stream, CompressMode.None, serverBase.ProviderSetting.MaximumReceiveStreamHeaderBlock, false);
                var json = Encoding.UTF8.GetString(bytes);
                MethodCallInfo callInfo = ServerSerializationHelper.Deserialize<MethodCallInfo>(json, serverBase);
                serviceName = callInfo.ServiceName;
                methodName = callInfo.MethodName;
                values = callInfo.Parameters;
                var service = serverBase.FindOneWayServiceByName(serviceName);
                //MethodsCallHandler.BeginStreamCallAction?.Invoke(client, guid, serviceName, methodName, values);
                if (service == null)
                    serverBase.DisposeClient(client, "oneway RunMethod service not found!");
                else
                {
                    var clientLimitationAttribute = service.GetType().GetCustomAttributes<ClientLimitationAttribute>(true).ToList();

                    var serviceType = service.GetType();
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                    var method = serviceType.GetTypeInfo().GetMethod(methodName, RuntimeTypeHelper.GetMethodTypes(serviceType, callInfo).ToArray());
#else
                    var method = serviceType.GetMethod(methodName, RuntimeTypeHelper.GetMethodTypes(serviceType, callInfo).ToArray());
#endif
                    clientLimitationAttribute.AddRange(method.GetCustomAttributes<ClientLimitationAttribute>());

                    foreach (ClientLimitationAttribute attrib in clientLimitationAttribute)
                    {
                        var allowAddresses = attrib.GetAllowAccessIpAddresses();
                        if (allowAddresses != null && allowAddresses.Length > 0)
                        {
                            if (!allowAddresses.Contains(client.IPAddress))
                            {
                                string data = $"Client IP Have Not Access To Call Method: {client.IPAddress}";
                                serverBase.DisposeClient(client, data);
                                serverBase.AutoLogger.LogText(data);
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
                                    string data = $"Client IP Have Not Access To Call Method: {client.IPAddress}";
                                    serverBase.DisposeClient(client, data);
                                    serverBase.AutoLogger.LogText(data);
                                    return;
                                }
                            }
                        }
                    }

                    List<object> parameters = new List<object>();
                    int index = 0;
                    var prms = method.GetParameters();
                    foreach (var item in values)
                    {
                        parameters.Add(ServerSerializationHelper.Deserialize(item.Value, prms[index].ParameterType, serverBase));
                        index++;
                    }

                    if (method.ReturnType == typeof(void))
                    {
                        method.Invoke(service, parameters.ToArray());
                    }
                    else
                    {
                        var result = method.Invoke(service, parameters.ToArray());
                        jsonResult = callback.Data = ServerSerializationHelper.SerializeObject(result);
                    }
                }
            }
            catch (IOException ex)
            {
                exception = ex;
                serverBase.AutoLogger.LogError(ex, "oneway RunMethod error");
                serverBase.DisposeClient(client, "oneway RunMethod exception");
                return;
            }
            catch (Exception ex)
            {
                exception = ex;
                callback.IsException = true;
                callback.Data = ServerSerializationHelper.SerializeObject(ex);
            }
            finally
            {
                //MethodsCallHandler.EndStreamCallAction?.Invoke(client, guid, serviceName, methodName, values, jsonResult, exception);
            }
            serverBase.SendCallbackData(callback, client);
            if (callback.IsException)
            {
                serverBase.DisposeClient(client, "oneway RunMethod exception 2");
            }
        }
    }
}
