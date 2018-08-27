using SignalGo.Server.Helpers;
using SignalGo.Server.Models;
using SignalGo.Shared;
using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Http;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Server.ServiceManager.Providers
{
    public class OneWayServiceProvider : BaseProvider
    {
        public static void StartToReadingClientData(ClientInfo client, ServerBase serverBase)
        {
#if (NET40 || NET35)
            Task.Factory.StartNew(() =>
#else
            Task.Run(() =>
#endif
            {
                int taskId = Task.CurrentId.GetValueOrDefault();
                try
                {
                    serverBase.AddTask(taskId, client.ClientId);
                    Console.WriteLine($"OneWay Client Connected: {client.IPAddress}");
                    RunMethod(serverBase, client);
                    serverBase.DisposeClient(client, "OneWay StartToReadingClientData finished");
                }
                catch (Exception ex)
                {
                    serverBase.AutoLogger.LogError(ex, $"{client.IPAddress} {client.ClientId} ServerBase OneWay StartToReadingClientData");
                    serverBase.DisposeClient(client, "OneWay StartToReadingClientData exception");
                }
                finally
                {
                    serverBase.RemoveTask(taskId);
                }
            });
        }

        internal static void RunMethod(ServerBase serverBase, ClientInfo client)
        {
            MethodCallbackInfo callback = null;
            try
            {
                byte[] bytes = client.StreamHelper.ReadBlockToEnd(client.ClientStream, CompressMode.None, serverBase.ProviderSetting.MaximumReceiveStreamHeaderBlock);
                string json = Encoding.UTF8.GetString(bytes);
                MethodCallInfo callInfo = ServerSerializationHelper.Deserialize<MethodCallInfo>(json, serverBase);
                //MethodsCallHandler.BeginStreamCallAction?.Invoke(client, guid, serviceName, methodName, values);
                callback = CallMethod(callInfo.ServiceName, callInfo.Guid, callInfo.MethodName, callInfo.Parameters, client, null, serverBase, null, null, out IStreamInfo streamInfo, out List<HttpKeyAttribute> httpKeyAttributes, out Type serviceType, out MethodInfo method, out object service, out FileActionResult fileActionResult);

            }
            catch (Exception ex)
            {
                callback = new MethodCallbackInfo
                {
                    IsException = true,
                    Data = ServerSerializationHelper.SerializeObject(ex)
                };
            }
            finally
            {
                //MethodsCallHandler.EndStreamCallAction?.Invoke(client, guid, serviceName, methodName, values, jsonResult, exception);
            }
            SendCallbackData(callback, client, serverBase);
        }
    }
}
