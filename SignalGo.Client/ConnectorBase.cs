using Newtonsoft.Json;
using SignalGo;
using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.IO;
using SignalGo.Shared.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
#if (!PORTABLE)
using System.Net.Sockets;
#endif
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SignalGo.Shared;
using SignalGo.Shared.Log;
using SignalGo.Shared.Security;

namespace SignalGo.Client
{
    /// <summary>
    /// connector extensions
    /// </summary>
    public static class ConnectorExtension
    {
        static ConnectorExtension()
        {
            CSCodeInjection.InvokedClientMethodAction = (client, method, parameters) =>
            {
                if (!(client is OperationCalls))
                {
                    AutoLogger.LogText($"cannot cast! {method.Name} params {parameters?.Length}");
                }
                SendDataInvoke((OperationCalls)client, method.Name, parameters);
            };

            CSCodeInjection.InvokedClientMethodFunction = (client, method, parameters) =>
            {
                var data = SendData((OperationCalls)client, method.Name, "", parameters);
                if (data == null)
                    return null;
                return data is StreamInfo ? data : JsonConvert.DeserializeObject(data.ToString(), method.ReturnType);
            };
        }

        /// <summary>
        /// call method wait for complete response from clients
        /// </summary>
        internal static ConcurrentDictionary<string, KeyValue<AutoResetEvent, MethodCallbackInfo>> WaitedMethodsForResponse { get; set; } = new ConcurrentDictionary<string, KeyValue<AutoResetEvent, MethodCallbackInfo>>();

        /// <summary>
        /// send data to client
        /// </summary>
        /// <typeparam name="T">return type data</typeparam>
        /// <param name="client">client for send data</param>
        /// <param name="callerName">method name</param>
        /// <param name="args">argumants of method</param>
        /// <returns></returns>
        internal static T SendData<T>(this OperationCalls client, string callerName, params object[] args)
        {
            var data = SendData(client, callerName, "", args);
            if (data == null || data.ToString() == "")
                return default(T);
            return JsonConvert.DeserializeObject<T>(data.ToString());
        }
        /// <summary>
        /// send data to connector
        /// </summary>
        /// <typeparam name="T">return type data</typeparam>
        /// <param name="connector">connetor for send data</param>
        /// <param name="callInfo">method for send</param>
        /// <returns></returns>
        internal static T SendData<T>(this ConnectorBase connector, MethodCallInfo callInfo)
        {
            var data = SendData(connector, callInfo, null);
            if (data == null || data.ToString() == "")
                return default(T);
            return JsonConvert.DeserializeObject<T>(data.ToString());
        }
        /// <summary>
        /// send data none return value
        /// </summary>
        /// <param name="client"></param>
        /// <param name="callerName"></param>
        /// <param name="args"></param>
        internal static void SendDataInvoke(this OperationCalls client, string callerName, params object[] args)
        {
            SendData(client, callerName, "", args);
        }

        /// <summary>
        /// send data not use params by array object
        /// </summary>
        /// <param name="client"></param>
        /// <param name="callerName"></param>
        /// <param name="attibName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static object SendDataNoParam(this OperationCalls client, string callerName, string attibName, object[] args)
        {
            return SendData(client, callerName, attibName, args);
        }

        /// <summary>
        /// send data to server
        /// </summary>
        /// <param name="client">client is sended</param>
        /// <param name="callerName">methos name</param>
        /// <param name="attibName">service name</param>
        /// <param name="args">method parameters</param>
        /// <returns></returns>
        internal static object SendData(this OperationCalls client, string callerName, string attibName, params object[] args)
        {
            string serviceName = "";
            if (string.IsNullOrEmpty(attibName))
                serviceName = client.GetType().GetCustomAttributes<ServiceContractAttribute>(true).FirstOrDefault().Name;
            else
                serviceName = attibName;

            return SendData(client.Connector, serviceName, callerName, args);
        }

        /// <summary>
        /// send data to server
        /// </summary>
        /// <returns></returns>
        internal static object SendData(ConnectorBase connector, string serviceName, string methodName, params object[] args)
        {
            MethodCallInfo callInfo = new MethodCallInfo();
            callInfo.ServiceName = serviceName;

            callInfo.MethodName = methodName;
            foreach (var item in args)
            {
                callInfo.Parameters.Add(new Shared.Models.ParameterInfo() { Value = JsonConvert.SerializeObject(item), Type = item?.GetType().FullName });
            }
            var guid = Guid.NewGuid().ToString();
            callInfo.Guid = guid;
            return SendData(connector, callInfo, args.Length == 1 && args[0] != null && args[0].GetType() == typeof(StreamInfo) ? (StreamInfo)args[0] : null);
        }

