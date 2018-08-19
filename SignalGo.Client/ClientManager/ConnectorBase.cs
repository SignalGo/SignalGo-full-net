﻿using Newtonsoft.Json;
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

        bool _IsConnected = false;
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
        internal AutoResetEvent AutoReconnectDelayResetEvent { get; set; } = new AutoResetEvent(true);
        internal AutoResetEvent HoldAllPrioritiesResetEvent { get; set; } = new AutoResetEvent(true);

        internal SecuritySettingsInfo SecuritySettings { get; set; } = null;

        internal string _address = "";
        internal int _port = 0;
        object _connectLock = new object();
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
                _client = new TcpClient();
                _client.NoDelay = true;
                var result = _client.BeginConnect(address, port, null, null);

                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5));

                if (!success)
                {
                    throw new Exception("Failed to connect.");
                }

                // we have connected
                _client.EndConnect(result);
#endif
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
            var type = typeof(T);
            var name = type.GetServerServiceName();
            var objectInstance = Activator.CreateInstance(type);
            var duplex = objectInstance as OperationCalls;
            if (duplex != null)
                duplex.Connector = this;
            Services.TryAdd(name, objectInstance);
            return (T)objectInstance;
        }

        /// <summary>
        /// register service by name
        /// </summary>
        /// <param name="name"></param>
        public void RegisterServerService(string name)
        {
            if (IsDisposed)
                throw new ObjectDisposedException("Connector");
            MethodCallInfo callInfo = new MethodCallInfo()
            {
                ServiceName = name,
                MethodName = "/RegisterService",
                Guid = Guid.NewGuid().ToString()
            };
            var callback = this.SendData<MethodCallbackInfo>(callInfo);
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

        internal void RunPriorities()
        {
            if (!IsPriorityEnabled)
                return;
            foreach (var item in PriorityActionsAfterConnected)
            {
                if (!IsPriorityEnabled)
                    break;
                if (item is Action)
                    ((Action)item)();
                else if (item is Func<PriorityAction>)
                {
                    var priorityAction = PriorityAction.TryAgain;
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
            var type = typeof(T);
            var name = type.GetServerServiceName();

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
        public T RegisterServerServiceInterfaceWrapper<T>() where T : class
        {
            if (IsDisposed)
                throw new ObjectDisposedException("Connector");
            var type = typeof(T);

            var name = type.GetServerServiceName();

            if (!ProviderSetting.AutoDetectRegisterServices)
            {
                MethodCallInfo callInfo = new MethodCallInfo()
                {
                    ServiceName = name,
                    MethodName = "/RegisterService",
                    Guid = Guid.NewGuid().ToString()
                };
                var callback = this.SendData<MethodCallbackInfo>(callInfo);
            }

            var objectInstance = InterfaceWrapper.Wrap<T>((serviceName, method, args) =>
            {
                //this is async action
                if (method.ReturnType == typeof(Task))
                {
                    string methodName = method.Name;
                    if (methodName.EndsWith("Async"))
                        methodName = methodName.Substring(0, methodName.Length - 5);
#if (NET40 || NET35)
                    return Task.Factory.StartNew(() =>
#else
                    return Task.Run(() =>
#endif
                    {
                        ConnectorExtensions.SendData(this, serviceName, methodName, method.MethodToParameters(x => ClientSerializationHelper.SerializeObject(x), args).ToArray());
                    });
                }
                //this is async function
                else if (method.ReturnType.GetBaseType() == typeof(Task))
                {
                    string methodName = method.Name;
                    if (methodName.ToLower().EndsWith("async"))
                        methodName = methodName.Substring(0, methodName.Length - 5);
#if (!PORTABLE)
                    // ConnectorExtension.SendDataAsync<object>()
                    var findMethod = typeof(ConnectorExtensions).FindMethod("SendDataTask", BindingFlags.Static | BindingFlags.NonPublic);
                    var methodType = method.ReturnType.GetListOfGenericArguments().FirstOrDefault();
                    var madeMethod = findMethod.MakeGenericMethod(methodType);
                    return madeMethod.Invoke(this, new object[] { this, serviceName, methodName, method, args });

#else
                    throw new NotSupportedException();
#endif

                    //var funcR = new Func<object>(() =>
                    //{
                    //    var data = ConnectorExtension.SendDataAsync(this, serviceName, methodName, args);
                    //    if (data == null)
                    //        return null;
                    //    if (data is StreamInfo)
                    //        return data;
                    //    else
                    //    {
                    //        var result = ClientSerializationHelper.DeserializeObject(data.ToString(), methodType);
                    //        return result;
                    //    }
                    //});
                    //var mc_custom_type = typeof(FunctionCaster<>).MakeGenericType(methodType);
                    //var mc_instance = Activator.CreateInstance(mc_custom_type);
                    //var mc_custom_method = mc_custom_type.FindMethod("Do");
                    //return mc_custom_method.Invoke(mc_instance, new object[] { funcR });
                }
                else
                {
                    if (typeof(void) == method.ReturnType)
                    {
                        ConnectorExtensions.SendData(this, serviceName, method.Name, method.MethodToParameters(x => ClientSerializationHelper.SerializeObject(x), args).ToArray());
                        return null;
                    }
                    else
                    {
                        var getServiceMethod = type.FindMethod(method.Name);
                        var customDataExchanger = getServiceMethod.GetCustomAttributes(typeof(CustomDataExchangerAttribute), true).Cast<CustomDataExchangerAttribute>().Where(x => x.GetExchangerByUserCustomization(this)).ToList();
                        var data = ConnectorExtensions.SendData(this, serviceName, method.Name, method.MethodToParameters(x => ClientSerializationHelper.SerializeObject(x), args).ToArray());
                        if (data == null)
                            return null;
                        var result = ClientSerializationHelper.DeserializeObject(data.ToString(), method.ReturnType, customDataExchanger: customDataExchanger.ToArray());
                        return result;
                    };
                }
            });

            Services.TryAdd(name, objectInstance);
            return (T)objectInstance;
        }

        ConcurrentDictionary<string, object> InstancesOfRegisterStreamService = new ConcurrentDictionary<string, object>();
        /// <summary>
        /// register service and method to server for file or stream download and upload
        /// </summary>
        /// <typeparam name="T">type of interface for create instanse</typeparam>
        /// <returns>return instance of interface that client can call methods</returns>
        public T RegisterStreamServiceInterfaceWrapper<T>(string serverAddress = null, int? port = null) where T : class
        {
            if (IsDisposed)
                throw new ObjectDisposedException("Connector");
            var type = typeof(T);
            var serviceType = type.GetServerServiceAttribute();
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

            var name = type.GetServerServiceName();

            var objectInstance = InterfaceWrapper.Wrap<T>((serviceName, method, args) =>
            {
                if (method.ReturnType == typeof(Task))
                {
                    var task = Task.Factory.StartNew(() =>
                    {
                        UploadStream(name, serverAddress, port, serviceName, method, args, false);
                    });
                    return task;
                }
                //this is async function
                else if (method.ReturnType.GetBaseType() == typeof(Task))
                {
#if (!PORTABLE)
                    // ConnectorExtension.SendDataAsync<object>()
                    var findMethod = typeof(ConnectorBase).FindMethod("UploadStreamAsync", BindingFlags.Instance | BindingFlags.NonPublic);
                    var methodType = method.ReturnType.GetListOfGenericArguments().FirstOrDefault();
                    var madeMethod = findMethod.MakeGenericMethod(methodType);
                    return madeMethod.Invoke(this, new object[] { name, serverAddress, port, serviceName, method, args });

#else
                    throw new NotSupportedException();
#endif
                }
                else
                    return UploadStream(name, serverAddress, port, serviceName, method, args, false);
            });
            InstancesOfRegisterStreamService.TryAdd(callKey, objectInstance);
            return (T)objectInstance;
        }

        private Task<T> UploadStreamAsync<T>(string name, string serverAddress, int? port, string serviceName, MethodInfo method, object[] args)
        {
            return Task<T>.Factory.StartNew(() =>
            {
                return (T)UploadStream(name, serverAddress, port, serviceName, method, args, true);
            });
        }

        public object UploadStream(string name, string serverAddress, int? port, string serviceName, MethodInfo method, object[] args, bool isAsync)
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
            var _newClient = new TcpClient(serverAddress, port.Value);
            _newClient.NoDelay = true;
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
            var header = "SignalGo-Stream/4.0\r\n";
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
            BaseStreamInfo iStream = null;
            foreach (var item in args)
            {
                if (item is BaseStreamInfo value)
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

            var json = ClientSerializationHelper.SerializeObject(callInfo);

            var jsonBytes = Encoding.UTF8.GetBytes(json);
            StreamHelper.WriteBlockToStream(stream, jsonBytes);
            CompressMode compressMode = CompressMode.None;
            if (isUpload)
            {
                KeyValue<DataType, CompressMode> firstData = null;
                iStream.GetPositionFlush = () =>
                {
                    if (firstData != null && firstData.Key != DataType.FlushStream)
                        return -1;
                    firstData = iStream.ReadFirstData(readStream, ProviderSetting.MaximumReceiveStreamHeaderBlock);
                    if (firstData.Key == DataType.FlushStream)
                    {
                        var data = StreamHelper.ReadBlockToEnd(readStream, firstData.Value, ProviderSetting.MaximumReceiveStreamHeaderBlock);
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
                        bytes = new byte[blockOfRead];
                        var readCount = iStream.Stream.Read(bytes, 0, bytes.Length);
                        position += readCount;
                        stream.Write(bytes, 0, readCount);
                    }
                }
                if (firstData == null || firstData.Key == DataType.FlushStream)
                {
                    while (true)
                    {
                        firstData = iStream.ReadFirstData(readStream, ProviderSetting.MaximumReceiveStreamHeaderBlock);
                        if (firstData.Key == DataType.FlushStream)
                        {
                            var data = StreamHelper.ReadBlockToEnd(readStream, firstData.Value, ProviderSetting.MaximumReceiveStreamHeaderBlock);
                        }
                        else
                            break;
                    }
                }
            }
            else
            {
                var dataTypeByte = StreamHelper.ReadOneByte(readStream, compressMode, ProviderSetting.MaximumReceiveStreamHeaderBlock);
                var compressModeByte = StreamHelper.ReadOneByte(readStream, compressMode, ProviderSetting.MaximumReceiveStreamHeaderBlock);
            }
            var callBackBytes = StreamHelper.ReadBlockToEnd(readStream, compressMode, ProviderSetting.MaximumReceiveStreamHeaderBlock);
            var callbackInfo = ClientSerializationHelper.DeserializeObject<MethodCallbackInfo>(Encoding.UTF8.GetString(callBackBytes, 0, callBackBytes.Length));
            var methodType = method.ReturnType;
            if (isAsync)
                methodType = method.ReturnType.GetListOfGenericArguments().FirstOrDefault();

            var result = ClientSerializationHelper.DeserializeObject(callbackInfo.Data, methodType);
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

            return result;
        }

        public static T SendOneWayMethod<T>(string serverAddress, int port, string serviceName, string methodName, params Shared.Models.ParameterInfo[] parameters)
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
            var _newClient = new TcpClient(serverAddress, port);
            _newClient.NoDelay = true;
#endif
#if (PORTABLE)
            var stream = _newClient.WriteStream;
            var readStream = _newClient.ReadStream;
#else
            var stream = _newClient.GetStream();
            var readStream = stream;
#endif
            MethodCallInfo callInfo = new MethodCallInfo
            {
                ServiceName = serviceName,
                MethodName = methodName
            };
            callInfo.Parameters = parameters;

            if (methodName.EndsWith("Async"))
                methodName = methodName.Substring(0, methodName.Length - 5);

            var json = ClientSerializationHelper.SerializeObject(callInfo);
            var jsonBytes = Encoding.UTF8.GetBytes(json);

            var line = "SignalGo-OneWay/4.0" + Environment.NewLine;
            var lineBytes = Encoding.UTF8.GetBytes(line);
            var streamHelper = new SignalGoStreamBase();
            streamHelper.WriteToStream(stream, lineBytes);
            streamHelper.WriteBlockToStream(stream, jsonBytes);

            var dataType = (DataType)stream.ReadByte();
            var compressMode = (CompressMode)stream.ReadByte();
            var readData = streamHelper.ReadBlockToEnd(stream, compressMode, uint.MaxValue);
            json = Encoding.UTF8.GetString(readData, 0, readData.Length);
            var callBack = ClientSerializationHelper.DeserializeObject<MethodCallbackInfo>(json);
            if (callBack.IsException)
                throw ClientSerializationHelper.DeserializeObject<Exception>(callBack.Data);
            return ClientSerializationHelper.DeserializeObject<T>(callBack.Data);
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
            var type = typeof(T);
            var name = type.GetServerServiceName();
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
        /// register a server service interface and get dynamic calls
        /// works for all platform like windows ,android ,ios and ...
        /// </summary>
        /// <typeparam name="T">interface type for use dynamic call</typeparam>
        /// <returns>return dynamic type to call methods</returns>
        public dynamic RegisterServerServiceDynamic<T>() where T : class
        {
            if (IsDisposed)
                throw new ObjectDisposedException("Connector");
            var type = typeof(T);
            var name = type.GetServerServiceName();
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
        /// register client service class, it's client methods that server call them
        /// </summary>
        /// <typeparam name="T">type of your class</typeparam>
        /// <returns>return instance if type</returns>
        public T RegisterClientService<T>()
        {
            if (IsDisposed)
                throw new ObjectDisposedException("Connector");
            var type = typeof(T);
            var name = type.GetClientServiceName();

            var objectInstance = Activator.CreateInstance(type);
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
            await Task.Run(() =>
#endif
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
                        var dataTypeByte = stream.ReadByte();
                        var dataType = (DataType)dataTypeByte;
                        if (dataType == DataType.PingPong)
                        {
                            PingAndWaitForPong.Set();
                            continue;
                        }
                        //secound byte is compress mode
                        var compressModeByte = stream.ReadByte();
                        var compresssMode = (CompressMode)compressModeByte;

                        // server is called client method
                        if (dataType == DataType.CallMethod)
                        {
                            var bytes = StreamHelper.ReadBlockToEnd(stream, compresssMode, ProviderSetting.MaximumReceiveDataBlock);
                            if (SecuritySettings != null)
                                bytes = DecryptBytes(bytes);
                            var json = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
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
                            var bytes = StreamHelper.ReadBlockToEnd(stream, compresssMode, ProviderSetting.MaximumReceiveDataBlock);
                            if (SecuritySettings != null)
                                bytes = DecryptBytes(bytes);
                            var json = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                            MethodCallbackInfo callback = ClientSerializationHelper.DeserializeObject<MethodCallbackInfo>(json);

                            var geted = ConnectorExtensions.WaitedMethodsForResponse.TryGetValue(callback.Guid, out KeyValue<AutoResetEvent, MethodCallbackInfo> keyValue);
                            if (geted)
                            {
                                keyValue.Value = callback;
                                keyValue.Key.Set();
                            }
                        }
                        else if (dataType == DataType.GetServiceDetails)
                        {
                            var bytes = StreamHelper.ReadBlockToEnd(stream, compresssMode, ProviderSetting.MaximumReceiveDataBlock);
                            if (SecuritySettings != null)
                                bytes = DecryptBytes(bytes);
                            var json = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                            getServiceDetialResult = ClientSerializationHelper.DeserializeObject<ProviderDetailsInfo>(json);
                            if (getServiceDetialResult == null)
                                getServiceDetialExceptionResult = ClientSerializationHelper.DeserializeObject<Exception>(json);
                            getServiceDetailEvent.Set();
                            getServiceDetailEvent.Reset();
                        }
                        else if (dataType == DataType.GetMethodParameterDetails)
                        {
                            var bytes = StreamHelper.ReadBlockToEnd(stream, compresssMode, ProviderSetting.MaximumReceiveDataBlock);
                            if (SecuritySettings != null)
                                bytes = DecryptBytes(bytes);
                            var json = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                            getmethodParameterDetailsResult = json;
                            getServiceDetailEvent.Set();
                            getServiceDetailEvent.Reset();
                        }
                        else if (dataType == DataType.GetClientId)
                        {
                            var bytes = StreamHelper.ReadBlockToEnd(stream, compresssMode, ProviderSetting.MaximumReceiveDataBlock);
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

        ManualResetEvent PingAndWaitForPong = new ManualResetEvent(true);
        public bool SendPingAndWaitToReceive()
        {
            try
            {
                PingAndWaitForPong.Reset();
#if (PORTABLE)
                var stream = _client.WriteStream;
#else
                var stream = _client.GetStream();
#endif
                StreamHelper.WriteToStream(stream, new byte[] { (byte)DataType.PingPong });
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

        internal void SendDataSync(MethodCallInfo data)
        {
            try
            {
#if (PORTABLE)
                var stream = _client.WriteStream;
#else
                var stream = _client.GetStream();
#endif
                var json = ClientSerializationHelper.SerializeObject(data);
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
                StreamHelper.WriteToStream(stream, bytes.ToArray());
            }
            catch (Exception ex)
            {
                AutoLogger.LogError(ex, "ConnectorBase SendData");
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
                var service = Callbacks[callInfo.ServiceName].Value;
                //#if (PORTABLE)
                var method = service.GetType().FindMethod(callInfo.MethodName);
                //#else
                //                var method = service.GetType().GetMethod(callInfo.MethodName, RuntimeTypeHelper.GetMethodTypes(service.GetType(), callInfo).ToArray());
                //#endif
                if (method == null)
                    throw new Exception($"Method {callInfo.MethodName} from service {callInfo.ServiceName} not found! serviceType: {service.GetType().FullName}");
                List<object> parameters = new List<object>();
                int index = 0;
                foreach (var item in method.GetParameters())
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
                        var task = ((Task)method.Invoke(service, parameters.ToArray()));
                        task.Wait();
                        data = task.GetType().GetProperty("Result").GetValue(task,null);
#else
                        var task = ((Task)method.Invoke(service, parameters.ToArray()));
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
        internal void SendCallbackData(MethodCallbackInfo callback)
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

#if (PORTABLE)
            var stream = _client.WriteStream;
#else
            var stream = _client.GetStream();
#endif
            StreamHelper.WriteToStream(stream, data.ToArray());
        }


        ManualResetEvent getServiceDetailEvent = new ManualResetEvent(false);
        ProviderDetailsInfo getServiceDetialResult = null;
        Exception getServiceDetialExceptionResult = null;

        public ProviderDetailsInfo GetListOfServicesWithDetials(string hostUrl)
        {
            AsyncActions.Run(() =>
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
#if (PORTABLE)
                var stream = _client.WriteStream;
#else
                var stream = _client.GetStream();
#endif
                StreamHelper.WriteToStream(stream, data.ToArray());
            });
            getServiceDetailEvent.WaitOne();
            if (getServiceDetialExceptionResult != null)
                throw getServiceDetialExceptionResult;
            return getServiceDetialResult;
        }

        string getmethodParameterDetailsResult = "";
        public string GetMethodParameterDetial(MethodParameterDetails methodParameterDetails)
        {
            if (IsDisposed)
                throw new ObjectDisposedException("Connector");
            AsyncActions.Run(() =>
            {
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
#if (PORTABLE)
                var stream = _client.WriteStream;
#else
                var stream = _client.GetStream();
#endif
                StreamHelper.WriteToStream(stream, data.ToArray());
            });
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

        object _autoReconnectLock = new object();
        bool _ManulyDisconnected = false;
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
            foreach (var item in ConnectorExtensions.WaitedMethodsForResponse)
            {
                item.Value.Key.Set();
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
