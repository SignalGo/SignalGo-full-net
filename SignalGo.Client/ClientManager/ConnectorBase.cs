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
using System.Net.Security;
using SignalGo.Client.IO;

namespace SignalGo.Client.ClientManager
{
    /// <summary>
    /// protocol of signalgo client to connect server
    /// if your server is hosted on iis use httpduplex
    /// </summary>
    public enum ClientProtocolType : byte
    {
        /// <summary>
        /// unknown
        /// </summary>
        None = 0,
        /// <summary>
        /// signalgo duplex client
        /// </summary>
        SignalGoDuplex = 1,
        /// <summary>
        /// signalgo http duplex
        /// </summary>
        HttpDuplex = 2,
        /// <summary>
        /// websocket
        /// </summary>
        WebSocket = 3
    }
    /// <summary>
    /// base client connect to server helper
    /// </summary>
    public abstract class ConnectorBase : IDisposable
    {
        static ConnectorBase()
        {
            //WebcoketDatagramBase.Current = new WebcoketIISDatagram();
            WebcoketDatagramBase.Current = new WebcoketDatagram();
        }

        public ConnectorBase()
        {
            JsonSettingHelper.Initialize();
        }

        /// <summary>
        /// call method wait for complete response from clients
        /// </summary>
        internal ConcurrentDictionary<string, TaskCompletionSource<MethodCallbackInfo>> WaitedMethodsForResponse { get; set; } = new ConcurrentDictionary<string, TaskCompletionSource<MethodCallbackInfo>>();
        /// <summary>
        /// protocol of signalgo client to connect server
        /// if your server is hosted on iis use httpduplex
        /// </summary>
        public ClientProtocolType ProtocolType { get; set; } = ClientProtocolType.SignalGoDuplex;
        /// <summary>
        /// when signalgo want use streaming protocol it will use http protocol to connect to server because iis is not support signalgo protocol
        /// </summary>
        public bool UseHttpStream { get; set; } = false;