        static object SendData(this ConnectorBase connector, MethodCallInfo callInfo, StreamInfo streamInfo)
        {
            var added = WaitedMethodsForResponse.TryAdd(callInfo.Guid, new KeyValue<AutoResetEvent, MethodCallbackInfo>(new AutoResetEvent(false), null));
            var service = connector.Services.ContainsKey(callInfo.ServiceName) ? connector.Services[callInfo.ServiceName] : null;
#if (PORTABLE)
            var method = service?.GetType().FindMethod(callInfo.MethodName);
#else
            var method = service?.GetType().GetMethod(callInfo.MethodName, RuntimeTypeHelper.GetMethodTypes(service.GetType(), callInfo).ToArray());
#endif
            if (method != null && method.ReturnType == typeof(StreamInfo))
            {
                callInfo.Data = connector.SessionId;
                StreamInfo stream = connector.RegisterFileStreamToDownload(callInfo);
                return stream;
            }
            else if (method != null && streamInfo != null && method.ReturnType == typeof(void) && method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == typeof(StreamInfo))
            {
                callInfo.Data = connector.SessionId;
                connector.RegisterFileStreamToUpload(streamInfo, callInfo);
                return null;
            }
            else
                connector.SendData(callInfo);


            var seted = WaitedMethodsForResponse[callInfo.Guid].Key.WaitOne(connector.ProviderSetting.SendDataTimeout);
            if (!seted)
            {
                if (connector.SettingInfo != null && connector.SettingInfo.IsDisposeClientWhenTimeout)
                    connector.Dispose();
                throw new TimeoutException();
            }
            var result = WaitedMethodsForResponse[callInfo.Guid].Value;
            if (!result.IsException && callInfo.MethodName == "/RegisterService")
            {
                connector.SessionId = JsonConvert.DeserializeObject<string>(result.Data);
                result.Data = null;
            }
            WaitedMethodsForResponse.Remove(callInfo.Guid);
            if (result == null)
            {
                if (connector.IsDisposed)
                    throw new Exception("client disconnected");
                return null;
            }
            if (result.IsException)
                throw new Exception("server exception:" + JsonConvert.DeserializeObject<string>(result.Data));
            else if (result.IsAccessDenied && result.Data == null)
                throw new Exception("server permission denied exception.");

            return result.Data;
        }

        public static string SendRequest(this ConnectorBase connector, string serviceName, ServiceDetailsMethod serviceDetailMethod, out string json)
        {
            MethodCallInfo callInfo = new MethodCallInfo()
            {
                ServiceName = serviceName,
                MethodName = serviceDetailMethod.MethodName
            };
            foreach (var item in serviceDetailMethod.Parameters)
            {
                callInfo.Parameters.Add(new Shared.Models.ParameterInfo() { Value = item.Value.ToString(), Type = item.FullTypeName });
            }

            var guid = Guid.NewGuid().ToString();
            callInfo.Guid = guid;
            var added = WaitedMethodsForResponse.TryAdd(callInfo.Guid, new KeyValue<AutoResetEvent, MethodCallbackInfo>(new AutoResetEvent(false), null));
            //var service = connector.Services.ContainsKey(callInfo.ServiceName) ? connector.Services[callInfo.ServiceName] : null;
            //var method = service == null ? null : service.GetType().GetMethod(callInfo.MethodName, RuntimeTypeHelper.GetMethodTypes(service.GetType(), callInfo).ToArray());
            json = JsonConvert.SerializeObject(callInfo);
            connector.SendData(callInfo);


            var seted = WaitedMethodsForResponse[callInfo.Guid].Key.WaitOne(connector.ProviderSetting.SendDataTimeout);
            if (!seted)
            {
                if (connector.SettingInfo != null && connector.SettingInfo.IsDisposeClientWhenTimeout)
                    connector.Dispose();
                throw new TimeoutException();
            }
            var result = WaitedMethodsForResponse[callInfo.Guid].Value;
            if (callInfo.MethodName == "/RegisterService")
            {
                connector.SessionId = JsonConvert.DeserializeObject<string>(result.Data);
                result.Data = null;
            }
            WaitedMethodsForResponse.Remove(callInfo.Guid);
            if (result == null)
            {
                if (connector.IsDisposed)
                    throw new Exception("client disconnected");
                return "disposed";
            }
            if (result.IsException)
                throw new Exception("server exception:" + JsonConvert.DeserializeObject<string>(result.Data));
            else if (result.IsAccessDenied && result.Data == null)
                throw new Exception("server permission denied exception.");

            return result.Data;
        }
    }

