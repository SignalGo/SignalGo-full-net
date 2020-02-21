using SignalGo.Server.Helpers;
using SignalGo.Server.Models;
using SignalGo.Shared.IO.Compressions;
using SignalGo.Shared.Models;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Server.ServiceManager.Providers
{
    public class OneWayServiceProvider : BaseProvider
    {
        public static async void StartToReadingClientData(ClientInfo client, ServerBase serverBase)
        {
            //#if (NET40 || NET35)
            //            Task.Factory.StartNew(() =>
            //#else
            //            Task.Run(() =>
            //#endif
            //            {
            try
            {
                //Console.WriteLine($"OneWay Client Connected: {client.IPAddress}");
                await RunMethod(serverBase, client);
                serverBase.DisposeClient(client, null, "OneWay StartToReadingClientData finished");
            }
            catch (Exception ex)
            {
                serverBase.AutoLogger.LogError(ex, $"{client.IPAddress} {client.ClientId} ServerBase OneWay StartToReadingClientData");
                serverBase.DisposeClient(client, null, "OneWay StartToReadingClientData exception");
            }
            //});
        }

        internal static async Task RunMethod(ServerBase serverBase, ClientInfo client)
        {
            MethodCallbackInfo callback = null;
            try
            {
                byte[] bytes = await client.StreamHelper.ReadBlockToEndAsync(client.ClientStream, CompressionHelper.GetCompression(serverBase.CurrentCompressionMode, serverBase.GetCustomCompression), serverBase.ProviderSetting.MaximumReceiveStreamHeaderBlock);
                string json = Encoding.UTF8.GetString(bytes);
                MethodCallInfo callInfo = ServerSerializationHelper.Deserialize<MethodCallInfo>(json, serverBase);
                //MethodsCallHandler.BeginStreamCallAction?.Invoke(client, guid, serviceName, methodName, values);
                CallMethodResultInfo<OperationContext> result = await CallMethod(callInfo.ServiceName, callInfo.Guid, callInfo.MethodName, callInfo.Parameters, null, client, null, serverBase, null, null);
                callback = result.CallbackInfo;
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
            await SendCallbackData(callback, client, serverBase);
        }
    }
}