        internal ISignalGoStream StreamHelper { get; set; } = null;
        internal JsonSettingHelper JsonSettingHelper { get; set; } = new JsonSettingHelper();
        internal AutoLogger AutoLogger { get; set; } = new AutoLogger() { FileName = "ConnectorBase Logs.log" };
        //internal ConcurrentList<AutoResetEvent> HoldMethodsToReconnect = new ConcurrentList<AutoResetEvent>();
        internal ConcurrentList<Delegate> PriorityActionsAfterConnected = new ConcurrentList<Delegate>();
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
        public virtual void Connect(string url)
        {

        }

#if (NET40 || NET35)
        public virtual void ConnectAsync(string url, bool isWebsocket = false)
#else
        public virtual Task ConnectAsync(string url)
#endif
        {
            throw new NotImplementedException();
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
        internal IClientWorker _client;
        internal PipeNetworkStream _clientStream;
        /// <summary>
        /// registred callbacks
        /// </summary>
        internal ConcurrentDictionary<string, KeyValue<SynchronizationContext, object>> Callbacks { get; set; } = new ConcurrentDictionary<string, KeyValue<SynchronizationContext, object>>();
        internal ConcurrentDictionary<string, object> Services { get; set; } = new ConcurrentDictionary<string, object>();

        internal AutoResetEvent HoldAllPrioritiesResetEvent { get; set; } = new AutoResetEvent(true);
        internal TaskCompletionSource<object> HoldAllPrioritiesTaskResult = new TaskCompletionSource<object>();
        internal TaskCompletionSource<object> AutoReconnectWaitToDisconnectTaskResult = new TaskCompletionSource<object>();
        internal TaskCompletionSource<ProviderDetailsInfo> ServiceDetailEventTaskResult = null;
        internal TaskCompletionSource<string> ServiceParameterDetailEventTaskResult = null;

        internal SecuritySettingsInfo SecuritySettings { get; set; } = null;

        internal string _address = "";
        internal int _port = 0;
        /// <summary>
        /// connect to server
        /// </summary>
        /// <param name="address">server address</param>
        /// <param name="port">server port</param>
#if (!NET35 && !NET40)
        internal async Task ConnectAsync(string address, int port)
        {
            if (IsConnected)
                throw new Exception("client is connected!");
            if (IsDisposed)
                throw new ObjectDisposedException("Connector");
            if (port == 443)
                ProviderSetting.ServerServiceSetting.IsHttps = true;
            _address = address;
            _port = port;
            StreamHelper = SignalGoStreamBase.CurrentBase;
            if (ProtocolType == ClientProtocolType.SignalGoDuplex)
            {
                _client = new TcpClientWorker(new TcpClient());
            }
            else
            {
                _client = new WebSocketClientWorker(new TcpClient());
            }

            await _client.ConnectAsync(address, port);
            if (ProviderSetting.ServerServiceSetting.IsHttps)
            {
#if (NETSTANDARD1_6)
                throw new Exception("not support ssl in net standard 1.6 yet.");
#else

                if (ProtocolType == ClientProtocolType.WebSocket)
                {
                    _clientStream = new PipeNetworkStream(new NormalStream(_client.GetStream()));
                    await ReadAllWebSocketResponseLinesAsync();
                    _clientStream = new PipeNetworkStream(new WebSocketStream(_client.GetStream()));
                }
                else
                {
                    SslStream sslStream = new SslStream(_client.GetStream());
                    await sslStream.AuthenticateAsClientAsync(address);
                    _clientStream = new PipeNetworkStream(new NormalStream(sslStream));
                }
#endif
            }
            else
            {
                _clientStream = new PipeNetworkStream(new NormalStream(_client.GetStream()));
                if (ProtocolType == ClientProtocolType.WebSocket)
                {
                    await ReadAllWebSocketResponseLinesAsync();
                    _clientStream = new PipeNetworkStream(new WebSocketStream(_client.GetStream()));
                }
            }
            if (ProtocolType == ClientProtocolType.WebSocket)
                _clientStream.BufferToRead = ushort.MaxValue;
        }
#endif

        private void ReadAllWebSocketResponseLines()
        {
            StringBuilder stringBuilder = new StringBuilder();
            while (true)
            {
                string line = _clientStream.ReadLine();
                stringBuilder.AppendLine(line);
                if (string.IsNullOrEmpty(line) || line == TextHelper.NewLine)
                    break;
            }
            var all = stringBuilder.ToString();
            if (all.Contains("IsSignalGoOverIIS"))
                WebcoketDatagramBase.Current = new WebcoketIISDatagram();
            if (!stringBuilder.ToString().Contains("101 Switching Protocols"))
                throw new Exception(stringBuilder.ToString());
        }

#if (!NET35 && !NET40)
        private async Task ReadAllWebSocketResponseLinesAsync()
        {
            StringBuilder stringBuilder = new StringBuilder();
            while (true)
            {
                string line = await _clientStream.ReadLineAsync();
                stringBuilder.AppendLine(line);
                if (string.IsNullOrEmpty(line) || line == TextHelper.NewLine)
                    break;
            }
            var all = stringBuilder.ToString();
            if (all.Contains("IsSignalGoOverIIS"))
                WebcoketDatagramBase.Current = new WebcoketIISDatagram();
            if (!stringBuilder.ToString().Contains("101 Switching Protocols"))
                throw new Exception(stringBuilder.ToString());
        }
#endif

        internal void Connect(string address, int port)
        {
            if (IsConnected)
                throw new Exception("client is connected!");
            if (IsDisposed)
                throw new ObjectDisposedException("Connector");
            if (port == 443)
                ProviderSetting.ServerServiceSetting.IsHttps = true;
            _address = address;
            _port = port;
            StreamHelper = SignalGoStreamBase.CurrentBase;
            if (ProtocolType == ClientProtocolType.SignalGoDuplex)
            {
                _client = new TcpClientWorker(new TcpClient());
            }
            else
            {
                _client = new WebSocketClientWorker(new TcpClient());
            }
#if (NETSTANDARD1_6)
            _client.ConnectAsync(address, port).GetAwaiter().GetResult();
#else
            _client.Connect(address, port);
#endif
            if (ProviderSetting.ServerServiceSetting.IsHttps)
            {
#if (NETSTANDARD1_6)
                throw new Exception("not support ssl in net standard 1.6 yet.");
#else

                if (ProtocolType == ClientProtocolType.WebSocket)
                {
                    _clientStream = new PipeNetworkStream(new NormalStream(_client.GetStream()));
#if (NETSTANDARD1_6)
                    ReadAllWebSocketResponseLines();
#else
                    ReadAllWebSocketResponseLines();
#endif
                    _clientStream = new PipeNetworkStream(new WebSocketStream(_client.GetStream()));
                }
                else
                {
                    SslStream sslStream = new SslStream(_client.GetStream());
                    sslStream.AuthenticateAsClient(address);
                    _clientStream = new PipeNetworkStream(new NormalStream(sslStream));
                }
#endif
            }
            else
            {
                _clientStream = new PipeNetworkStream(new NormalStream(_client.GetStream()));
                if (ProtocolType == ClientProtocolType.WebSocket)
                {
                    ReadAllWebSocketResponseLines();
                    _clientStream = new PipeNetworkStream(new WebSocketStream(_client.GetStream()));
                }
            }
            if (ProtocolType == ClientProtocolType.WebSocket)
                _clientStream.BufferToRead = ushort.MaxValue;
        }
        /// <summary>
        /// This registers service on server and methods that the client can call
        /// T type must inherited OprationCalls interface
        /// T type must not be an interface
        /// </summary>
        /// <typeparam name="T">type of class for call server methods</typeparam>
        /// <returns>return instance class for call methods</returns>
        public T RegisterServerService<T>(params object[] constructors)
        {
            if (IsDisposed)
                throw new ObjectDisposedException("Connector");
            if (constructors == null || constructors.Length == 0)
                constructors = new object[] { this };
            Type type = typeof(T);
            string name = type.GetServerServiceName(true);
            object objectInstance = Activator.CreateInstance(type, constructors);
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
            return GetType().GetMethod("GetDefaultGeneric").MakeGenericMethod(t).Invoke(this, null);
        }

#if (!NET35 && !NET40)
        internal async Task RunPrioritiesAsync()
        {
            if (!IsPriorityEnabled)
                return;
            foreach (Delegate item in PriorityActionsAfterConnected)
            {
                try
                {
                    if (!IsPriorityEnabled || !IsConnected)
                        break;
                    if (item is Action action)
                        action();
                    else if (item is Func<PriorityAction>)
                    {
                        PriorityAction priorityAction = PriorityAction.TryAgain;
                        do
                        {
                            if (!IsConnected)
                                break;
                            Thread.Sleep(ProviderSetting.PriorityFunctionDelayTime);
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
                    else if (item is Func<Task<PriorityAction>> function)
                    {
                        PriorityAction priorityAction = PriorityAction.TryAgain;
                        do
                        {
                            if (!IsConnected)
                                break;
                            await Task.Delay(ProviderSetting.PriorityFunctionDelayTime);
                            priorityAction = await function();
                            if (priorityAction == PriorityAction.BreakAll)
                                break;
                            else if (priorityAction == PriorityAction.HoldAll)
                            {
                                HoldAllPrioritiesTaskResult = new TaskCompletionSource<object>();
                                await HoldAllPrioritiesTaskResult.Task;
                            }
                        }
                        while (IsPriorityEnabled && priorityAction == PriorityAction.TryAgain);
                        if (priorityAction == PriorityAction.BreakAll)
                            break;
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
#endif
        internal void RunPriorities()
        {
            if (!IsPriorityEnabled)
                return;
            foreach (Delegate item in PriorityActionsAfterConnected)
            {
                try
                {
                    if (!IsPriorityEnabled || !IsConnected)
                        break;
                    if (item is Action action)
                        action();
                    else if (item is Func<PriorityAction>)
                    {
                        PriorityAction priorityAction = PriorityAction.TryAgain;
                        do
                        {
                            if (!IsConnected)
                                break;
                            Thread.Sleep(ProviderSetting.PriorityFunctionDelayTime);
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
                    else if (item is Func<Task<PriorityAction>> function)
                    {
                        PriorityAction priorityAction = PriorityAction.TryAgain;
                        do
                        {
                            if (!IsConnected)
                                break;
                            Thread.Sleep(ProviderSetting.PriorityFunctionDelayTime);

                            priorityAction = function().Result;
                            if (priorityAction == PriorityAction.BreakAll)
                                break;
                            else if (priorityAction == PriorityAction.HoldAll)
                            {
                                HoldAllPrioritiesTaskResult = new TaskCompletionSource<object>();
                                object result = HoldAllPrioritiesTaskResult.Task.Result;
                            }
                        }
                        while (IsPriorityEnabled && priorityAction == PriorityAction.TryAgain);
                        if (priorityAction == PriorityAction.BreakAll)
                            break;
                    }
                }
                catch (Exception ex)
                {

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

        /// <summary>
        /// un hold priority when you return hold all
        /// </summary>
        public void UnHoldPriority()
        {
            HoldAllPrioritiesResetEvent.Set();
            HoldAllPrioritiesTaskResult.SetResult(null);
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
                IStreamInfo iStream = null;
                foreach (object item in args)
                {
                    if (item is IStreamInfo value)
                    {
                        iStream = value;
                        iStream.ClientId = ClientId;
                    }
                }

                MethodInfo findMethod = typeof(ConnectorBase).FindMethod("UploadStreamAsync");
                MethodInfo madeMethod = findMethod.MakeGenericMethod(method.ReturnType);
                Task result = (Task)madeMethod.Invoke(this, new object[] { this, serverAddress, port, name, method.Name, ArgumentsToParameters(args).ToArray(), iStream });

#if (NET40 || NET35)
                result.Wait();
#else
                result.GetAwaiter().GetResult();
#endif
                if (method.ReturnType == typeof(void))
                    return null;
                return result.GetType().GetProperty("Result").GetValue(result, null);
            }, (serviceName, method, args) =>
            {
                IStreamInfo iStream = null;
                foreach (object item in args)
                {
                    if (item is IStreamInfo value)
                    {
                        iStream = value;
                        iStream.ClientId = ClientId;
                    }
                }

                MethodInfo findMethod = typeof(ConnectorBase).FindMethod("UploadStreamAsync");
                Type methodType = method.ReturnType.GetListOfGenericArguments().FirstOrDefault();
                MethodInfo madeMethod = findMethod.MakeGenericMethod(methodType);

                return madeMethod.Invoke(this, new object[] { this, serverAddress, port, name, method.Name, ArgumentsToParameters(args).ToArray(), iStream });
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
        public static T UploadStreamSync<T>(ClientProvider clientProvider, string serverAddress, int? port, string serviceName, string methodName, Shared.Models.ParameterInfo[] parameters, IStreamInfo iStream)
        {
#if (NET40 || NET35)
            return UploadStreamAsync<T>(clientProvider, serverAddress, port, serviceName, methodName, parameters, iStream).Result;
#else
            return UploadStreamAsync<T>(clientProvider, serverAddress, port, serviceName, methodName, parameters, iStream).GetAwaiter().GetResult();
#endif
        }

#if (NET40 || NET35)
        public static Task<T> UploadStreamAsync<T>(ClientProvider clientProvider, string serverAddress, int? port, string serviceName, string methodName, Shared.Models.ParameterInfo[] parameters, IStreamInfo iStream)
#else
        public static async Task<T> UploadStreamAsync<T>(ClientProvider clientProvider, string serverAddress, int? port, string serviceName, string methodName, Shared.Models.ParameterInfo[] parameters, IStreamInfo iStream)
#endif
        {
            ISignalGoStream streamHelper = clientProvider == null ? SignalGoStreamBase.CurrentBase : clientProvider.StreamHelper;
            int maximumReceiveStreamHeaderBlock = clientProvider == null ? int.MaxValue : clientProvider.ProviderSetting.MaximumReceiveStreamHeaderBlock;
            Type returnType = typeof(T);
            if (string.IsNullOrEmpty(serverAddress))
                serverAddress = clientProvider?._address;

            if (port == null || port.Value == 0)
                port = clientProvider?._port;

            if (port == null || string.IsNullOrEmpty(serverAddress))
                throw new Exception("please fill server address and port number");
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            TcpClient _newClient = new TcpClient();
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
            string header = "SignalGo-Stream/4.0" + TextHelper.NewLine;
            byte[] bytes = Encoding.UTF8.GetBytes(header);
#if (NET40 || NET35)
            stream.Write(bytes, 0, bytes.Length);
#else
            await stream.WriteAsync(bytes, 0, bytes.Length);
#endif
            bool isUpload = false;
            //if (parameters.Any(x => x.ParameterType == typeof(StreamInfo) || (x.ParameterType.GetIsGenericType() && x.ParameterType.GetGenericTypeDefinition() == typeof(StreamInfo<>))))
            if (iStream != null)
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
            callInfo.ServiceName = serviceName;
            //IStreamInfo iStream = null;
            //foreach (object item in args)
            //{
            //    if (item is IStreamInfo value)
            //    {
            //        iStream = value;
            //        iStream.ClientId = ClientId;
            //    }
            //}
            callInfo.Parameters = parameters;// method.MethodToParameters(x => ClientSerializationHelper.SerializeObject(x), args).ToArray();
            if (methodName.EndsWith("Async"))
                methodName = methodName.Substring(0, methodName.Length - 5);
            callInfo.MethodName = methodName;

            string json = ClientSerializationHelper.SerializeObject(callInfo);

            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
#if (NET40 || NET35)
            streamHelper.WriteBlockToStream(stream, jsonBytes);
#else
            await streamHelper.WriteBlockToStreamAsync(stream, jsonBytes);
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
                    firstData = iStream.ReadFirstData(stream, maximumReceiveStreamHeaderBlock);
#else
                    firstData = await iStream.ReadFirstDataAsync(stream, maximumReceiveStreamHeaderBlock);
#endif
                    if (firstData.Key == DataType.FlushStream)
                    {
#if (NET40 || NET35)
                        byte[] data = streamHelper.ReadBlockToEnd(stream, firstData.Value, maximumReceiveStreamHeaderBlock);
#else
                        byte[] data = await streamHelper.ReadBlockToEndAsync(stream, firstData.Value, maximumReceiveStreamHeaderBlock);
#endif
                        return BitConverter.ToInt64(data, 0);
                    }
                    return -1;
                };
                if (iStream.WriteManually != null && iStream.WriteManuallyAsync != null)
                    throw new Exception("don't set both of WriteManually and WriteManuallyAsync");
                if (iStream.WriteManually != null)
                    iStream.WriteManually(stream);
                else if (iStream.WriteManuallyAsync != null)
                {
#if (NET40 || NET35)
                    iStream.WriteManuallyAsync(stream).Wait();
#else
                    await iStream.WriteManuallyAsync(stream);
#endif
                }
                else
                {
                    long length = iStream.Length.Value;
                    long position = 0;
                    int blockOfRead = 1024 * 10;
                    while (length != position)
                    {

                        if (position + blockOfRead > length)
                            blockOfRead = (int)(length - position);
                        if (bytes.Length < blockOfRead)
                            bytes = new byte[blockOfRead];
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
                        firstData = iStream.ReadFirstData(stream, maximumReceiveStreamHeaderBlock);
#else
                        firstData = await iStream.ReadFirstDataAsync(stream, maximumReceiveStreamHeaderBlock);
#endif
                        if (firstData.Key == DataType.FlushStream)
                        {
#if (NET40 || NET35)
                            byte[] data = streamHelper.ReadBlockToEnd(stream, firstData.Value, maximumReceiveStreamHeaderBlock);
#else
                            byte[] data = await streamHelper.ReadBlockToEndAsync(stream, firstData.Value, maximumReceiveStreamHeaderBlock);
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
                byte dataTypeByte = streamHelper.ReadOneByte(stream);
                byte compressModeByte = streamHelper.ReadOneByte(stream);
#else
                byte dataTypeByte = await streamHelper.ReadOneByteAsync(stream);
                byte compressModeByte = await streamHelper.ReadOneByteAsync(stream);
#endif
            }
#if (NET40 || NET35)
            byte[] callBackBytes = streamHelper.ReadBlockToEnd(stream, compressMode, maximumReceiveStreamHeaderBlock);
#else
            byte[] callBackBytes = await streamHelper.ReadBlockToEndAsync(stream, compressMode, maximumReceiveStreamHeaderBlock);
#endif
            MethodCallbackInfo callbackInfo = ClientSerializationHelper.DeserializeObject<MethodCallbackInfo>(Encoding.UTF8.GetString(callBackBytes, 0, callBackBytes.Length));
            if (callbackInfo.IsException)
                throw new Exception(callbackInfo.Data);
            Type methodType = returnType;
            if (methodType.GetBaseType() == typeof(Task))
                methodType = returnType.GetListOfGenericArguments().FirstOrDefault();

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

        private static List<Shared.Models.ParameterInfo> ArgumentsToParameters(object[] args)
        {
            List<Shared.Models.ParameterInfo> parameters = new List<Shared.Models.ParameterInfo>();
            foreach (object item in args)
            {
                if (item is SignalGo.Shared.Models.ParameterInfo p)
                    parameters.Add(p);
                else
                    parameters.Add(new Shared.Models.ParameterInfo() { Value = ClientSerializationHelper.SerializeObject(item) });
            }
            return parameters;
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
            TcpClient _newClient = new TcpClient();
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

            string line = "SignalGo-OneWay/4.0" + TextHelper.NewLine;
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
            DataType dataType = (DataType)await stream.ReadOneByteAsync();
            CompressMode compressMode = (CompressMode)await stream.ReadOneByteAsync();
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
#if (NET40 || NET35)
        Func<string> ReadJsonFromStreamFunction { get; set; }
#else
        Func<Task<string>> ReadJsonFromStreamFunction { get; set; }
#endif

#if (NET40 || NET35)
        Func<DataType> ReadDataTypeFunction { get; set; }
        Func<CompressMode> ReadCompressModeFunction { get; set; }
#else
        Func<Task<DataType>> ReadDataTypeFunction { get; set; }
        Func<Task<CompressMode>> ReadCompressModeFunction { get; set; }
#endif


#if (NET40 || NET35)
        string NormalReadJsonFromStream()
#else
        async Task<string> NormalReadJsonFromStream()
#endif
        {
#if (NET40 || NET35)
            byte[] bytes = StreamHelper.ReadBlockToEnd(_clientStream, CompressMode.None, ProviderSetting.MaximumReceiveDataBlock);
#else
            byte[] bytes = await StreamHelper.ReadBlockToEndAsync(_clientStream, CompressMode.None, ProviderSetting.MaximumReceiveDataBlock);
#endif
            if (SecuritySettings != null)
                bytes = DecryptBytes(bytes);
            var json = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            return json;
        }


#if (NET40 || NET35)
        string WebsocketReadJsonFromStream()
#else
        async Task<string> WebsocketReadJsonFromStream()
#endif
        {
#if (NET40 || NET35)
            string json = _clientStream.ReadLine("#end");
#else
            string json = await _clientStream.ReadLineAsync("#end");
#endif
            //json = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            return json.Substring(0, json.Length - 4); ;
        }


#if (NET40 || NET35)
        DataType NormalReadDataType()
#else
        async Task<DataType> NormalReadDataType()
#endif
        {
#if (NET40 || NET35)
            int dataTypeByte = StreamHelper.ReadOneByte(_clientStream);
#else
            int dataTypeByte = await StreamHelper.ReadOneByteAsync(_clientStream);
#endif
            return (DataType)dataTypeByte;
        }


#if (NET40 || NET35)
        CompressMode NormalReadCompressMode()
#else
        async Task<CompressMode> NormalReadCompressMode()
#endif
        {
#if (NET40 || NET35)
            int compressModeByte = StreamHelper.ReadOneByte(_clientStream);
#else
            int compressModeByte = await StreamHelper.ReadOneByteAsync(_clientStream);
#endif
            return (CompressMode)compressModeByte;
        }
#if (NET40 || NET35)
        DataType WebsocketReadDataType()
#else
        async Task<DataType> WebsocketReadDataType()
#endif
        {
#if (NET40 || NET35)
            byte dataTypeByte = StreamHelper.ReadOneByte(_clientStream);
#else
            byte dataTypeByte = await StreamHelper.ReadOneByteAsync(_clientStream);
#endif
            //read ','
            char character = (char)dataTypeByte;
#if (NET40 || NET35)
            StreamHelper.ReadOneByte(_clientStream);
#else
            await StreamHelper.ReadOneByteAsync(_clientStream);
#endif
            return (DataType)int.Parse(character.ToString());
        }

#if (NET40 || NET35)
        CompressMode WebsocketReadCompressMode()
#else
        async Task<CompressMode> WebsocketReadCompressMode()
#endif
        {
#if (NET40 || NET35)
            byte compressModeByte = StreamHelper.ReadOneByte(_clientStream);
#else
            byte compressModeByte = await StreamHelper.ReadOneByteAsync(_clientStream);
#endif
            char character = (char)compressModeByte;
            //read '/'
#if (NET40 || NET35)
            StreamHelper.ReadOneByte(_clientStream);
#else
            await StreamHelper.ReadOneByteAsync(_clientStream);
#endif
            return (CompressMode)int.Parse(character.ToString());
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
            if (ProtocolType == ClientProtocolType.WebSocket)
            {
                ReadDataTypeFunction = WebsocketReadDataType;
                ReadCompressModeFunction = WebsocketReadCompressMode;
                ReadJsonFromStreamFunction = WebsocketReadJsonFromStream;
            }
            else
            {
                ReadDataTypeFunction = NormalReadDataType;
                ReadCompressModeFunction = NormalReadCompressMode;
                ReadJsonFromStreamFunction = NormalReadJsonFromStream;
            }

            try
            {

#if (NET35 || NET40)
                Task.Factory.StartNew(() =>
#else
                await Task.Factory.StartNew(async () =>
#endif
                {
                    try
                    {
                        while (true)
                        {
                            //first byte is DataType
#if (NET35 || NET40)
                            DataType dataType = ReadDataTypeFunction();
#else
                            DataType dataType = await ReadDataTypeFunction();
#endif

                            if (dataType == DataType.PingPong)
                            {
                                PingAndWaitForPong.Set();
                                continue;
                            }
                            //secound byte is compress mode
#if (NET35 || NET40)
                            CompressMode compresssMode = ReadCompressModeFunction();
#else
                            CompressMode compresssMode = await ReadCompressModeFunction();
#endif
                            // server is called client method
                            if (dataType == DataType.CallMethod)
                            {
#if (NET40 || NET35)
                                string json = ReadJsonFromStreamFunction();
#else
                                string json = await ReadJsonFromStreamFunction();
#endif
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
                                string json = ReadJsonFromStreamFunction();
#else
                                string json = await ReadJsonFromStreamFunction();
#endif

                                MethodCallbackInfo callback = ClientSerializationHelper.DeserializeObject<MethodCallbackInfo>(json);

                                bool geted = WaitedMethodsForResponse.TryGetValue(callback.Guid, out TaskCompletionSource<MethodCallbackInfo> keyValue);
                                if (geted)
                                {
                                    if (callback.IsException)
                                    {
                                        keyValue.SetException(new Exception(callback.Data));
                                    }
                                    else
                                        keyValue.SetResult(callback);
                                }
                            }
                            else if (dataType == DataType.GetServiceDetails)
                            {
#if (NET40 || NET35)
                                string json = ReadJsonFromStreamFunction();
#else
                                string json = await ReadJsonFromStreamFunction();
#endif
                                ProviderDetailsInfo getServiceDetialResult = ClientSerializationHelper.DeserializeObject<ProviderDetailsInfo>(json);
                                if (getServiceDetialResult == null)
                                    ServiceDetailEventTaskResult.SetException(ClientSerializationHelper.DeserializeObject<Exception>(json));
                                else
                                    ServiceDetailEventTaskResult.SetResult(getServiceDetialResult);
                            }
                            else if (dataType == DataType.GetMethodParameterDetails)
                            {
#if (NET40 || NET35)
                                string json = ReadJsonFromStreamFunction();
#else
                                string json = await ReadJsonFromStreamFunction();
#endif
                                ServiceParameterDetailEventTaskResult.SetResult(json);
                            }
                            else if (dataType == DataType.GetClientId)
                            {
#if (NET40 || NET35)
                                string json = ReadJsonFromStreamFunction();
#else
                                string json = await ReadJsonFromStreamFunction();
#endif
                                //ClientId = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
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
                        Console.WriteLine("Client Disconnected");
                        AutoLogger.LogError(ex, "StartToReadingClientData");
                        Disconnect();
                    }
                });
            }
            catch
            {

            }
        }

        private ManualResetEvent PingAndWaitForPong = new ManualResetEvent(true);
#if (!NET40 && !NET35)
        public async Task<bool> SendPingAndWaitToReceiveAsync()
        {
            try
            {
                PingAndWaitForPong.Reset();
                await StreamHelper.WriteToStreamAsync(_clientStream, new byte[] { (byte)DataType.PingPong });
                return PingAndWaitForPong.WaitOne(new TimeSpan(0, 0, 3));
            }
            catch (Exception ex)
            {
                AutoLogger.LogError(ex, "ConnectorBase SendData");
                return false;
            }
        }
#endif

        public bool SendPingAndWaitToReceive()
        {
            try
            {
                PingAndWaitForPong.Reset();
                StreamHelper.WriteToStream(_clientStream, new byte[] { (byte)DataType.PingPong });
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
                //                if (IsWebSocket)
                //                {
                //                    string json = ClientSerializationHelper.SerializeObject(callback) + "#end";
                //                    byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
                //#if (NET40 || NET35)
                //                    StreamHelper.WriteToStream(_clientStream, new byte[] { (byte)DataType.CallMethod });
                //                    StreamHelper.WriteToStream(_clientStream, new byte[] { (byte)CompressMode.None });
                //                    StreamHelper.WriteToStream(_clientStream, jsonBytes.ToArray());
                //#else
                //                    await StreamHelper.WriteToStreamAsync(_clientStream, new byte[] { (byte)DataType.CallMethod });
                //                    await StreamHelper.WriteToStreamAsync(_clientStream, new byte[] { (byte)CompressMode.None });
                //                    await StreamHelper.WriteToStreamAsync(_clientStream, jsonBytes);
                //#endif
                //                }
                //                else
                //                {
                string json = ClientSerializationHelper.SerializeObject(callback);
                if (ProtocolType == ClientProtocolType.WebSocket)
                {
                    json += "#end";
                }

                List<byte> bytes = new List<byte>();
                byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
                if (SecuritySettings != null)
                    jsonBytes = EncryptBytes(jsonBytes);
                if (ProtocolType != ClientProtocolType.WebSocket)
                {
                    bytes.Add((byte)DataType.CallMethod);
                    bytes.Add((byte)CompressMode.None);

                    byte[] dataLen = BitConverter.GetBytes(jsonBytes.Length);
                    bytes.AddRange(dataLen);
                }
                else
                {
                    bytes.Add((byte)'1');
                    bytes.Add((byte)',');
                    bytes.Add((byte)'0');
                    bytes.Add((byte)'/');
                }
                bytes.AddRange(jsonBytes);
                if (bytes.Count > ProviderSetting.MaximumSendDataBlock)
                    throw new Exception("SendData data length is upper than MaximumSendDataBlock");
#if (NET40 || NET35)
                    StreamHelper.WriteToStream(_clientStream, bytes.ToArray());
#else
                await StreamHelper.WriteToStreamAsync(_clientStream, bytes.ToArray());
#endif
                //}
            }
            catch (Exception ex)
            {
                try
                {
                    if (WaitedMethodsForResponse.TryGetValue(callback.Guid, out TaskCompletionSource<MethodCallbackInfo> keyValue))
                    {
                        keyValue.SetException(ex);
                    }
                }
                catch
                {


                }
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
                if (!Callbacks.TryGetValue(callInfo.ServiceName.ToLower(), out KeyValue<SynchronizationContext, object> keyValue))
                    throw new Exception($"Callback service {callInfo.ServiceName} not found or not registred in client side!");
                object service = keyValue.Value;
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
                try
                {
                    AutoLogger.LogError(ex, "ConnectorBase CallMethod");
                    callback.IsException = true;
                    callback.Data = ClientSerializationHelper.SerializeObject(ex.ToString());
                }
                catch
                {

                }
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
            try
            {
                string json = ClientSerializationHelper.SerializeObject(callback);
                if (ProtocolType == ClientProtocolType.WebSocket)
                    json += "#end";
                List<byte> data = new List<byte>();
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                if (SecuritySettings != null)
                    bytes = EncryptBytes(bytes);
                if (ProtocolType != ClientProtocolType.WebSocket)
                {
                    data.Add((byte)DataType.ResponseCallMethod);
                    data.Add((byte)CompressMode.None);
                    byte[] len = BitConverter.GetBytes(bytes.Length);
                    data.AddRange(len);
                }
                else
                {
                    data.Add((byte)'2');
                    data.Add((byte)',');
                    data.Add((byte)'0');
                    data.Add((byte)'/');
                }
                data.AddRange(bytes);
                if (data.Count > ProviderSetting.MaximumSendDataBlock)
                    throw new Exception("SendCallbackData data length is upper than MaximumSendDataBlock");

#if (NET40 || NET35)
                StreamHelper.WriteToStream(_clientStream, data.ToArray());
#else
                await StreamHelper.WriteToStreamAsync(_clientStream, data.ToArray());
#endif
            }
            catch (Exception ex)
            {
                AutoLogger.LogError(ex, "SendCallbackData");
            }
        }


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
            ServiceDetailEventTaskResult = new TaskCompletionSource<ProviderDetailsInfo>();
#if (NET40 || NET35)
            StreamHelper.WriteToStream(_clientStream, data.ToArray());
            return ServiceDetailEventTaskResult.Task.Result;
#else
            await StreamHelper.WriteToStreamAsync(_clientStream, data.ToArray());
            bool isReceived = await Task.WhenAny(ServiceDetailEventTaskResult.Task, Task.Delay(new TimeSpan(0, 0, 15))) == ServiceDetailEventTaskResult.Task;
            if (!isReceived)
                throw new TimeoutException();
            return await ServiceDetailEventTaskResult.Task;
#endif
        }

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
            ServiceParameterDetailEventTaskResult = new TaskCompletionSource<string>();
#if (NET40 || NET35)
            StreamHelper.WriteToStream(_clientStream, data.ToArray());
            return ServiceParameterDetailEventTaskResult.Task.Result;
#else
            await StreamHelper.WriteToStreamAsync(_clientStream, data.ToArray());
            return await ServiceParameterDetailEventTaskResult.Task;
#endif
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

        /// <summary>
        /// calls this function after connected and befor holded methods
        /// if you return false this will hold methods and call this function again after a time until you return true
        /// </summary>
        /// <param name="function"></param>
        public void AddPriorityAsyncFunction(Func<Task<PriorityAction>> function)
        {
            PriorityActionsAfterConnected.Add(function);
        }

        public void Disconnect()
        {
            if (IsDisposed)
                throw new ObjectDisposedException("Connector");

            if (_client != null)
                _client.Dispose();
            if (IsConnected)
            {
                IsConnected = false;
            }
            foreach (KeyValuePair<string, TaskCompletionSource<MethodCallbackInfo>> item in WaitedMethodsForResponse)
            {
                item.Value.TrySetCanceled();
            }
            WaitedMethodsForResponse.Clear();

            OnConnectionChanged?.Invoke(ConnectionStatus.Disconnected);

            //if (!IsAutoReconnecting)
            //{
            //    lock (_autoReconnectLock)
            //    {
            //        if (ProviderSetting.AutoReconnect && !IsAutoReconnecting)
            //        {
            //            IsAutoReconnecting = true;
            //            while (!IsConnected && !IsDisposed)
            //            {
            //                try
            //                {
            //                    OnConnectionChanged?.Invoke(ConnectionStatus.Reconnecting);
            //                    OnAutoReconnecting?.Invoke();
            //                    Connect(ServerUrl);
            //                }
            //                catch (Exception ex)
            //                {

            //                }
            //                finally
            //                {
            //                    AutoReconnectDelayResetEvent.Reset();
            //                    AutoReconnectDelayResetEvent.WaitOne(ProviderSetting.AutoReconnectTime);
            //                }
            //            }
            //            //foreach (var item in HoldMethodsToReconnect.ToList())
            //            //{
            //            //    item.Set();
            //            //}
            //            //HoldMethodsToReconnect.Clear();
            //            IsAutoReconnecting = false;
            //        }
            //    }
            //}
            //else
            //{

            //}
            if (AutoReconnectWaitToDisconnectTaskResult.Task.Status != TaskStatus.RanToCompletion)
                AutoReconnectWaitToDisconnectTaskResult.SetResult(null);
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