    /// <summary>
    /// base client connect to server helper
    /// </summary>
    public abstract class ConnectorBase : IDisposable
    {
        /// <summary>
        /// is WebSocket data provider
        /// </summary>
        public bool IsWebSocket { get; internal set; }
        /// <summary>
        /// client session id from server
        /// </summary>
        public string SessionId { get; set; }
        /// <summary>
        /// connector is disposed
        /// </summary>
        public bool IsDisposed { get; internal set; }
        /// <summary>
        /// if provider is connected
        /// </summary>
        public bool IsConnected { get; set; }
        /// <summary>
        /// after client disconnected call this action
        /// </summary>
        public Action OnDisconnected { get; set; }
        /// <summary>
        /// settings of connector
        /// </summary>
        public ProviderSetting ProviderSetting { get; set; } = new ProviderSetting();
        /// <summary>
        /// client tcp
        /// </summary>
#if (PORTABLE)
        internal Sockets.Plugin.TcpSocketClient _client;
#else
        internal TcpClient _client;
#endif
        /// <summary>
        /// registred callbacks
        /// </summary>
        internal ConcurrentDictionary<string, KeyValue<SynchronizationContext, object>> Callbacks { get; set; } = new ConcurrentDictionary<string, KeyValue<SynchronizationContext, object>>();
        internal ConcurrentDictionary<string, object> Services { get; set; } = new ConcurrentDictionary<string, object>();

        internal SecuritySettingsInfo SecuritySettings { get; set; } = null;
        internal SettingInfo SettingInfo { get; set; } = null;

        internal string _address = "";
        internal int _port = 0;
        /// <summary>
        /// connect to server
        /// </summary>
        /// <param name="address">server address</param>
        /// <param name="port">server port</param>
#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
        internal void Connect(string address, int port)
#else
        internal void Connect(string address, int port)
#endif
        {
            _address = address;
            _port = port;
            IsDisposed = false;
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            _client = new TcpClient();
            bool isSuccess = _client.ConnectAsync(address, port).Wait(new TimeSpan(0, 0, 5));
            if (!isSuccess)
                throw new TimeoutException();
#elif (PORTABLE)
            _client = new Sockets.Plugin.TcpSocketClient();
            bool isSuccess = _client.ConnectAsync(address, port).Wait(new TimeSpan(0, 0, 5));
            if (!isSuccess)
                throw new TimeoutException();
#else
            _client = new TcpClient(address, port);
#endif

            IsConnected = true;

        }

