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

namespace SignalGo.Client.ClientManager
{
    /// <summary>
    /// base client connect to server helper
    /// </summary>
    public abstract class ConnectorBase : IDisposable
    {
        public ConnectorBase()
        {
            JsonSettingHelper.Initialize();
        }

        internal ISignalGoStream StreamHelper { get; set; } = null;
        internal JsonSettingHelper JsonSettingHelper { get; set; } = new JsonSettingHelper();
        internal AutoLogger AutoLogger { get; set; } = new AutoLogger() { FileName = "ConnectorBase Logs.log" };
        //internal ConcurrentList<AutoResetEvent> HoldMethodsToReconnect = new ConcurrentList<AutoResetEvent>();
        internal ConcurrentList<Delegate> PriorityActionsAfterConnected = new ConcurrentList<Delegate>();
        /// <summary>
        /// is WebSocket data provider
        /// </summary>
        public bool IsWebSocket { get; internal set; }
        /// <summary>
        /// client session id from server
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsPriorityEnabled { get; set; } = true;

        /// <summary>
        /// connector is disposed
        /// </summary>
        public bool IsDisposed { get; internal set; }

        internal string ServerUrl { get; set; }
        public virtual void Connect(string url, bool isWebsocket = false)
        {

        }

        private bool _IsConnected = false;
        /// <summary>
        /// if provider is connected
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return _IsConnected;
            }
            internal set
            {
                _IsConnected = value;
            }
        }
        /// <summary>
        /// is signalgo system on reconnecting to server in while
        /// </summary>
        public bool IsAutoReconnecting { get; internal set; }
        /// <summary>
        /// after client connect or disconnected call this action
        /// bool value is IsConnected property value
        /// </summary>
        public Action<ConnectionStatus> OnConnectionChanged { get; set; }
        /// <summary>
        /// when provider is reconnecting
        /// </summary>
        public Action OnAutoReconnecting { get; set; }
        /// <summary>
        /// settings of connector
        /// </summary>
        public ProviderSetting ProviderSetting { get; set; } = new ProviderSetting();
        /// <summary>
        /// client tcp
        /// </summary>
        internal TcpClient _client;
        internal PipeNetworkStream _clientStream;
        /// <summary>
        /// registred callbacks
        /// </summary>
        internal ConcurrentDictionary<string, KeyValue<SynchronizationContext, object>> Callbacks { get; set; } = new ConcurrentDictionary<string, KeyValue<SynchronizationContext, object>>();
        internal ConcurrentDictionary<string, object> Services { get; set; } = new ConcurrentDictionary<string, object>();
        internal AutoResetEvent AutoReconnectDelayResetEvent { get; set; } = new AutoResetEvent(true);
        internal AutoResetEvent HoldAllPrioritiesResetEvent { get; set; } = new AutoResetEvent(true);

        internal SecuritySettingsInfo SecuritySettings { get; set; } = null;

        internal string _address = "";
        internal int _port = 0;
        private readonly object _connectLock = new object();
        /// <summary>
        /// connect to server
        /// </summary>
        /// <param name="address">server address</param>
        /// <param name="port">server port</param>
        internal void Connect(string address, int port)
        {
            lock (_connectLock)
            {
                if (IsConnected)
                    throw new Exception("client is connected!");
                if (IsDisposed)
                    throw new ObjectDisposedException("Connector");
                if (IsWebSocket)
                    StreamHelper = SignalGoStreamWebSocket.CurrentWebSocket;
                else
                    StreamHelper = SignalGoStreamBase.CurrentBase;
                _ManulyDisconnected = false;
                _address = address;
                _port = port;
#if (NET45)
                _client = new TcpClient(address, port);
#elif (NETSTANDARD)
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
                _client = new TcpClient();
                _client.NoDelay = true;
                IAsyncResult result = _client.BeginConnect(address, port, null, null);

                bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5));

                if (!success)
                {
                    throw new Exception("Failed to connect.");
                }

                // we have connected
                _client.EndConnect(result);
#endif
                _clientStream = new PipeNetworkStream(new NormalStream(_client.GetStream()));
            }
        }

        /// <summary>
        /// This registers service on server and methods that the client can call
        /// T type must inherited OprationCalls interface
        /// T type must not be an interface
        /// </summary>
        /// <typeparam name="T">type of class for call server methods</typeparam>
        /// <returns>return instance class for call methods</returns>
        public T RegisterServerService<T>()
        {
            if (IsDisposed)
                throw new ObjectDisposedException("Connector");
            Type type = typeof(T);
            string name = type.GetServerServiceName(true);
            object objectInstance = Activator.CreateInstance(type);
            OperationCalls duplex = objectInstance as OperationCalls;
            if (duplex != null)
                duplex.Connector = this;
            Services.TryAdd(name, objectInstance);
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
            return GetType().GetMethod("GetDefaultGeneric").MakeGenericMethod(t).Invoke(this, null);
#else
            return this.GetType().FindMethod("GetDefaultGeneric").MakeGenericMethod(t).Invoke(this, null);
#endif
        }

        internal void RunPriorities()
        {
            if (!IsPriorityEnabled)
                return;
            foreach (Delegate item in PriorityActionsAfterConnected)
            {
                if (!IsPriorityEnabled)
                    break;
                if (item is Action)
                    ((Action)item)();
                else if (item is Func<PriorityAction>)
                {
                    PriorityAction priorityAction = PriorityAction.TryAgain;
                    do
                    {
#if (PORTABLE)
                        Task.Delay(ProviderSetting.PriorityFunctionDelayTime).Wait();
#else
                        Thread.Sleep(ProviderSetting.PriorityFunctionDelayTime);
#endif
                        priorityAction = ((Func<PriorityAction>)item)();
                        if (priorityAction == PriorityAction.BreakAll)
                            break;
                        else if (priorityAction == PriorityAction.HoldAll)
                        {
                            HoldAllPrioritiesResetEvent.Reset();
                            HoldAllPrioritiesResetEvent.WaitOne();
                        }
                    }
                    while (IsPriorityEnabled && priorityAction == PriorityAction.TryAgain);
                    if (priorityAction == PriorityAction.BreakAll)
                        break;
                }
            }
        }

        public void DisablePriority()
        {
            IsPriorityEnabled = false;
        }

        public void EnablePriority()
        {
            IsPriorityEnabled = true;
        }

        public void UnHoldPriority()
        {
            HoldAllPrioritiesResetEvent.Set();
        }
        /// <summary>
        /// get default value from type
        /// </summary>
        /// <returns></returns>
        internal T GetDefaultGeneric<T>()
        {
            return default(T);
        }

