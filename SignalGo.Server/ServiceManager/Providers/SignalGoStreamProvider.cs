using SignalGo.Server.Helpers;
using SignalGo.Server.Models;
using SignalGo.Shared.IO;
using SignalGo.Shared.IO.Compressions;
using SignalGo.Shared.Models;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Server.ServiceManager.Providers
{
    public class SignalGoStreamProvider : BaseProvider
    {
        public static async void StartToReadingClientData(ClientInfo client, ServerBase serverBase)
        {
            try
            {
                //Console.WriteLine($"Stream Client Connected: {client.IPAddress}");
                PipeNetworkStream stream = client.ClientStream;
                byte firstByte = await client.StreamHelper.ReadOneByteAsync(stream).ConfigureAwait(false);
                if (firstByte == 0)
                {
                    await DownloadStreamFromClient(client, serverBase).ConfigureAwait(false);
                }
                //download from server and upload from client
                else
                {
                    await UploadStreamToClient(client, serverBase).ConfigureAwait(false);
                }
                serverBase.DisposeClient(client, null, "StartToReadingClientData finished");
            }
            catch (Exception ex)
            {
                serverBase.AutoLogger.LogError(ex, $"{client.IPAddress} {client.ClientId} ServerBase SignalGoStreamProvider StartToReadingClientData");
                serverBase.DisposeClient(client, null, "SignalGoStreamProvider StartToReadingClientData exception");
            }
        }

        /// <summary>
        /// this method call when client want to upload file or stream to your server
        /// </summary>
        /// <param name="serverBase">client stream</param>
        /// <param name="client">client</param>
        private static async Task DownloadStreamFromClient(ClientInfo client, ServerBase serverBase)
        {
            MethodCallbackInfo callback = null;
            string guid = null;
            try
            {
                byte[] bytes = await client.StreamHelper.ReadBlockToEndAsync(client.ClientStream, CompressionHelper.GetCompression(CompressMode.None, serverBase.GetCustomCompression), serverBase.ProviderSetting.MaximumReceiveStreamHeaderBlock).ConfigureAwait(false);
                string json = Encoding.UTF8.GetString(bytes);
                MethodCallInfo callInfo = ServerSerializationHelper.Deserialize<MethodCallInfo>(json, serverBase);
                guid = callInfo.Guid;
                CallMethodResultInfo<OperationContext> result = await CallMethod(callInfo.ServiceName, callInfo.Guid, callInfo.MethodName, callInfo.MethodName,
                    callInfo.Parameters, null, client, null, serverBase, null, null).ConfigureAwait(false);
                callback = result.CallbackInfo;
            }
            catch (IOException ex)
            {
                callback = new MethodCallbackInfo();
                callback.IsException = true;
                callback.Data = ServerSerializationHelper.SerializeObject(ex);
                //return;
            }
            catch (Exception ex)
            {
                callback = new MethodCallbackInfo();
                callback.IsException = true;
                callback.Data = ServerSerializationHelper.SerializeObject(ex);
            }
            finally
            {
                callback.Guid = guid;
            }
            await SendCallbackData(callback, client, serverBase).ConfigureAwait(false);
        }

        /// <summary>
        /// this method calll when client want to download file or stream from your server
        /// </summary>
        /// <param name="stream">client stream</param>
        /// <param name="client">client</param>
        private static async Task UploadStreamToClient(ClientInfo client, ServerBase serverBase)
        {
            MethodCallbackInfo callback = null;
            IStreamInfo streamInfo = null;
            PipeNetworkStream userStream = null;
            PipeNetworkStream stream = client.ClientStream;
            bool isCallbackSended = false;
            try
            {
                byte[] bytes = await client.StreamHelper.ReadBlockToEndAsync(client.ClientStream, CompressionHelper.GetCompression(CompressMode.None, serverBase.GetCustomCompression), serverBase.ProviderSetting.MaximumReceiveStreamHeaderBlock).ConfigureAwait(false);
                string json = Encoding.UTF8.GetString(bytes);
                MethodCallInfo callInfo = ServerSerializationHelper.Deserialize<MethodCallInfo>(json, serverBase);
                CallMethodResultInfo<OperationContext> result = await CallMethod(callInfo.ServiceName, callInfo.Guid, callInfo.MethodName, callInfo.MethodName,
                    callInfo.Parameters, null, client, null, serverBase, null, null).ConfigureAwait(false);
                callback = result.CallbackInfo;
                streamInfo = result.StreamInfo;
                userStream = streamInfo.Stream;
                long len = streamInfo.Length.GetValueOrDefault();
                await SendCallbackData(callback, client, serverBase).ConfigureAwait(false);
                isCallbackSended = true;
                long writeLen = 0;
                while (writeLen < len)
                {
                    bytes = new byte[1024 * 100];
                    int readCount = await userStream.ReadAsync(bytes, bytes.Length).ConfigureAwait(false);
                    byte[] sendBytes = bytes.Take(readCount).ToArray();
                    await stream.WriteAsync(sendBytes, 0, sendBytes.Length).ConfigureAwait(false);
                    writeLen += readCount;
                }
                userStream.Dispose();
                //Console.WriteLine("user stream finished");
            }
            catch (Exception ex)
            {
                if (streamInfo != null)
                    streamInfo.Dispose();
                if (userStream != null)
                {
                    userStream.Dispose();
                    //Console.WriteLine("user stream disposed");
                }
                if (!isCallbackSended && !client.ClientStream.IsClosed)
                {
                    if (callback == null)
                        callback = new MethodCallbackInfo();
                    callback.IsException = true;
                    callback.Data = ServerSerializationHelper.SerializeObject(ex);
                    await SendCallbackData(callback, client, serverBase).ConfigureAwait(false);
                }
            }
            finally
            {
                //MethodsCallHandler.EndStreamCallAction?.Invoke(client, guid, serviceName, methodName, values, jsonResult, exception);
            }
        }
    }
}