        /// <summary>
        /// register service and method to server for client call thats
        /// T type must inherited OprationCalls interface
        /// T type must not be an interface
        /// </summary>
        /// <typeparam name="T">type of class for call server methods</typeparam>
        /// <returns>return instance class for call methods</returns>
        public T RegisterClientService<T>()
        {
            var type = typeof(T);
            MethodCallInfo callInfo = new MethodCallInfo()
            {
#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
                ServiceName = ((ServiceContractAttribute)type.GetTypeInfo().GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name,
#else
                ServiceName = ((ServiceContractAttribute)type.GetCustomAttributes(typeof(ServiceContractAttribute), true).FirstOrDefault()).Name,
#endif
                MethodName = "/RegisterService",
                Guid = Guid.NewGuid().ToString()
            };
            var callback = this.SendData<MethodCallbackInfo>(callInfo);
            var objectInstance = Activator.CreateInstance(type);
            var duplex = objectInstance as OperationCalls;
            duplex.Connector = this;
            Services.TryAdd(callInfo.ServiceName, objectInstance);
            return (T)objectInstance;
        }
        /// <summary>
        /// get default value from type
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        internal object GetDefault(Type t)
        {
#if (!PORTABLE)
            return this.GetType().GetMethod("GetDefaultGeneric").MakeGenericMethod(t).Invoke(this, null);
#else
            return this.GetType().FindMethod("GetDefaultGeneric").MakeGenericMethod(t).Invoke(this, null);
#endif
        }
        /// <summary>
        /// get default value from type
        /// </summary>
        /// <returns></returns>
        internal T GetDefaultGeneric<T>()
        {
            return default(T);
        }
#if (!NETSTANDARD1_6 && !NETCOREAPP1_1 && !NET35 && !PORTABLE)
        /// <summary>
        /// register service and method to server for client call thats
        /// </summary>
        /// <typeparam name="T">type of interface for create instanse</typeparam>
        /// <returns>return instance of interface that client can call methods</returns>
        public T RegisterClientServiceInterface<T>() where T : class
        {
            var type = typeof(T);
            var name = type.GetCustomAttributes<ServiceContractAttribute>(true).FirstOrDefault().Name;
            MethodCallInfo callInfo = new MethodCallInfo()
            {
                ServiceName = name,
                MethodName = "/RegisterService",
                Guid = Guid.NewGuid().ToString()
            };
            var callback = this.SendData<MethodCallbackInfo>(callInfo);

            var t = CSCodeInjection.GenerateInterfaceType(type, typeof(OperationCalls), new List<Type>() { typeof(ServiceContractAttribute), this.GetType() }, false);

            var objectInstance = Activator.CreateInstance(t);
            dynamic dobj = objectInstance;
            dobj.InvokedClientMethodAction = CSCodeInjection.InvokedClientMethodAction;
            dobj.InvokedClientMethodFunction = CSCodeInjection.InvokedClientMethodFunction;

            var duplex = objectInstance as OperationCalls;
            duplex.Connector = this;
            Services.TryAdd(name, objectInstance);
            return (T)objectInstance;
        }
#endif
        /// <summary>
        /// register service and method to server for client call thats
        /// </summary>
        /// <typeparam name="T">type of interface for create instanse</typeparam>
        /// <returns>return instance of interface that client can call methods</returns>
        public T RegisterClientServiceInterfaceWrapper<T>() where T : class
        {
            var type = typeof(T);
            var name = type.GetCustomAttributes<ServiceContractAttribute>(true).FirstOrDefault().Name;
            MethodCallInfo callInfo = new MethodCallInfo()
            {
                ServiceName = name,
                MethodName = "/RegisterService",
                Guid = Guid.NewGuid().ToString()
            };
            var callback = this.SendData<MethodCallbackInfo>(callInfo);

            var objectInstance = InterfaceWrapper.Wrap<T>((serviceName, method, args) =>
            {
                try
                {
                    if (typeof(void) == method.ReturnType)
                    {
                        ConnectorExtension.SendData(this, serviceName, method.Name, args);
                        return null;
                    }
                    else
                    {
                        var data = ConnectorExtension.SendData(this, serviceName, method.Name, args);
                        if (data == null)
                            return null;
                        if (data is StreamInfo)
                            return data;
                        else
                        {
                            var result = JsonConvert.DeserializeObject(data.ToString(), method.ReturnType, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Converters = new List<JsonConverter>() { new Shared.Converters.DataExchangeConverter(LimitExchangeType.Both) { Server = null, Client = this } }, Formatting = Formatting.None, NullValueHandling = NullValueHandling.Ignore });
                            return result;
                        }
                    };
                }
                catch (Exception ex)
                {
                    return null;
                }
            });

            Services.TryAdd(name, objectInstance);
            return (T)objectInstance;
        }