#if (!NETSTANDARD1_6 && !NETSTANDARD2_0 && !NETCOREAPP1_1 && !NET35 && !PORTABLE)
        /// <summary>
        /// register service and method to server for client call thats
        /// </summary>
        /// <typeparam name="T">type of interface for create instanse</typeparam>
        /// <returns>return instance of interface that client can call methods</returns>
        public T RegisterServerServiceInterface<T>() where T : class
        {
            if (IsDisposed)
                throw new ObjectDisposedException("Connector");
            Type type = typeof(T);
            string name = type.GetServerServiceName(true);

            MethodCallInfo callInfo = new MethodCallInfo()
            {
                ServiceName = name,
                MethodName = "/RegisterService",
                Guid = Guid.NewGuid().ToString()
            };
#if (NET40 || NET35)
            MethodCallbackInfo callback = this.SendData<MethodCallbackInfo>(callInfo);
#else
            MethodCallbackInfo callback = this.SendData<MethodCallbackInfo>(callInfo).GetAwaiter().GetResult();
#endif
            Type t = CSCodeInjection.GenerateInterfaceType(type, typeof(OperationCalls), new List<Type>() { typeof(ServiceContractAttribute), GetType() }, false);

            object objectInstance = Activator.CreateInstance(t);
            dynamic dobj = objectInstance;
            dobj.InvokedClientMethodAction = CSCodeInjection.InvokedClientMethodAction;
            dobj.InvokedClientMethodFunction = CSCodeInjection.InvokedClientMethodFunction;

            OperationCalls duplex = objectInstance as OperationCalls;
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
        public T RegisterServerServiceInterfaceWrapper<T>() where T : class
        {
            if (IsDisposed)
                throw new ObjectDisposedException("Connector");
            Type type = typeof(T);

            string name = type.GetServerServiceName(true);

            if (!ProviderSetting.AutoDetectRegisterServices)
            {
                MethodCallInfo callInfo = new MethodCallInfo()
                {
                    ServiceName = name,
                    MethodName = "/RegisterService",
                    Guid = Guid.NewGuid().ToString()
                };
#if (NET40 || NET35)
                MethodCallbackInfo callback = this.SendData<MethodCallbackInfo>(callInfo);
#else
                MethodCallbackInfo callback = this.SendData<MethodCallbackInfo>(callInfo).GetAwaiter().GetResult();
#endif
            }

            T objectInstance = InterfaceWrapper.Wrap<T>((serviceName, method, args) =>
            {
                if (typeof(void) == method.ReturnType)
                {
#if (NET40 || NET35)
                    ConnectorExtensions.SendData(this, serviceName, method.Name, method.MethodToParameters(x => ClientSerializationHelper.SerializeObject(x), args).ToArray());
#else
                    ConnectorExtensions.SendDataAsync(this, serviceName, method.Name, method.MethodToParameters(x => ClientSerializationHelper.SerializeObject(x), args).ToArray()).GetAwaiter().GetResult();
#endif
                    return null;
                }
                else
                {
                    MethodInfo getServiceMethod = type.FindMethod(method.Name);
                    List<CustomDataExchangerAttribute> customDataExchanger = getServiceMethod.GetCustomAttributes(typeof(CustomDataExchangerAttribute), true).Cast<CustomDataExchangerAttribute>().Where(x => x.GetExchangerByUserCustomization(this)).ToList();
#if (NET40 || NET35)
                    string data = ConnectorExtensions.SendData(this, serviceName, method.Name, method.MethodToParameters(x => ClientSerializationHelper.SerializeObject(x), args).ToArray());
#else
                    string data = ConnectorExtensions.SendDataAsync(this, serviceName, method.Name, method.MethodToParameters(x => ClientSerializationHelper.SerializeObject(x), args).ToArray()).GetAwaiter().GetResult();
#endif
                    if (data == null)
                        return null;
                    object result = ClientSerializationHelper.DeserializeObject(data.ToString(), method.ReturnType, customDataExchanger: customDataExchanger.ToArray());

                    return result;
                };
            }, (serviceName, method, args) =>
            {
                //this is async action
                if (method.ReturnType == typeof(Task))
                {
                    string methodName = method.Name;
                    if (methodName.EndsWith("Async"))
                        methodName = methodName.Substring(0, methodName.Length - 5);
                    return ConnectorExtensions.SendDataTaskAsync<object>(this, serviceName, methodName, method, method.MethodToParameters(x => ClientSerializationHelper.SerializeObject(x), args).ToArray());
                }
                //this is async function
                else if (method.ReturnType.GetBaseType() == typeof(Task))
                {
                    string methodName = method.Name;
                    if (methodName.ToLower().EndsWith("async"))
                        methodName = methodName.Substring(0, methodName.Length - 5);
                    // ConnectorExtension.SendDataAsync<object>()
                    MethodInfo findMethod = typeof(ConnectorExtensions).FindMethod("SendDataTaskAsync", BindingFlags.Static | BindingFlags.NonPublic);
                    Type methodType = method.ReturnType.GetListOfGenericArguments().FirstOrDefault();
                    MethodInfo madeMethod = findMethod.MakeGenericMethod(methodType);

                    return madeMethod.Invoke(this, new object[] { this, serviceName, methodName, method, args });
                }
                throw new NotSupportedException();
            });

            Services.TryAdd(name, objectInstance);
            return objectInstance;
        }

        private ConcurrentDictionary<string, object> InstancesOfRegisterStreamService = new ConcurrentDictionary<string, object>();
        /// <summary>
        /// register service and method to server for file or stream download and upload
        /// </summary>
        /// <typeparam name="T">type of interface for create instanse</typeparam>
        /// <returns>return instance of interface that client can call methods</returns>
        public T RegisterStreamServiceInterfaceWrapper<T>(string serverAddress = null, int? port = null) where T : class
        {
            if (IsDisposed)
                throw new ObjectDisposedException("Connector");
            Type type = typeof(T);
            ServiceContractAttribute serviceType = type.GetServerServiceAttribute();
            if (serviceType.ServiceType != ServiceType.StreamService)
                throw new Exception("your service is not a stream service");
            if (string.IsNullOrEmpty(serverAddress))
                serverAddress = _address;

            if (port == null)
                port = _port;

            string callKey = typeof(T).FullName + serverAddress + ":" + port.Value;

            if (InstancesOfRegisterStreamService.TryGetValue(callKey, out object instance))
            {
                return (T)instance;
            }

            string name = type.GetServerServiceName(true);

            T objectInstance = InterfaceWrapper.Wrap<T>((serviceName, method, args) =>
            {
#if (NET40 || NET35)
                return UploadStreamAsync<object>(name, serverAddress, port, serviceName, method, args).Result;
#else
                return UploadStreamAsync<object>(name, serverAddress, port, serviceName, method, args).GetAwaiter().GetResult();
#endif
            }, (serviceName, method, args) =>
            {
                if (method.ReturnType == typeof(Task))
                {
                    return UploadStreamAsync<object>(name, serverAddress, port, serviceName, method, args);
                }
                //this is async function
                else if (method.ReturnType.GetBaseType() == typeof(Task))
                {
                    //return UploadStreamAsync(name, serverAddress, port, serviceName, method, args);
                    //ConnectorExtension.SendDataAsync<object>()
                    MethodInfo findMethod = typeof(ConnectorBase).FindMethod("UploadStreamAsync");
                    Type methodType = method.ReturnType.GetListOfGenericArguments().FirstOrDefault();
                    MethodInfo madeMethod = findMethod.MakeGenericMethod(methodType);
                    return madeMethod.Invoke(this, new object[] { name, serverAddress, port, serviceName, method, args });
                }
                throw new NotSupportedException();
            });
            InstancesOfRegisterStreamService.TryAdd(callKey, objectInstance);
            return objectInstance;
        }

        //private Task<T> UploadStreamAsync<T>(string name, string serverAddress, int? port, string serviceName, MethodInfo method, object[] args)
        //{
        //    return Task<T>.Factory.StartNew(async () =>
        //    {
        //        return (T)await UploadStream(name, serverAddress, port, serviceName, method, args, true);
        //    });
        //}

#if (NET40 || NET35)
        public Task<T> UploadStreamAsync<T>(string name, string serverAddress, int? port, string serviceName, MethodInfo method, object[] args)
#else
        public async Task<T> UploadStreamAsync<T>(string name, string serverAddress, int? port, string serviceName, MethodInfo method, object[] args)
#endif
        {
            if (string.IsNullOrEmpty(serverAddress))
                serverAddress = _address;

            if (port == null || port.Value == 0)
                port = _port;
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var _newClient = new TcpClient();
            bool isSuccess = _newClient.ConnectAsync(serverAddress, port.Value).Wait(new TimeSpan(0, 0, 10));
            if (!isSuccess)
                throw new TimeoutException();
#elif (PORTABLE)
            var _newClient = new Sockets.Plugin.TcpSocketClient();
            bool isSuccess = _newClient.ConnectAsync(serverAddress, port.Value).Wait(new TimeSpan(0, 0, 10));
            if (!isSuccess)
                throw new TimeoutException();
#else
            TcpClient _newClient = new TcpClient(serverAddress, port.Value);
            _newClient.NoDelay = true;
#endif
            PipeNetworkStream stream = new PipeNetworkStream(new NormalStream(_newClient.GetStream()));
            //var json = JsonConvert.SerializeObject(Data);
            //var jsonBytes = Encoding.UTF8.GetBytes(json);
            string header = "SignalGo-Stream/4.0\r\n";
            byte[] bytes = Encoding.UTF8.GetBytes(header);
#if (NET40 || NET35)
            stream.Write(bytes, 0, bytes.Length);
#else
            await stream.WriteAsync(bytes, 0, bytes.Length);
#endif
            bool isUpload = false;
            if (method.GetParameters().Any(x => x.ParameterType == typeof(StreamInfo) || (x.ParameterType.GetIsGenericType() && x.ParameterType.GetGenericTypeDefinition() == typeof(StreamInfo<>))))
            {
                isUpload = true;
#if (NET40 || NET35)
                stream.Write(new byte[] { 0 }, 0, 1);
#else
                await stream.WriteAsync(new byte[] { 0 }, 0, 1);
#endif
            }
            else
            {
#if (NET40 || NET35)
                stream.Write(new byte[] { 1 }, 0, 1);
#else
                await stream.WriteAsync(new byte[] { 1 }, 0, 1);
#endif
            }
            MethodCallInfo callInfo = new MethodCallInfo();
            callInfo.ServiceName = name;
            IStreamInfo iStream = null;
            foreach (object item in args)
            {
                if (item is IStreamInfo value)
                {
                    iStream = value;
                    iStream.ClientId = ClientId;
                }
            }
            callInfo.Parameters = method.MethodToParameters(x => ClientSerializationHelper.SerializeObject(x), args).ToArray();
            string methodName = method.Name;
            if (methodName.EndsWith("Async"))
                methodName = methodName.Substring(0, methodName.Length - 5);
            callInfo.MethodName = methodName;

            string json = ClientSerializationHelper.SerializeObject(callInfo);

            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
#if (NET40 || NET35)
            StreamHelper.WriteBlockToStream(stream, jsonBytes);
#else
            await StreamHelper.WriteBlockToStreamAsync(stream, jsonBytes);
#endif
            CompressMode compressMode = CompressMode.None;
            if (isUpload)
            {
                KeyValue<DataType, CompressMode> firstData = null;
#if (NET40 || NET35)
                iStream.GetPositionFlush = () =>
#else
                iStream.GetPositionFlush = async () =>
#endif
                {
                    if (firstData != null && firstData.Key != DataType.FlushStream)
                        return -1;
#if (NET40 || NET35)
                    firstData = iStream.ReadFirstData(stream, ProviderSetting.MaximumReceiveStreamHeaderBlock);
#else
                    firstData = await iStream.ReadFirstDataAsync(stream, ProviderSetting.MaximumReceiveStreamHeaderBlock);
#endif
                    if (firstData.Key == DataType.FlushStream)
                    {
#if (NET40 || NET35)
                        byte[] data = StreamHelper.ReadBlockToEnd(stream, firstData.Value, ProviderSetting.MaximumReceiveStreamHeaderBlock);
#else
                        byte[] data = await StreamHelper.ReadBlockToEndAsync(stream, firstData.Value, ProviderSetting.MaximumReceiveStreamHeaderBlock);
#endif
                        return BitConverter.ToInt32(data, 0);
                    }
                    return -1;
                };
                if (iStream.WriteManually != null)
                    iStream.WriteManually(stream);
                else
                {
                    long length = iStream.Length;
                    long position = 0;
                    int blockOfRead = 1024 * 10;
                    while (length != position)
                    {

                        if (position + blockOfRead > length)
                            blockOfRead = (int)(length - position);
#if (NET40 || NET35)
                        int readCount = iStream.Stream.Read(bytes, blockOfRead);
#else
                        int readCount = await iStream.Stream.ReadAsync(bytes, blockOfRead);
#endif
                        position += readCount;
                        byte[] data = bytes.Take(readCount).ToArray();
#if (NET40 || NET35)
                        stream.Write(data, 0, data.Length);
#else
                        await stream.WriteAsync(data, 0, data.Length);
#endif
                    }
                }
                if (firstData == null || firstData.Key == DataType.FlushStream)
                {
                    while (true)
                    {
#if (NET40 || NET35)
                        firstData = iStream.ReadFirstData(stream, ProviderSetting.MaximumReceiveStreamHeaderBlock);
#else
                        firstData = await iStream.ReadFirstDataAsync(stream, ProviderSetting.MaximumReceiveStreamHeaderBlock);
#endif
                        if (firstData.Key == DataType.FlushStream)
                        {
#if (NET40 || NET35)
                            byte[] data = StreamHelper.ReadBlockToEnd(stream, firstData.Value, ProviderSetting.MaximumReceiveStreamHeaderBlock);
#else
                            byte[] data = await StreamHelper.ReadBlockToEndAsync(stream, firstData.Value, ProviderSetting.MaximumReceiveStreamHeaderBlock);
#endif
                        }
                        else
                            break;
                    }
                }
            }
            else
            {
#if (NET40 || NET35)
                byte dataTypeByte = StreamHelper.ReadOneByte(stream);
                byte compressModeByte = StreamHelper.ReadOneByte(stream);
#else
                byte dataTypeByte = await StreamHelper.ReadOneByteAsync(stream);
                byte compressModeByte = await StreamHelper.ReadOneByteAsync(stream);
#endif
            }
#if (NET40 || NET35)
            byte[] callBackBytes = StreamHelper.ReadBlockToEnd(stream, compressMode, ProviderSetting.MaximumReceiveStreamHeaderBlock);
#else
            byte[] callBackBytes = await StreamHelper.ReadBlockToEndAsync(stream, compressMode, ProviderSetting.MaximumReceiveStreamHeaderBlock);
#endif
            MethodCallbackInfo callbackInfo = ClientSerializationHelper.DeserializeObject<MethodCallbackInfo>(Encoding.UTF8.GetString(callBackBytes, 0, callBackBytes.Length));
            if (callbackInfo.IsException)
                throw new Exception(callbackInfo.Data);
            Type methodType = method.ReturnType;
            if (methodType.GetBaseType() == typeof(Task))
                methodType = method.ReturnType.GetListOfGenericArguments().FirstOrDefault();

            object result = ClientSerializationHelper.DeserializeObject(callbackInfo.Data, methodType);
            if (!isUpload)
            {
                result.GetType().GetPropertyInfo("Stream").SetValue(result, stream, null);
                result.GetType().GetPropertyInfo("GetStreamAction"
#if (!PORTABLE)
                        , BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
#endif
                        ).SetValue(result, new Action(() =>
                        {
                            //stream.Write(new byte[] { 0 }, 0, 1);
                            result.GetType().GetPropertyInfo("GetStreamAction"
#if (!PORTABLE)
                        , BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
#endif
                            ).SetValue(result, null, null);
                        }), null);
            }

#if (NET40 || NET35)
            return Task.Factory.StartNew(() => (T)result);
#else
            return (T)result;
#endif
        }
        public static T SendOneWayMethod<T>(string serverAddress, int port, string serviceName, string methodName, params Shared.Models.ParameterInfo[] parameters)
        {
#if (NET40 || NET35)
            return SendOneWayMethodAsync<T>(serverAddress, port, serviceName, methodName, parameters).Result;
#else
            return SendOneWayMethodAsync<T>(serverAddress, port, serviceName, methodName, parameters).GetAwaiter().GetResult();
#endif
        }

#if (NET40 || NET35)
        public static Task<T> SendOneWayMethodAsync<T>(string serverAddress, int port, string serviceName, string methodName, params Shared.Models.ParameterInfo[] parameters)
#else
        public static async Task<T> SendOneWayMethodAsync<T>(string serverAddress, int port, string serviceName, string methodName, params Shared.Models.ParameterInfo[] parameters)
#endif
        {
            //if (string.IsNullOrEmpty(serverAddress))
            //    serverAddress = _address;

            //if (port == null || port.Value == 0)
            //    port = _port;

#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var _newClient = new TcpClient();
            bool isSuccess = _newClient.ConnectAsync(serverAddress, port).Wait(new TimeSpan(0, 0, 10));
            if (!isSuccess)
                throw new TimeoutException();
#elif (PORTABLE)
            var _newClient = new Sockets.Plugin.TcpSocketClient();
            bool isSuccess = _newClient.ConnectAsync(serverAddress, port).Wait(new TimeSpan(0, 0, 10));
            if (!isSuccess)
                throw new TimeoutException();
#else
            TcpClient _newClient = new TcpClient(serverAddress, port);
            _newClient.NoDelay = true;
#endif
            PipeNetworkStream stream = new PipeNetworkStream(new NormalStream(_newClient.GetStream()));
            MethodCallInfo callInfo = new MethodCallInfo
            {
                ServiceName = serviceName,
                MethodName = methodName
            };
            callInfo.Parameters = parameters;

            if (methodName.EndsWith("Async"))
                methodName = methodName.Substring(0, methodName.Length - 5);

            string json = ClientSerializationHelper.SerializeObject(callInfo);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

            string line = "SignalGo-OneWay/4.0" + "\r\n";
            byte[] lineBytes = Encoding.UTF8.GetBytes(line);
            SignalGoStreamBase streamHelper = new SignalGoStreamBase();
#if (NET40 || NET35)
            streamHelper.WriteToStream(stream, lineBytes);
            streamHelper.WriteBlockToStream(stream, jsonBytes);
#else
            await streamHelper.WriteToStreamAsync(stream, lineBytes);
            await streamHelper.WriteBlockToStreamAsync(stream, jsonBytes);
#endif
#if (NET40 || NET35)
            DataType dataType = (DataType)stream.ReadOneByte();
            CompressMode compressMode = (CompressMode)stream.ReadOneByte();
            byte[] readData = streamHelper.ReadBlockToEnd(stream, compressMode, int.MaxValue);
#else
            DataType dataType = (DataType)await stream.ReadOneByteAcync();
            CompressMode compressMode = (CompressMode)await stream.ReadOneByteAcync();
            byte[] readData = await streamHelper.ReadBlockToEndAsync(stream, compressMode, int.MaxValue);
#endif
            json = Encoding.UTF8.GetString(readData, 0, readData.Length);
            MethodCallbackInfo callBack = ClientSerializationHelper.DeserializeObject<MethodCallbackInfo>(json);
            if (callBack.IsException)
                throw new Exception(callBack.Data);
#if (NET40 || NET35)
            return Task.Factory.StartNew(() => ClientSerializationHelper.DeserializeObject<T>(callBack.Data));
#else
            return ClientSerializationHelper.DeserializeObject<T>(callBack.Data);
#endif
        }

        //public void RegisterServerServiceInterface(string serviceName)
        //{
        //    if (IsDisposed)
        //        throw new ObjectDisposedException("Connector");
        //    MethodCallInfo callInfo = new MethodCallInfo()
        //    {
        //        ServiceName = serviceName,
        //        MethodName = "/RegisterService",
        //        Guid = Guid.NewGuid().ToString()
        //    };
        //    var callback = this.SendData<MethodCallbackInfo>(callInfo);
        //}
#if (!NETSTANDARD1_6 && !NETSTANDARD2_0 && !NETCOREAPP1_1 && !NET35 && !PORTABLE)
        /// <summary>
        /// register a callback interface and get dynamic calls
        /// not work on ios
        /// using ImpromptuInterface.Impromptu library
        /// </summary>
        /// <typeparam name="T">interface to instance</typeparam>
        /// <returns>return interface type to call methods</returns>
        public T RegisterServerServiceDynamicInterface<T>() where T : class
        {
            if (IsDisposed)
                throw new ObjectDisposedException("Connector");
            Type type = typeof(T);
            string name = type.GetServerServiceName(true);
            MethodCallInfo callInfo = new MethodCallInfo()
            {
                ServiceName = name,
                MethodName = "/RegisterService",
                Guid = Guid.NewGuid().ToString()
            };
#if (NET40 || NET35)
            MethodCallbackInfo callback = this.SendData<MethodCallbackInfo>(callInfo);
#else
            MethodCallbackInfo callback = this.SendData<MethodCallbackInfo>(callInfo).GetAwaiter().GetResult();
#endif
            DynamicServiceObject obj = new DynamicServiceObject()
            {
                Connector = this,
                ServiceName = name
            };
            obj.InitializeInterface(type);
            Services.TryAdd(name, obj);
            return ImpromptuInterface.Impromptu.ActLike<T>(obj);
        }
#endif
#if (!NET35)
        /// <summary>
        /// register a server service interface and get dynamic calls
        /// works for all platform like windows ,android ,ios and ...
        /// </summary>
        /// <typeparam name="T">interface type for use dynamic call</typeparam>
        /// <returns>return dynamic type to call methods</returns>
        public dynamic RegisterServerServiceDynamic<T>() where T : class
        {
            if (IsDisposed)
                throw new ObjectDisposedException("Connector");
            Type type = typeof(T);
            string name = type.GetServerServiceName(true);
            MethodCallInfo callInfo = new MethodCallInfo()
            {
                ServiceName = name,
                MethodName = "/RegisterService",
                Guid = Guid.NewGuid().ToString()
            };
#if (NET40 || NET35)
            MethodCallbackInfo callback = this.SendData<MethodCallbackInfo>(callInfo);
#else
            MethodCallbackInfo callback = this.SendData<MethodCallbackInfo>(callInfo).GetAwaiter().GetResult();
#endif
            DynamicServiceObject obj = new DynamicServiceObject()
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
        public dynamic RegisterServerServiceDynamic(string serviceName)
        {
            if (IsDisposed)
                throw new ObjectDisposedException("Connector");
            MethodCallInfo callInfo = new MethodCallInfo()
            {
                ServiceName = serviceName,
                MethodName = "/RegisterService",
                Guid = Guid.NewGuid().ToString()
            };
#if (NET40 || NET35)
            MethodCallbackInfo callback = this.SendData<MethodCallbackInfo>(callInfo);
#else
            MethodCallbackInfo callback = this.SendData<MethodCallbackInfo>(callInfo).GetAwaiter().GetResult();
#endif
            DynamicServiceObjectWitoutInterface obj = new DynamicServiceObjectWitoutInterface()
            {
                Connector = this,
                ServiceName = serviceName
            };
            Services.TryAdd(serviceName, obj);
            return obj;
        }
#endif
        /// <summary>
        /// register client service class, it's client methods that server call them
        /// </summary>
        /// <typeparam name="T">type of your class</typeparam>
        /// <returns>return instance if type</returns>
        public T RegisterClientService<T>()
        {
            if (IsDisposed)
                throw new ObjectDisposedException("Connector");
            Type type = typeof(T);
            string name = type.GetClientServiceName(true);

            object objectInstance = Activator.CreateInstance(type);
            //var duplex = objectInstance as ClientDuplex;
            //duplex.Connector = this;

            Callbacks.TryAdd(name, new KeyValue<SynchronizationContext, object>(SynchronizationContext.Current, objectInstance));
            OperationContract.SetConnector(objectInstance, this);
            return (T)objectInstance;
        }

        /// <summary>
        /// start client to reading stream and data from server
        /// </summary>
        /// <param name="client"></param>
#if (NET35 || NET40)
        internal void StartToReadingClientData()
#else
        internal async void StartToReadingClientData()

#endif
        {
#if (NET35 || NET40)
            Task.Factory.StartNew(() =>
#else
            await Task.Run(async () =>
#endif
            {
                try
                {

                    while (true)
                    {
                        //first byte is DataType
#if (NET40 || NET35)
                        int dataTypeByte = _clientStream.ReadOneByte();
#else
                        int dataTypeByte = await _clientStream.ReadOneByteAcync();
#endif
                        DataType dataType = (DataType)dataTypeByte;
                        if (dataType == DataType.PingPong)
                        {
                            PingAndWaitForPong.Set();
                            continue;
                        }
                        //secound byte is compress mode
#if (NET40 || NET35)
                        int compressModeByte = _clientStream.ReadOneByte();
#else
                        int compressModeByte = await _clientStream.ReadOneByteAcync();
#endif
                        CompressMode compresssMode = (CompressMode)compressModeByte;

                        // server is called client method
                        if (dataType == DataType.CallMethod)
                        {
#if (NET40 || NET35)
                            byte[] bytes = StreamHelper.ReadBlockToEnd(_clientStream, compresssMode, ProviderSetting.MaximumReceiveDataBlock);
#else
                            byte[] bytes = await StreamHelper.ReadBlockToEndAsync(_clientStream, compresssMode, ProviderSetting.MaximumReceiveDataBlock);
#endif
                            if (SecuritySettings != null)
                                bytes = DecryptBytes(bytes);
                            string json = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                            MethodCallInfo callInfo = ClientSerializationHelper.DeserializeObject<MethodCallInfo>(json);
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
#if (NET40 || NET35)
                            byte[] bytes = StreamHelper.ReadBlockToEnd(_clientStream, compresssMode, ProviderSetting.MaximumReceiveDataBlock);
#else
                            byte[] bytes = await StreamHelper.ReadBlockToEndAsync(_clientStream, compresssMode, ProviderSetting.MaximumReceiveDataBlock);
#endif
                            if (SecuritySettings != null)
                                bytes = DecryptBytes(bytes);
                            string json = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                            MethodCallbackInfo callback = ClientSerializationHelper.DeserializeObject<MethodCallbackInfo>(json);

                            bool geted = ConnectorExtensions.WaitedMethodsForResponse.TryGetValue(callback.Guid, out TaskCompletionSource<MethodCallbackInfo> keyValue);
                            if (geted)
                            {
                                keyValue.SetResult(callback);
                            }
                        }
                        else if (dataType == DataType.GetServiceDetails)
                        {
#if (NET40 || NET35)
                            byte[] bytes = StreamHelper.ReadBlockToEnd(_clientStream, compresssMode, ProviderSetting.MaximumReceiveDataBlock);
#else
                            byte[] bytes = await StreamHelper.ReadBlockToEndAsync(_clientStream, compresssMode, ProviderSetting.MaximumReceiveDataBlock);
#endif
                            if (SecuritySettings != null)
                                bytes = DecryptBytes(bytes);
                            string json = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                            getServiceDetialResult = ClientSerializationHelper.DeserializeObject<ProviderDetailsInfo>(json);
                            if (getServiceDetialResult == null)
                                getServiceDetialExceptionResult = ClientSerializationHelper.DeserializeObject<Exception>(json);
                            getServiceDetailEvent.Set();
                            getServiceDetailEvent.Reset();
                        }
                        else if (dataType == DataType.GetMethodParameterDetails)
                        {
#if (NET40 || NET35)
                            byte[] bytes = StreamHelper.ReadBlockToEnd(_clientStream, compresssMode, ProviderSetting.MaximumReceiveDataBlock);
#else
                            byte[] bytes = await StreamHelper.ReadBlockToEndAsync(_clientStream, compresssMode, ProviderSetting.MaximumReceiveDataBlock);
#endif
                            if (SecuritySettings != null)
                                bytes = DecryptBytes(bytes);
                            string json = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                            getmethodParameterDetailsResult = json;
                            getServiceDetailEvent.Set();
                            getServiceDetailEvent.Reset();
                        }
                        else if (dataType == DataType.GetClientId)
                        {
#if (NET40 || NET35)
                            byte[] bytes = StreamHelper.ReadBlockToEnd(_clientStream, compresssMode, ProviderSetting.MaximumReceiveDataBlock);
#else
                            byte[] bytes = await StreamHelper.ReadBlockToEndAsync(_clientStream, compresssMode, ProviderSetting.MaximumReceiveDataBlock);
#endif
                            if (SecuritySettings != null)
                                bytes = DecryptBytes(bytes);
                            ClientId = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                        }
                        else
                        {
                            //incorrect data! :|
                            AutoLogger.LogText("StartToReadingClientData Incorrect Data!");
                            Disconnect();
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    AutoLogger.LogError(ex, "StartToReadingClientData");
                    Disconnect();
                }
            });
        }

        private ManualResetEvent PingAndWaitForPong = new ManualResetEvent(true);
#if (NET40 || NET35)
        public bool SendPingAndWaitToReceive()
#else
        public async Task<bool> SendPingAndWaitToReceive()
#endif
        {
            try
            {
                PingAndWaitForPong.Reset();
#if (PORTABLE)
                var stream = _client.WriteStream;
#else
                NetworkStream stream = _client.GetStream();
#endif
#if (NET40 || NET35)
                StreamHelper.WriteToStream(_clientStream, new byte[] { (byte)DataType.PingPong });
#else
                await StreamHelper.WriteToStreamAsync(_clientStream, new byte[] { (byte)DataType.PingPong });
#endif
                return PingAndWaitForPong.WaitOne(new TimeSpan(0, 0, 3));
            }
            catch (Exception ex)
            {
                AutoLogger.LogError(ex, "ConnectorBase SendData");
                return false;
            }
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
        internal void SendData(MethodCallInfo data)
        {
            AsyncActions.Run(() =>
            {
                SendDataSync(data);
            });
        }

#if (NET40 || NET35)
        internal void SendDataSync(MethodCallInfo callback)
#else
        internal async void SendDataSync(MethodCallInfo callback)
#endif
        {
            try
            {
#if (PORTABLE)
                var stream = _client.WriteStream;
#else
                NetworkStream stream = _client.GetStream();
#endif
                string json = ClientSerializationHelper.SerializeObject(callback);
                List<byte> bytes = new List<byte>
                    {
                        (byte)DataType.CallMethod,
                        (byte)CompressMode.None
                    };
                byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
                if (SecuritySettings != null)
                    jsonBytes = EncryptBytes(jsonBytes);
                byte[] dataLen = BitConverter.GetBytes(jsonBytes.Length);
                bytes.AddRange(dataLen);
                bytes.AddRange(jsonBytes);
                if (bytes.Count > ProviderSetting.MaximumSendDataBlock)
                    throw new Exception("SendData data length is upper than MaximumSendDataBlock");
#if (NET40 || NET35)
                StreamHelper.WriteToStream(_clientStream, bytes.ToArray());
#else
                await StreamHelper.WriteToStreamAsync(_clientStream, bytes.ToArray());
#endif
            }
            catch (Exception ex)
            {
                if (ConnectorExtensions.WaitedMethodsForResponse.TryGetValue(callback.Guid, out TaskCompletionSource<MethodCallbackInfo> keyValue))
                {
                    MethodCallbackInfo data = new MethodCallbackInfo();
                    data.IsException = true;
                    data.Data = ex.Message;
                    keyValue.SetResult(data);
                }
                //AutoLogger.LogError(ex, "ConnectorBase SendData");
            }
        }

        internal abstract StreamInfo RegisterFileStreamToDownload(MethodCallInfo Data);
        internal abstract void RegisterFileStreamToUpload(StreamInfo streamInfo, MethodCallInfo Data);

        /// <summary>
        /// when server called a client methods this action will call
        /// </summary>
        public Action<MethodCallInfo> OnCalledMethodAction { get; set; }
        /// <summary>
        /// call a method of client from server
        /// </summary>
        /// <param name="callInfo">method call data</param>
#if (NET40 || NET35)
        internal void CallMethod(MethodCallInfo callInfo)
#else
        internal async void CallMethod(MethodCallInfo callInfo)
#endif
        {
            MethodCallbackInfo callback = new MethodCallbackInfo()
            {
                Guid = callInfo.Guid
            };
            try
            {
                OnCalledMethodAction?.Invoke(callInfo);
                object service = Callbacks[callInfo.ServiceName].Value;
                //#if (PORTABLE)
                MethodInfo method = service.GetType().FindMethod(callInfo.MethodName);
                //#else
                //                var method = service.GetType().GetMethod(callInfo.MethodName, RuntimeTypeHelper.GetMethodTypes(service.GetType(), callInfo).ToArray());
                //#endif
                if (method == null)
                    throw new Exception($"Method {callInfo.MethodName} from service {callInfo.ServiceName} not found! serviceType: {service.GetType().FullName}");
                List<object> parameters = new List<object>();
                int index = 0;
                foreach (System.Reflection.ParameterInfo item in method.GetParameters())
                {
                    parameters.Add(ClientSerializationHelper.DeserializeObject(callInfo.Parameters[index].Value, item.ParameterType));
                    index++;
                }
                if (method.ReturnType == typeof(void))
                    method.Invoke(service, parameters.ToArray());
                else
                {
                    object data = null;
                    //this is async action
                    if (method.ReturnType == typeof(Task))
                    {
#if (NET40 || NET35)
                        ((Task)method.Invoke(service, parameters.ToArray())).Wait();
#else
                        await (Task)method.Invoke(service, parameters.ToArray());
#endif
                    }
                    //this is async function
                    else if (method.ReturnType.GetBaseType() == typeof(Task))
                    {
#if (NET40 || NET35)
                        Task task = ((Task)method.Invoke(service, parameters.ToArray()));
                        task.Wait();
                        data = task.GetType().GetProperty("Result").GetValue(task, null);
#else
                        Task task = ((Task)method.Invoke(service, parameters.ToArray()));
                        await task;
                        data = task.GetType().GetProperty("Result").GetValue(task, null);
#endif

                    }
                    else
                    {
                        data = method.Invoke(service, parameters.ToArray());
                    }
                    callback.Data = data == null ? null : ClientSerializationHelper.SerializeObject(data);
                }
            }
            catch (Exception ex)
            {
                AutoLogger.LogError(ex, "ConnectorBase CallMethod");
                callback.IsException = true;
                callback.Data = ClientSerializationHelper.SerializeObject(ex.ToString());
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
#if (NET40 || NET35)
        internal void SendCallbackData(MethodCallbackInfo callback)
#else
        internal async void SendCallbackData(MethodCallbackInfo callback)
#endif
        {
            string json = ClientSerializationHelper.SerializeObject(callback);
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

#if (NET40 || NET35)
            StreamHelper.WriteToStream(_clientStream, data.ToArray());
#else
            await StreamHelper.WriteToStreamAsync(_clientStream, data.ToArray());
#endif
        }

        private ManualResetEvent getServiceDetailEvent = new ManualResetEvent(false);
        private ProviderDetailsInfo getServiceDetialResult = null;
        private Exception getServiceDetialExceptionResult = null;

#if (NET40 || NET35)
        public ProviderDetailsInfo GetListOfServicesWithDetials(string hostUrl)
#else
        public async Task<ProviderDetailsInfo> GetListOfServicesWithDetials(string hostUrl)
#endif
        {
            string json = ClientSerializationHelper.SerializeObject(hostUrl);
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

#if (NET40 || NET35)
            StreamHelper.WriteToStream(_clientStream, data.ToArray());
#else
            await StreamHelper.WriteToStreamAsync(_clientStream, data.ToArray());
#endif
            getServiceDetailEvent.WaitOne();
            if (getServiceDetialExceptionResult != null)
                throw getServiceDetialExceptionResult;
            return getServiceDetialResult;
        }

        private string getmethodParameterDetailsResult = "";
#if (NET40 || NET35)
        public string GetMethodParameterDetial(MethodParameterDetails methodParameterDetails)
#else
        public async Task<string> GetMethodParameterDetial(MethodParameterDetails methodParameterDetails)
#endif
        {
            if (IsDisposed)
                throw new ObjectDisposedException("Connector");

            string json = ClientSerializationHelper.SerializeObject(methodParameterDetails);
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

#if (NET40 || NET35)
            StreamHelper.WriteToStream(_clientStream, data.ToArray());
#else
            await StreamHelper.WriteToStreamAsync(_clientStream, data.ToArray());
#endif
            getServiceDetailEvent.WaitOne();
            return getmethodParameterDetailsResult;
        }

        /// <summary>
        /// calls this actions after connected and befor holded methods
        /// </summary>
        /// <param name="action"></param>
        public void AddPriorityAction(Action action)
        {
            PriorityActionsAfterConnected.Add(action);
        }

        /// <summary>
        /// calls this function after connected and befor holded methods
        /// if you return false this will hold methods and call this function again after a time until you return true
        /// </summary>
        /// <param name="function"></param>
        public void AddPriorityFunction(Func<PriorityAction> function)
        {
            PriorityActionsAfterConnected.Add(function);
        }

        private readonly object _autoReconnectLock = new object();
        private bool _ManulyDisconnected = false;
        public void Disconnect()
        {
            if (_ManulyDisconnected)
                return;
            _ManulyDisconnected = true;
            if (IsDisposed)
                throw new ObjectDisposedException("Connector");

            if (_client != null)
#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
                _client.Dispose();
#else
                _client.Close();
#endif
            foreach (KeyValuePair<string, TaskCompletionSource<MethodCallbackInfo>> item in ConnectorExtensions.WaitedMethodsForResponse)
            {
                item.Value.SetCanceled();
            }
            ConnectorExtensions.WaitedMethodsForResponse.Clear();
            if (IsConnected)
            {
                IsConnected = false;
            }
            OnConnectionChanged?.Invoke(ConnectionStatus.Disconnected);

            getServiceDetailEvent?.Reset();
            if (!IsAutoReconnecting)
            {
                lock (_autoReconnectLock)
                {
                    if (ProviderSetting.AutoReconnect && !IsAutoReconnecting)
                    {
                        IsAutoReconnecting = true;
                        while (!IsConnected && !IsDisposed)
                        {
                            try
                            {
                                OnConnectionChanged?.Invoke(ConnectionStatus.Reconnecting);
                                OnAutoReconnecting?.Invoke();
                                Connect(ServerUrl);
                            }
                            catch (Exception ex)
                            {

                            }
                            finally
                            {
                                AutoReconnectDelayResetEvent.Reset();
                                AutoReconnectDelayResetEvent.WaitOne(ProviderSetting.AutoReconnectTime);
                            }
                        }
                        //foreach (var item in HoldMethodsToReconnect.ToList())
                        //{
                        //    item.Set();
                        //}
                        //HoldMethodsToReconnect.Clear();
                        IsAutoReconnecting = false;
                    }
                }
            }
            else
            {

            }
        }
        

        /// <summary>
        /// close and dispose connector
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                throw new ObjectDisposedException("Connector");
            try
            {
                AutoLogger.LogText("Disposing Client");
                Disconnect();
                IsDisposed = true;
            }
            catch (Exception ex)
            {
                AutoLogger.LogError(ex, "ConnectorBase Dispose");
            }
        }
    }
}
