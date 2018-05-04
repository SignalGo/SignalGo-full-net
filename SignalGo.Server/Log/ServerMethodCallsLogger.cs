using Newtonsoft.Json;
using SignalGo.Server.Models;
using SignalGo.Shared.Events;
using SignalGo.Shared.Http;
using SignalGo.Shared.Log;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SignalGo.Server.Log
{
    /// <summary>
    /// Base class for log information
    /// </summary>
    public abstract class BaseLogInformation
    {
        public string CallerGuid { get; set; }
        /// <summary>
        /// Ignore log for this information
        /// </summary>
        public bool IsIgnoreLogTextFile { get; set; }
        /// <summary>
        /// Calling method date. This snapshots date and time when method is called
        /// </summary>
        public DateTime DateTimeStartMethod { get; set; }
        /// <summary>
        /// Result method date. This tells when a method execution has been completed
        /// </summary>
        public DateTime DateTimeEndMethod { get; set; }
        /// <summary>
        /// Method elapsed time. The duration of method execution.
        /// </summary>
        public TimeSpan Elapsed
        {
            get
            {
                return DateTimeEndMethod - DateTimeStartMethod;
            }
        }

        /// <summary>
        /// True = can write to file, False = nothing is written into log file
        /// </summary>
        public bool CanWriteToFile { get; set; }
        /// <summary>
        /// The client ID
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// Client connection Date. Registered when connection to the server has been established.
        /// </summary>
        public DateTime ConnectedDateTime { get; set; }
        /// <summary>
        /// The client address
        /// </summary>
        public string IPAddress { get; set; }
        /// <summary>
        /// The method result. Can be a complex object too
        /// </summary>
        public string Result { get; set; }
        /// <summary>
        /// Name of the method called and executed
        /// </summary>
        public string MethodName { get; set; }
        /// <summary>
        /// The exception message showed if the called method raises an exception
        /// </summary>
        public Exception Exception { get; set; }
    }

    /// <summary>
    /// Method's log
    /// </summary>
    public class CallMethodLogInformation : BaseLogInformation
    {
        /// <summary>
        /// The name of the service
        /// </summary>
        public string ServiceName { get; set; }
        /// <summary>
        /// The method parameters
        /// </summary>
        public List<SignalGo.Shared.Models.ParameterInfo> Parameters { get; set; }
        /// <summary>
        /// The method
        /// </summary>
        public MethodInfo Method { get; set; }
    }

    /// <summary>
    /// Callbacks log
    /// </summary>
    public class CallClientMethodLogInformation : BaseLogInformation
    {
        /// <summary>
        /// The service name
        /// </summary>
        public string ServiceName { get; set; }
        /// <summary>
        /// The parameters
        /// </summary>
        public List<SignalGo.Shared.Models.ParameterInfo> Parameters { get; set; }
    }

    /// <summary>
    /// Http calls log
    /// </summary>
    public class HttpCallMethodLogInformation : BaseLogInformation
    {
        /// <summary>
        /// Address of http caller
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// The parameters
        /// </summary>
        public List<string> Parameters { get; set; }
        /// <summary>
        /// The method
        /// </summary>
        public MethodInfo Method { get; set; }
    }

    /// <summary>
    /// Stream services log
    /// </summary>
    public class StreamCallMethodLogInformation : BaseLogInformation
    {
        /// <summary>
        /// The service name
        /// </summary>
        public string ServiceName { get; set; }
        /// <summary>
        /// The parameters
        /// </summary>
        public List<SignalGo.Shared.Models.ParameterInfo> Parameters { get; set; }
    }

    /// <summary>
    /// SignalGo log system manager
    /// </summary>
    public class ServerMethodCallsLogger : IDisposable
    {
        public AutoLogger AutoLogger { get; set; } = new AutoLogger() { FileName = "ServerMethodCalls Logs.log" };
        /// <summary>
        /// Action raised when a client calls a method on server (service) and receive a response
        /// </summary>
        public Action<CallMethodLogInformation> OnServiceMethodCalledAction { get; set; }
        /// <summary>
        /// Action raised when the server calls a method on client
        /// </summary>
        public Action<CallClientMethodLogInformation> OnServiceCallbackMethodCalledAction { get; set; }
        /// <summary>
        /// Action raised when a client calls an HTTP method
        /// </summary>
        public Action<HttpCallMethodLogInformation> OnHttpServiceMethodCalledAction { get; set; }
        /// <summary>
        /// Action raised when a client calls a streams method
        /// </summary>
        public Action<StreamCallMethodLogInformation> OnStreamServiceMethodCalledAction { get; set; }

        /// <summary>
        /// Initialize events and starts the service
        /// </summary>
        public void Initialize()
        {
            MethodsCallHandler.BeginClientMethodCallAction += new BeginClientMethodCallAction(BeginClientMethodCallAction);
            MethodsCallHandler.BeginHttpMethodCallAction += new BeginHttpCallAction(BeginHttpMethodCallAction);
            MethodsCallHandler.BeginMethodCallAction += new BeginMethodCallAction(BeginMethodCallAction);
            MethodsCallHandler.BeginStreamCallAction += new BeginStreamCallAction(BeginStreamCallAction);

            MethodsCallHandler.EndClientMethodCallAction += new EndClientMethodCallAction(EndClientMethodCallAction);
            MethodsCallHandler.EndHttpMethodCallAction += new EndHttpCallAction(EndHttpMethodCallAction);
            MethodsCallHandler.EndMethodCallAction += new EndMethodCallAction(EndMethodCallAction);
            MethodsCallHandler.EndStreamCallAction += new EndStreamCallAction(EndStreamCallAction);
            StartEngine();
        }

        void BeginClientMethodCallAction(object clientInfo, string callGuid, string serviceName, string methodName, List<SignalGo.Shared.Models.ParameterInfo> values)
        {
            ClientInfo client = (ClientInfo)clientInfo;
            var result = AddCallClientMethodLog(client.ClientId, client.IPAddress, client.ConnectedDateTime, serviceName, methodName, values);
            if (result != null)
                result.CallerGuid = callGuid;
        }

        void BeginHttpMethodCallAction(object clientInfo, string callGuid, string address, MethodInfo method, List<string> values)
        {
            ClientInfo client = (ClientInfo)clientInfo;
            var result = AddHttpMethodLog(client.ClientId, client.IPAddress, client.ConnectedDateTime, address, method, values);
            if (result != null)
                result.CallerGuid = callGuid;
        }

        void BeginMethodCallAction(object clientInfo, string callGuid, string serviceName, MethodInfo method, List<SignalGo.Shared.Models.ParameterInfo> values)
        {
            ClientInfo client = (ClientInfo)clientInfo;
            var result = AddCallMethodLog(client.ClientId, client.IPAddress, client.ConnectedDateTime, serviceName, method, values);
            if (result != null)
                result.CallerGuid = callGuid;
        }

        void BeginStreamCallAction(object clientInfo, string callGuid, string serviceName, string methodName, List<SignalGo.Shared.Models.ParameterInfo> values)
        {
            ClientInfo client = (ClientInfo)clientInfo;
            var result = AddStreamCallMethodLog(client.ClientId, client.IPAddress, client.ConnectedDateTime, serviceName, methodName, values);
            if (result != null)
                result.CallerGuid = callGuid;
        }

        void EndClientMethodCallAction(object clientInfo, string callGuid, string serviceName, string methodName, object[] values, string result, Exception exception)
        {
            var find = Logs.FirstOrDefault(x => x.CallerGuid == callGuid);
            if (find != null)
            {
                find.Exception = exception;
                FinishLog((CallClientMethodLogInformation)find, result);
            }
        }

        void EndHttpMethodCallAction(object clientInfo, string callGuid, string address, System.Reflection.MethodInfo method, List<string> values, object result, Exception exception)
        {
            var find = Logs.FirstOrDefault(x => x.CallerGuid == callGuid);
            if (find != null)
            {
                find.Exception = exception;
                FinishLog((HttpCallMethodLogInformation)find, result == null ? "" : JsonConvert.SerializeObject(result, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Error = (o, e) =>
                    {

                    }
                }));
            }
        }

        void EndMethodCallAction(object clientInfo, string callGuid, string serviceName, System.Reflection.MethodInfo method, List<SignalGo.Shared.Models.ParameterInfo> values, string result, Exception exception)
        {
            var find = Logs.FirstOrDefault(x => x.CallerGuid == callGuid);
            if (find != null)
            {
                find.Exception = exception;
                FinishLog((CallMethodLogInformation)find, result);
            }
        }

        void EndStreamCallAction(object clientInfo, string callGuid, string serviceName, string methodName, List<SignalGo.Shared.Models.ParameterInfo> values, string result, Exception exception)
        {
            var find = Logs.FirstOrDefault(x => x.CallerGuid == callGuid);
            if (find != null)
            {
                find.Exception = exception;
                FinishLog((StreamCallMethodLogInformation)find, result);
            }
        }


        /// <summary>
        /// True = the logger is started. False = the logger is stopped
        /// </summary>
        public bool IsStart
        {
            get
            {
                return !isStop;
            }
        }
        /// <summary>
        /// True = it logs persian date time.
        /// </summary>
        public bool IsPersianDateLog { get; set; } = false;

        bool isStop = true;

        Task _thread = null;
        void StartEngine()
        {
#if (PORTABLE)
            throw new NotSupportedException();
#else
            if (!isStop)
                return;
            isStop = false;
            _thread = Task.Factory.StartNew(() =>
            {
                try
                {
                    BaseLogInformation nextLog = null;
                    while (!isStop)
                    {
                        if (Logs.TryPeek(out nextLog) && nextLog.CanWriteToFile)
                        {
                            if (isStop)
                                break;
                            if (Logs.TryDequeue(out nextLog))
                            {
                                if (isStop)
                                    break;
                                if (nextLog.IsIgnoreLogTextFile)
                                    continue;
                                if (nextLog is CallMethodLogInformation)
                                    WriteToFile((CallMethodLogInformation)nextLog);
                                else if (nextLog is HttpCallMethodLogInformation)
                                    WriteToFile((HttpCallMethodLogInformation)nextLog);
                                else if (nextLog is CallClientMethodLogInformation)
                                    WriteToFile((CallClientMethodLogInformation)nextLog);
                                else if (nextLog is StreamCallMethodLogInformation)
                                    WriteToFile((StreamCallMethodLogInformation)nextLog);
                            }
                            else
                            {
                                AutoLogger.LogText("WTF MethodCallsLogger StartEngine");
                            }
                        }
                        else
                        {
                            Thread.Sleep(1000);
                        }
                    }
                }
                catch (Exception ex)
                {
                    AutoLogger.LogError(ex, "MethodCallsLogger StartEngine");
                }
                isStop = true;
            });
#endif
        }

        ConcurrentQueue<BaseLogInformation> Logs = new ConcurrentQueue<BaseLogInformation>();

        public CallMethodLogInformation AddCallMethodLog(string clientId, string ipAddress, DateTime connectedDateTime, string serviceName, MethodInfo method, List<SignalGo.Shared.Models.ParameterInfo> parameters)
        {
            if (isStop)
                return null;
            var log = new CallMethodLogInformation() { DateTimeStartMethod = DateTime.Now.ToLocalTime(), Method = method, Parameters = parameters, ServiceName = serviceName, ConnectedDateTime = connectedDateTime, IPAddress = ipAddress, ClientId = clientId, MethodName = method?.Name };
            Logs.Enqueue(log);
            return log;
        }

        public HttpCallMethodLogInformation AddHttpMethodLog(string clientId, string ipAddress, DateTime connectedDateTime, string address, MethodInfo method, List<string> parameters)
        {
            if (isStop)
                return null;
            var log = new HttpCallMethodLogInformation() { DateTimeStartMethod = DateTime.Now.ToLocalTime(), Method = method, Parameters = parameters, Address = address, ConnectedDateTime = connectedDateTime, IPAddress = ipAddress, ClientId = clientId, MethodName = method?.Name };
            Logs.Enqueue(log);
            return log;
        }

        public CallClientMethodLogInformation AddCallClientMethodLog(string clientId, string ipAddress, DateTime connectedDateTime, string serviceName, string methodName, List<SignalGo.Shared.Models.ParameterInfo> parameters)
        {
            if (isStop)
                return null;
            var log = new CallClientMethodLogInformation() { DateTimeStartMethod = DateTime.Now.ToLocalTime(), MethodName = methodName, Parameters = parameters, ServiceName = serviceName, ConnectedDateTime = connectedDateTime, IPAddress = ipAddress, ClientId = clientId };
            Logs.Enqueue(log);
            return log;
        }

        public StreamCallMethodLogInformation AddStreamCallMethodLog(string clientId, string ipAddress, DateTime connectedDateTime, string serviceName, string methodName, List<SignalGo.Shared.Models.ParameterInfo> parameters)
        {
            if (isStop)
                return null;
            var log = new StreamCallMethodLogInformation() { DateTimeStartMethod = DateTime.Now.ToLocalTime(), MethodName = methodName, Parameters = parameters, ServiceName = serviceName, ConnectedDateTime = connectedDateTime, IPAddress = ipAddress, ClientId = clientId };
            Logs.Enqueue(log);
            return log;
        }

        public void FinishLog(CallMethodLogInformation log, string result)
        {
            if (isStop)
                return;
            if (log == null)
                return;
            log.DateTimeEndMethod = DateTime.Now.ToLocalTime();
            if (log.Exception == null)
                log.Result = result;
            OnServiceMethodCalledAction?.Invoke(log);
            log.CanWriteToFile = true;
        }

        public void FinishLog(HttpCallMethodLogInformation log, string result)
        {
            if (isStop)
                return;
            if (log == null)
                return;
            log.DateTimeEndMethod = DateTime.Now.ToLocalTime();
            if (log.Exception == null)
                log.Result = result;
            OnHttpServiceMethodCalledAction?.Invoke(log);
            log.CanWriteToFile = true;
        }

        public void FinishLog(CallClientMethodLogInformation log, string result)
        {
            if (isStop)
                return;
            if (log == null)
                return;
            log.DateTimeEndMethod = DateTime.Now.ToLocalTime();
            if (log.Exception == null)
                log.Result = result;
            OnServiceCallbackMethodCalledAction?.Invoke(log);
            log.CanWriteToFile = true;
        }

        public void FinishLog(StreamCallMethodLogInformation log, string result)
        {
            if (isStop)
                return;
            if (log == null)
                return;
            log.DateTimeEndMethod = DateTime.Now.ToLocalTime();
            if (log.Exception == null)
                log.Result = result;
            OnStreamServiceMethodCalledAction?.Invoke(log);
            log.CanWriteToFile = true;
        }

        string CombinePath(params string[] pathes)
        {
            string result = pathes[0];
            foreach (var item in pathes.Skip(1))
            {
                result = System.IO.Path.Combine(result, item);
            }
            return result;
        }

        void WriteToFile(CallMethodLogInformation log)
        {
#if (PORTABLE)
            throw new NotSupportedException();
#else
#if (NET35)
            string path = CombinePath(AutoLogger.DirectoryLocation, "Logs", log.DateTimeStartMethod.Year.ToString(), log.DateTimeStartMethod.Month.ToString(), log.DateTimeStartMethod.Day.ToString());
#else
            string path = System.IO.Path.Combine(AutoLogger.DirectoryLocation, "Logs", log.DateTimeStartMethod.Year.ToString(), log.DateTimeStartMethod.Month.ToString(), log.DateTimeStartMethod.Day.ToString());
#endif
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
            path = System.IO.Path.Combine(path, $"{log.DateTimeStartMethod.Year}-{log.DateTimeStartMethod.Month}-{log.DateTimeStartMethod.Day} {log.DateTimeStartMethod.ToLocalTime().Hour}.log");

            StringBuilder build = new StringBuilder();
            build.AppendLine("########################################");
            build.AppendLine("Client Information:");
            build.AppendLine($"	Ip Address:	{log.IPAddress}");
            build.AppendLine($"	ClientId:	{log.ClientId}");
            build.AppendLine($"	Connected Time:	{GetDateTimeString(log.ConnectedDateTime)}");
            build.AppendLine("");
            build.AppendLine($"Call Information:");
            build.AppendLine($"	Service Name:	{log.ServiceName}");
            build.Append($"	Method:		{log.Method.Name}(");
            bool isFirst = true;
            foreach (var parameter in log.Method.GetParameters())
            {
                build.Append((isFirst ? "" : ",") + parameter.ParameterType.Name + " " + parameter.Name);
                isFirst = false;
            }
            build.AppendLine(")");

            build.AppendLine($"	With Values:");
            foreach (var parameter in log.Parameters)
            {
                build.AppendLine("			" + (parameter.Value == null ? "Null" : JsonConvert.SerializeObject(parameter.Value, Formatting.None, new JsonSerializerSettings() { Formatting = Formatting.None }).Replace(@"\""", "")));
            }
            build.AppendLine("");

            if (log.Exception == null)
            {
                build.AppendLine($"Result Information:");
                build.AppendLine("			" + log.Result);
                build.AppendLine("");
            }
            else
            {
                build.AppendLine($"Exception:");
                build.AppendLine("			" + log.Exception.ToString());
                build.AppendLine("");
            }


            build.AppendLine($"Invoked Time:");
            build.AppendLine($"			{GetDateTimeString(log.DateTimeStartMethod)}");
            build.AppendLine($"Result Time:");
            build.AppendLine($"			{GetDateTimeString(log.DateTimeEndMethod)}");
            build.AppendLine("----------------------------------------------------------------------------------------");
            build.AppendLine("");
            using (var stream = new System.IO.FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                stream.Seek(0, System.IO.SeekOrigin.End);
                byte[] bytes = Encoding.UTF8.GetBytes(build.ToString());
                stream.Write(bytes, 0, bytes.Length);
            }
#endif
        }

        void WriteToFile(CallClientMethodLogInformation log)
        {
#if (PORTABLE)
            throw new NotSupportedException();
#else
#if (NET35)
            string path = CombinePath(AutoLogger.DirectoryLocation, "Logs", log.DateTimeStartMethod.Year.ToString(), log.DateTimeStartMethod.Month.ToString(), log.DateTimeStartMethod.Day.ToString());
#else
            string path = System.IO.Path.Combine(AutoLogger.DirectoryLocation, "Logs", log.DateTimeStartMethod.Year.ToString(), log.DateTimeStartMethod.Month.ToString(), log.DateTimeStartMethod.Day.ToString());
#endif
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
            path = System.IO.Path.Combine(path, $"Callback-{log.DateTimeStartMethod.Year}-{log.DateTimeStartMethod.Month}-{log.DateTimeStartMethod.Day} {log.DateTimeStartMethod.ToLocalTime().Hour}.log");

            StringBuilder build = new StringBuilder();
            build.AppendLine("########################################");
            build.AppendLine("Client Information:");
            build.AppendLine($"	Ip Address:	{log.IPAddress}");
            build.AppendLine($"	ClientId:	{log.ClientId}");
            build.AppendLine($"	Connected Time:	{GetDateTimeString(log.ConnectedDateTime)}");
            build.AppendLine("");
            build.AppendLine($"Call Information:");
            build.AppendLine($"	Service Name:	{log.ServiceName}");
            build.Append($"	Method:		{log.MethodName}(");
            bool isFirst = true;
            int index = 1;
            foreach (var parameter in log.Parameters)
            {
                build.Append((isFirst ? "" : ",") + (parameter.Type == null ? "Null" : parameter.Type) + " obj" + index);
                isFirst = false;
                index++;
            }
            build.AppendLine(")");

            build.AppendLine($"	With Values:");
            foreach (var parameter in log.Parameters)
            {
                build.AppendLine("			" + (parameter.Value == null ? "Null" : JsonConvert.SerializeObject(parameter.Value, Formatting.None, new JsonSerializerSettings() { Formatting = Formatting.None }).Replace(@"\""", "")));
            }
            build.AppendLine("");
            if (log.Exception == null)
            {
                build.AppendLine($"Result Information:");
                build.AppendLine("			" + log.Result);
                build.AppendLine("");
            }
            else
            {
                build.AppendLine($"Exception:");
                build.AppendLine("			" + log.Exception.ToString());
                build.AppendLine("");
            }
            build.AppendLine($"Invoked Time:");
            build.AppendLine($"			{GetDateTimeString(log.DateTimeStartMethod)}");
            build.AppendLine($"Result Time:");
            build.AppendLine($"			{GetDateTimeString(log.DateTimeEndMethod)}");
            build.AppendLine("----------------------------------------------------------------------------------------");
            build.AppendLine("");
            using (var stream = new System.IO.FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                stream.Seek(0, System.IO.SeekOrigin.End);
                byte[] bytes = Encoding.UTF8.GetBytes(build.ToString());
                stream.Write(bytes, 0, bytes.Length);
            }
#endif
        }

        void WriteToFile(HttpCallMethodLogInformation log)
        {
#if (PORTABLE)
            throw new NotSupportedException();
#else
#if (NET35)
            string path = CombinePath(AutoLogger.DirectoryLocation, "Logs", log.DateTimeStartMethod.Year.ToString(), log.DateTimeStartMethod.Month.ToString(), log.DateTimeStartMethod.Day.ToString());
#else
            string path = System.IO.Path.Combine(AutoLogger.DirectoryLocation, "Logs", log.DateTimeStartMethod.Year.ToString(), log.DateTimeStartMethod.Month.ToString(), log.DateTimeStartMethod.Day.ToString());
#endif
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
            path = System.IO.Path.Combine(path, $"HTTP-{log.DateTimeStartMethod.Year}-{log.DateTimeStartMethod.Month}-{log.DateTimeStartMethod.Day} {log.DateTimeStartMethod.ToLocalTime().Hour}.log");

            StringBuilder build = new StringBuilder();
            build.AppendLine("########################################");
            build.AppendLine("Client Information:");
            build.AppendLine($"	Ip Address:	{log.IPAddress}");
            build.AppendLine($"	ClientId:	{log.ClientId}");
            build.AppendLine($"	Connected Time:	{GetDateTimeString(log.ConnectedDateTime)}");
            build.AppendLine("");
            build.AppendLine($"Call Information:");
            build.AppendLine($"	Address:	{log.Address}");
            build.Append($"	Method:		{log.Method.Name}(");
            bool isFirst = true;
            foreach (var parameter in log.Method.GetParameters())
            {
                build.Append((isFirst ? "" : ",") + parameter.ParameterType.Name + " " + parameter.Name);
                isFirst = false;
            }
            build.AppendLine(")");
            if (log.Parameters != null)
            {
                build.AppendLine($"	With Values:");
                foreach (var value in log.Parameters)
                {
                    build.AppendLine("			" + (value == null ? "Null" : value));
                }
                build.AppendLine("");
            }
            if (log.Exception == null)
            {
                build.AppendLine($"Result Information:");
                build.AppendLine("			" + log.Result);
                build.AppendLine("");
            }
            else
            {
                build.AppendLine($"Exception:");
                build.AppendLine("			" + log.Exception.ToString());
            }
            build.AppendLine($"Invoked Time:");
            build.AppendLine($"			{GetDateTimeString(log.DateTimeStartMethod)}");
            build.AppendLine($"Result Time:");
            build.AppendLine($"			{GetDateTimeString(log.DateTimeEndMethod)}");
            build.AppendLine("----------------------------------------------------------------------------------------");
            build.AppendLine("");
            using (var stream = new System.IO.FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                stream.Seek(0, System.IO.SeekOrigin.End);
                byte[] bytes = Encoding.UTF8.GetBytes(build.ToString());
                stream.Write(bytes, 0, bytes.Length);
            }
#endif
        }

        void WriteToFile(StreamCallMethodLogInformation log)
        {
#if (PORTABLE)
            throw new NotSupportedException();
#else
#if (NET35)
            string path = CombinePath(AutoLogger.DirectoryLocation, "Logs", log.DateTimeStartMethod.Year.ToString(), log.DateTimeStartMethod.Month.ToString(), log.DateTimeStartMethod.Day.ToString());
#else
            string path = System.IO.Path.Combine(AutoLogger.DirectoryLocation, "Logs", log.DateTimeStartMethod.Year.ToString(), log.DateTimeStartMethod.Month.ToString(), log.DateTimeStartMethod.Day.ToString());
#endif
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
            path = System.IO.Path.Combine(path, $"Stream-{log.DateTimeStartMethod.Year}-{log.DateTimeStartMethod.Month}-{log.DateTimeStartMethod.Day} {log.DateTimeStartMethod.ToLocalTime().Hour}.log");

            StringBuilder build = new StringBuilder();
            build.AppendLine("########################################");
            build.AppendLine("Client Information:");
            build.AppendLine($"	Ip Address:	{log.IPAddress}");
            build.AppendLine($"	ClientId:	{log.ClientId}");
            build.AppendLine($"	Connected Time:	{GetDateTimeString(log.ConnectedDateTime)}");
            build.AppendLine("");
            build.AppendLine($"Call Information:");
            build.AppendLine($"	Service Name:	{log.ServiceName}");
            build.Append($"	Method:		{log.MethodName}");
            //build.Append($"	Method:		{log.Method.Name}(");
            //bool isFirst = true;
            //foreach (var parameter in log.Method.GetParameters())
            //{
            //    build.Append((isFirst ? "" : ",") + parameter.ParameterType.Name + " " + parameter.Name);
            //    isFirst = false;
            //}
            //build.AppendLine(")");
            if (log.Parameters != null)
            {
                build.AppendLine($"	With Values:");
                foreach (var parameter in log.Parameters)
                {
                    build.AppendLine("			" + (parameter.Value == null ? "Null" : JsonConvert.SerializeObject(parameter.Value, Formatting.None, new JsonSerializerSettings() { Formatting = Formatting.None }).Replace(@"\""", "")));
                }
                build.AppendLine("");
            }
            if (log.Exception == null)
            {
                build.AppendLine($"Result Information:");
                build.AppendLine("			" + log.Result);
                build.AppendLine("");
            }
            else
            {
                if (log.Exception != null)
                {
                    build.AppendLine($"Exception:");
                    build.AppendLine("			" + log.Exception.ToString());
                    build.AppendLine("");
                }
            }


            build.AppendLine($"Invoked Time:");
            build.AppendLine($"			{GetDateTimeString(log.DateTimeStartMethod)}");
            build.AppendLine($"Result Time:");
            build.AppendLine($"			{GetDateTimeString(log.DateTimeEndMethod)}");
            build.AppendLine("----------------------------------------------------------------------------------------");
            build.AppendLine("");
            using (var stream = new System.IO.FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                stream.Seek(0, System.IO.SeekOrigin.End);
                byte[] bytes = Encoding.UTF8.GetBytes(build.ToString());
                stream.Write(bytes, 0, bytes.Length);
            }
#endif
        }

        string GetDateTimeString(DateTime dt)
        {
#if (PORTABLE)
            throw new NotSupportedException();
#else
            PersianCalendar persian = new PersianCalendar();
            string log = "";
            if (IsPersianDateLog)
                log = $"{persian.GetYear(dt)}/{persian.GetMonth(dt)}/{persian.GetDayOfMonth(dt)} {persian.GetHour(dt)}:{persian.GetMinute(dt)}:{persian.GetSecond(dt)}.{persian.GetMilliseconds(dt)} ## ";
            return log + $"{dt.ToString("MM/dd/yyyy HH:mm:ss")}.{dt.Millisecond}";
#endif
        }

        public void Dispose()
        {
            MethodsCallHandler.BeginClientMethodCallAction -= new BeginClientMethodCallAction(BeginClientMethodCallAction);
            MethodsCallHandler.BeginHttpMethodCallAction -= new BeginHttpCallAction(BeginHttpMethodCallAction);
            MethodsCallHandler.BeginMethodCallAction -= new BeginMethodCallAction(BeginMethodCallAction);

            MethodsCallHandler.EndClientMethodCallAction -= new EndClientMethodCallAction(EndClientMethodCallAction);
            MethodsCallHandler.EndHttpMethodCallAction -= new EndHttpCallAction(EndHttpMethodCallAction);
            MethodsCallHandler.EndMethodCallAction -= new EndMethodCallAction(EndMethodCallAction);

            isStop = true;
            foreach (var item in Logs)
            {
                item.CanWriteToFile = true;
            }
            Logs.Enqueue(new CallMethodLogInformation() { CanWriteToFile = true });
        }
    }
}