        /// <summary>
        /// register service and method to server for file or stream download and upload
        /// </summary>
        /// <typeparam name="T">type of interface for create instanse</typeparam>
        /// <returns>return instance of interface that client can call methods</returns>
        public T RegisterStreamServiceInterfaceWrapper<T>(string serverAddress = null, int? port = null) where T : class
        {
            if (string.IsNullOrEmpty(serverAddress))
                serverAddress = _address;
            if (port == null)
                port = _port;
            var type = typeof(T);
            var name = type.GetCustomAttributes<ServiceContractAttribute>(true).FirstOrDefault().Name;

            var objectInstance = InterfaceWrapper.Wrap<T>((serviceName, method, args) =>
            {
                try
                {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                    var _newClient = new TcpClient();
                    bool isSuccess = _newClient.ConnectAsync(serverAddress, port.Value).Wait(new TimeSpan(0, 0, 5));
                    if (!isSuccess)
                        throw new TimeoutException();
#elif (PORTABLE)
                    var _newClient = new Sockets.Plugin.TcpSocketClient();
                    bool isSuccess = _newClient.ConnectAsync(serverAddress, port.Value).Wait(new TimeSpan(0, 0, 5));
                    if (!isSuccess)
                        throw new TimeoutException();
#else
                    var _newClient = new TcpClient(serverAddress, port.Value);
#endif
#if (PORTABLE)
                    var stream = _newClient.WriteStream;
                    var readStream = _newClient.ReadStream;
#else
                    var stream = _newClient.GetStream();
                    var readStream = stream;
#endif
                    //var json = JsonConvert.SerializeObject(Data);
                    //var jsonBytes = Encoding.UTF8.GetBytes(json);
                    var header = "SignalGo-Stream/2.0\r\n";
                    var bytes = Encoding.UTF8.GetBytes(header);
                    stream.Write(bytes, 0, bytes.Length);
                    bool isUpload = false;
                    if (method.GetParameters().Any(x => x.ParameterType == typeof(StreamInfo) || (x.ParameterType.GetIsGenericType() && x.ParameterType.GetGenericTypeDefinition() == typeof(StreamInfo<>))))
                    {
                        isUpload = true;
                        stream.Write(new byte[] { 0 }, 0, 1);
                    }
                    else
                        stream.Write(new byte[] { 1 }, 0, 1);

                    MethodCallInfo callInfo = new MethodCallInfo();
                    callInfo.ServiceName = name;
                    IStreamInfo iStream = null;
                    foreach (var item in args)
                    {
                        callInfo.Parameters.Add(new Shared.Models.ParameterInfo() { Value = JsonConvert.SerializeObject(item) });
                        if (item is IStreamInfo)
                        {
                            iStream = (IStreamInfo)item;
                        }
                    }
                    callInfo.MethodName = method.Name;
                    var json = JsonConvert.SerializeObject(callInfo);

                    var jsonBytes = Encoding.UTF8.GetBytes(json);
                    GoStreamWriter.WriteBlockToStream(stream, jsonBytes);
                    if (isUpload)
                    {
                        long length = iStream.Length;
                        long position = 0;
                        int blockOfRead = 1024 * 10;
                        while (length != position)
                        {

                            if (position + blockOfRead > length)
                                blockOfRead = (int)(length - position);
                            bytes = new byte[blockOfRead];
                            var readCount = iStream.Stream.Read(bytes, 0, bytes.Length);
                            position += readCount;
                            stream.Write(bytes, 0, readCount);
                        }
                        var responseType = (DataType)GoStreamReader.ReadOneByte(readStream, CompressMode.None, ProviderSetting.MaximumReceiveStreamHeaderBlock, false);
                        var compressMode = (CompressMode)GoStreamReader.ReadOneByte(readStream, CompressMode.None, ProviderSetting.MaximumReceiveStreamHeaderBlock, false);
                    }


                    var callBackBytes = GoStreamReader.ReadBlockToEnd(readStream, CompressMode.None, ProviderSetting.MaximumReceiveStreamHeaderBlock, false);
                    var callbackInfo = JsonConvert.DeserializeObject<MethodCallbackInfo>(Encoding.UTF8.GetString(callBackBytes, 0, callBackBytes.Length));

                    var result = JsonConvert.DeserializeObject(callbackInfo.Data, method.ReturnType);
                    if (!isUpload)
                    {
                        result.GetType().GetPropertyInfo("Stream").SetValue(result, stream, null);
                        result.GetType().GetPropertyInfo("GetStreamAction"
#if (!PORTABLE)
                        , BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
#endif
                        ).SetValue(result, new Action(() =>
                        {
                            stream.Write(new byte[] { 0 }, 0, 1);
                            result.GetType().GetPropertyInfo("GetStreamAction"
#if (!PORTABLE)
                        , BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
#endif
                            ).SetValue(result, null, null);
                        }), null);
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    return null;
                }
            });

            return (T)objectInstance;
        }

        public void RegisterClientServiceInterface(string serviceName)
        {
            MethodCallInfo callInfo = new MethodCallInfo()
            {
                ServiceName = serviceName,
                MethodName = "/RegisterService",
                Guid = Guid.NewGuid().ToString()
            };
            var callback = this.SendData<MethodCallbackInfo>(callInfo);
        }
#if (!NETSTANDARD1_6 && !NETCOREAPP1_1 && !NET35 && !PORTABLE)
        /// <summary>
        /// register a callback interface and get dynamic calls
        /// not work on ios
        /// using ImpromptuInterface.Impromptu library
        /// </summary>
        /// <typeparam name="T">interface to instance</typeparam>
        /// <returns>return interface type to call methods</returns>
        public T RegisterClientServiceDynamicInterface<T>() where T : class
        {
            var type = typeof(T);
            var name = type.GetCustomAttributes<ServiceContractAttribute>(true).FirstOrDefault().Name;
            MethodCallInfo callInfo = new MethodCallInfo()
            {
                ServiceName = name,
                MethodName = "/RegisterService",
                Guid = Guid.NewGuid().ToString()
            };
            var callback = this.SendData<MethodCallbackInfo>(callInfo);

            var obj = new DynamicServiceObject()
            {
                Connector = this,
                ServiceName = name
            };
            obj.InitializeInterface(type);
            Services.TryAdd(name, obj);
            return (T)ImpromptuInterface.Impromptu.ActLike<T>(obj);
        }
#endif
#if (!NET35)
        /// <summary>
        /// register a callback interface and get dynamic calls
        /// works for all platform like windows ,android ,ios and ...
        /// </summary>
        /// <typeparam name="T">interface type for use dynamic call</typeparam>
        /// <returns>return dynamic type to call methods</returns>
        public dynamic RegisterClientServiceDynamic<T>() where T : class
        {
            var type = typeof(T);
            var name = type.GetCustomAttributes<ServiceContractAttribute>(true).FirstOrDefault().Name;
            MethodCallInfo callInfo = new MethodCallInfo()
            {
                ServiceName = name,
                MethodName = "/RegisterService",
                Guid = Guid.NewGuid().ToString()
            };
            var callback = this.SendData<MethodCallbackInfo>(callInfo);

            var obj = new DynamicServiceObject()
            {
                Connector = this,
                ServiceName = name
            };
            obj.InitializeInterface(type);
            Services.TryAdd(name, obj);
            return obj;
        }

        /// <summary>
        /// register a callback interface and get dynamic calls
        /// works for all platform like windows ,android ,ios and ...
        /// </summary>
        /// <typeparam name="T">interface type for use dynamic call</typeparam>
        /// <returns>return dynamic type to call methods</returns>
        public dynamic RegisterClientServiceDynamic(string serviceName)
        {
            MethodCallInfo callInfo = new MethodCallInfo()
            {
                ServiceName = serviceName,
                MethodName = "/RegisterService",
                Guid = Guid.NewGuid().ToString()
            };
            var callback = this.SendData<MethodCallbackInfo>(callInfo);

            var obj = new DynamicServiceObjectWitoutInterface()
            {
                Connector = this,
                ServiceName = serviceName
            };
            Services.TryAdd(serviceName, obj);
            return obj;
        }
#endif
        /// <summary>
        /// register server callback class, it's client methods wait for server call thats
        /// </summary>
        /// <typeparam name="T">type of your class</typeparam>
        /// <returns>return instance if type</returns>
        public T RegisterServerCallback<T>()
        {
            var type = typeof(T);
            var objectInstance = Activator.CreateInstance(type);
            //var duplex = objectInstance as ClientDuplex;
            //duplex.Connector = this;

            Callbacks.TryAdd(type.GetCustomAttributes<ServiceContractAttribute>(true).FirstOrDefault().Name, new KeyValue<SynchronizationContext, object>(SynchronizationContext.Current, objectInstance));
            OperationContract.SetConnector(objectInstance, this);
            return (T)objectInstance;
        }

        /// <summary>
        /// start client to reading stream and data from server
        /// </summary>
        /// <param name="client"></param>
        internal void StartToReadingClientData()
        {
            AsyncActions.Run(() =>
            {
                try
                {
#if (PORTABLE)
                    var stream = _client.ReadStream;
#else
                    var stream = _client.GetStream();
#endif
                    while (true)
                    {
                        //first byte is DataType
                        var dataType = (DataType)stream.ReadByte();
                        //secound byte is compress mode
                        var compresssMode = (CompressMode)stream.ReadByte();

                        // server is called client method
                        if (dataType == DataType.CallMethod)
                        {
                            var bytes = GoStreamReader.ReadBlockToEnd(stream, compresssMode, ProviderSetting.MaximumReceiveDataBlock, IsWebSocket);
                            if (SecuritySettings != null)
                                bytes = DecryptBytes(bytes);
                            var json = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                            MethodCallInfo callInfo = JsonConvert.DeserializeObject<MethodCallInfo>(json);
                            if (callInfo.Type == MethodType.User)
                                CallMethod(callInfo);
                            else if (callInfo.Type == MethodType.SignalGo)
                            {
                                if (callInfo.MethodName == "/MustReconnectUdpServer")
                                {
                                    ReconnectToUdp(callInfo);
                                }
                            }
                        }
                        //after client called server method, server response to client
                        else if (dataType == DataType.ResponseCallMethod)
                        {
                            var bytes = GoStreamReader.ReadBlockToEnd(stream, compresssMode, ProviderSetting.MaximumReceiveDataBlock, IsWebSocket);
                            if (SecuritySettings != null)
                                bytes = DecryptBytes(bytes);
                            var json = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                            MethodCallbackInfo callback = JsonConvert.DeserializeObject<MethodCallbackInfo>(json);

                            var geted = ConnectorExtension.WaitedMethodsForResponse.TryGetValue(callback.Guid, out KeyValue<AutoResetEvent, MethodCallbackInfo> keyValue);
                            if (geted)
                            {
                                keyValue.Value = callback;
                                keyValue.Key.Set();
                            }
                        }
                        else if (dataType == DataType.GetServiceDetails)
                        {
                            var bytes = GoStreamReader.ReadBlockToEnd(stream, compresssMode, ProviderSetting.MaximumReceiveDataBlock, IsWebSocket);
                            if (SecuritySettings != null)
                                bytes = DecryptBytes(bytes);
                            var json = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                            getServiceDetialResult = JsonConvert.DeserializeObject<ProviderDetailsInfo>(json);
                            if (getServiceDetialResult == null)
                                getServiceDetialExceptionResult = JsonConvert.DeserializeObject<Exception>(json);
                            getServiceDetailEvent.Set();
                            getServiceDetailEvent.Reset();
                        }
                        else if (dataType == DataType.GetMethodParameterDetails)
                        {
                            var bytes = GoStreamReader.ReadBlockToEnd(stream, compresssMode, ProviderSetting.MaximumReceiveDataBlock, IsWebSocket);
                            if (SecuritySettings != null)
                                bytes = DecryptBytes(bytes);
                            var json = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                            getmethodParameterDetailsResult = json;
                            getServiceDetailEvent.Set();
                            getServiceDetailEvent.Reset();
                        }
                        else
                        {
                            //incorrect data! :|
                            SignalGo.Shared.Log.AutoLogger.LogText("StartToReadingClientData Incorrect Data!");
                            Dispose();
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    SignalGo.Shared.Log.AutoLogger.LogError(ex, "StartToReadingClientData");
                    Dispose();
                }
            });
        }

        internal byte[] DecryptBytes(byte[] bytes)
        {
            return AESSecurity.DecryptBytes(bytes, SecuritySettings.Data.Key, SecuritySettings.Data.IV);
        }

        internal byte[] EncryptBytes(byte[] bytes)
        {
            return AESSecurity.EncryptBytes(bytes, SecuritySettings.Data.Key, SecuritySettings.Data.IV);
        }

        //public object SendData(this ClientDuplex client, string className, string callerName, params object[] args)
        //{
        //    MethodCallInfo callInfo = new MethodCallInfo();
        //    callInfo.FullClassName = className;
        //    callInfo.MethodName = callerName;
        //    foreach (var item in args)
        //    {
        //        callInfo.Parameters.Add(new Shared.Models.ParameterInfo() { Type = item.GetType().FullName, Value = JsonConvert.SerializeObject(item) });
        //    }
        //    SendData(callInfo);
        //    return null;
        //}

        /// <summary>
        /// send data to call server method
        /// </summary>
        /// <param name="Data"></param>
        internal void SendData(MethodCallInfo Data)
        {
            AsyncActions.Run(() =>
            {
                try
                {
#if (PORTABLE)
                    var stream = _client.WriteStream;
#else
                    var stream = _client.GetStream();
#endif
                    var json = JsonConvert.SerializeObject(Data);
                    List<byte> bytes = new List<byte>
                    {
                        (byte)DataType.CallMethod,
                        (byte)CompressMode.None
                    };
                    var jsonBytes = Encoding.UTF8.GetBytes(json);
                    if (SecuritySettings != null)
                        jsonBytes = EncryptBytes(jsonBytes);
                    byte[] dataLen = BitConverter.GetBytes(jsonBytes.Length);
                    bytes.AddRange(dataLen);
                    bytes.AddRange(jsonBytes);
                    if (bytes.Count > ProviderSetting.MaximumSendDataBlock)
                        throw new Exception("SendData data length is upper than MaximumSendDataBlock");
                    GoStreamWriter.WriteToStream(stream, bytes.ToArray(), IsWebSocket);
                }
                catch (Exception ex)
                {
                    AutoLogger.LogError(ex, "ConnectorBase SendData");
                }
            });
        }

        internal abstract StreamInfo RegisterFileStreamToDownload(MethodCallInfo Data);
        internal abstract void RegisterFileStreamToUpload(StreamInfo streamInfo, MethodCallInfo Data);

        /// <summary>
        /// call a method of client from server
        /// </summary>
        /// <param name="callInfo">method call data</param>
        internal void CallMethod(MethodCallInfo callInfo)
        {
            MethodCallbackInfo callback = new MethodCallbackInfo()
            {
                Guid = callInfo.Guid
            };
            try
            {
                var service = Callbacks[callInfo.ServiceName].Value;
#if (PORTABLE)
                var method = service.GetType().FindMethod(callInfo.MethodName);
#else
                var method = service.GetType().GetMethod(callInfo.MethodName, RuntimeTypeHelper.GetMethodTypes(service.GetType(), callInfo).ToArray());
#endif
                if (method == null)
                    throw new Exception($"Method {callInfo.MethodName} from service {callInfo.ServiceName} not found! serviceType: {service.GetType().FullName}");
                List<object> parameters = new List<object>();
                int index = 0;
                foreach (var item in method.GetParameters())
                {
                    parameters.Add(JsonConvert.DeserializeObject(callInfo.Parameters[index].Value, item.ParameterType));
                    index++;
                }
                if (method.ReturnType == typeof(void))
                    method.Invoke(service, parameters.ToArray());
                else
                {
                    var data = method.Invoke(service, parameters.ToArray());
                    callback.Data = data == null ? null : JsonConvert.SerializeObject(data);
                }
            }
            catch (Exception ex)
            {
                AutoLogger.LogError(ex, "ConnectorBase CallMethod");
                callback.IsException = true;
                callback.Data = JsonConvert.SerializeObject(ex.ToString());
            }
            SendCallbackData(callback);
        }

        /// <summary>
        /// reconnect to udp service it's call from server tcp service
        /// </summary>
        /// <param name="callInfo"></param>
        internal virtual void ReconnectToUdp(MethodCallInfo callInfo)
        {

        }

        /// <summary>
        /// after call method from server , client must send callback to server
        /// </summary>
        /// <param name="callback">method callback data</param>
        internal void SendCallbackData(MethodCallbackInfo callback)
        {
            string json = JsonConvert.SerializeObject(callback);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            if (SecuritySettings != null)
                bytes = EncryptBytes(bytes);
            byte[] len = BitConverter.GetBytes(bytes.Length);
            List<byte> data = new List<byte>
            {
                (byte)DataType.ResponseCallMethod,
                (byte)CompressMode.None
            };
            data.AddRange(len);
            data.AddRange(bytes);
            if (data.Count > ProviderSetting.MaximumSendDataBlock)
                throw new Exception("SendCallbackData data length is upper than MaximumSendDataBlock");

#if (PORTABLE)
            var stream = _client.WriteStream;
#else
            var stream = _client.GetStream();
#endif
            GoStreamWriter.WriteToStream(stream, data.ToArray(), IsWebSocket);
        }


        ManualResetEvent getServiceDetailEvent = new ManualResetEvent(false);
        ProviderDetailsInfo getServiceDetialResult = null;
        Exception getServiceDetialExceptionResult = null;

        public ProviderDetailsInfo GetListOfServicesWithDetials(string hostUrl)
        {
            AsyncActions.Run(() =>
            {
                string json = JsonConvert.SerializeObject(hostUrl);
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                if (SecuritySettings != null)
                    bytes = EncryptBytes(bytes);
                byte[] len = BitConverter.GetBytes(bytes.Length);
                List<byte> data = new List<byte>
                {
                    (byte)DataType.GetServiceDetails,
                    (byte)CompressMode.None
                };
                data.AddRange(len);
                data.AddRange(bytes);
                if (data.Count > ProviderSetting.MaximumSendDataBlock)
                    throw new Exception("SendCallbackData data length is upper than MaximumSendDataBlock");
#if (PORTABLE)
                var stream = _client.WriteStream;
#else
                var stream = _client.GetStream();
#endif
                GoStreamWriter.WriteToStream(stream, data.ToArray(), IsWebSocket);
            });
            getServiceDetailEvent.WaitOne();
            if (getServiceDetialExceptionResult != null)
                throw getServiceDetialExceptionResult;
            return getServiceDetialResult;
        }

        string getmethodParameterDetailsResult = "";
        public string GetMethodParameterDetial(MethodParameterDetails methodParameterDetails)
        {
            AsyncActions.Run(() =>
            {
                string json = JsonConvert.SerializeObject(methodParameterDetails);
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                if (SecuritySettings != null)
                    bytes = EncryptBytes(bytes);
                byte[] len = BitConverter.GetBytes(bytes.Length);
                List<byte> data = new List<byte>
                {
                    (byte)DataType.GetMethodParameterDetails,
                    (byte)CompressMode.None
                };
                data.AddRange(len);
                data.AddRange(bytes);
                if (data.Count > ProviderSetting.MaximumSendDataBlock)
                    throw new Exception("SendCallbackData data length is upper than MaximumSendDataBlock");
#if (PORTABLE)
                var stream = _client.WriteStream;
#else
                var stream = _client.GetStream();
#endif
                GoStreamWriter.WriteToStream(stream, data.ToArray(), IsWebSocket);
            });
            getServiceDetailEvent.WaitOne();
            return getmethodParameterDetailsResult;
        }


        /// <summary>
        /// close and dispose connector
        /// </summary>
        public void Dispose()
        {
            try
            {
                AutoLogger.LogText("Disposing Client");
                IsConnected = false;
                IsDisposed = true;
                foreach (var item in ConnectorExtension.WaitedMethodsForResponse)
                {
                    item.Value.Key.Set();
                }
                if (_client != null)
#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
                    _client.Dispose();
#else
                    _client.Close();
#endif
                OnDisconnected?.Invoke();
#if (NET35)
                getServiceDetailEvent?.Close();
#else
                getServiceDetailEvent?.Dispose();
#endif
            }
            catch (Exception ex)
            {
                AutoLogger.LogError(ex, "ConnectorBase Dispose");
            }
        }
    }
}
