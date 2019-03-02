using Newtonsoft.Json;
using SignalGo.Server.Models;
using SignalGo.Shared.Events;
using SignalGo.Shared.Log;
using System;
using System.Collections.Concurrent;
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
    /// base of log information
    /// </summary>
    public abstract class BaseLogInformation
    {
        public string CallerGuid { get; set; }
        /// <summary>
        /// ignore log for this iformation
        /// </summary>
        public bool IsIgnoreLogTextFile { get; set; }
        /// <summary>
        /// date of call
        /// </summary>
        public DateTime DateTimeStartMethod { get; set; }
        /// <summary>
        /// date of result
        /// </summary>
        public DateTime DateTimeEndMethod { get; set; }
        /// <summary>
        /// Elapsed time from start call to result
        /// </summary>
        public TimeSpan Elapsed
        {
            get
            {
                return DateTimeEndMethod - DateTimeStartMethod;
            }
        }

        /// <summary>
        /// after set call method result is going to true and write to log file
        /// </summary>
        public bool CanWriteToFile { get; set; }
        /// <summary>
        /// client clientId
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// client connected Date Time
        /// </summary>
        public DateTime ConnectedDateTime { get; set; }
        /// <summary>
        /// ip addresses
        /// </summary>
        public string IPAddress { get; set; }
        /// <summary>
        /// object od result
        /// </summary>
        public string Result { get; set; }
        /// <summary>
        /// name of method
        /// </summary>
        public string MethodName { get; set; }
        /// <summary>
        /// if method call have exception result
        /// </summary>
        public Exception Exception { get; set; }
    }

    /// <summary>
    /// log of method called
    /// </summary>
    public class CallMethodLogInformation : BaseLogInformation
    {
        /// <summary>
        /// name of service
        /// </summary>
        public string ServiceName { get; set; }
        /// <summary>
        /// parameters of method
        /// </summary>
        public SignalGo.Shared.Models.ParameterInfo[] Parameters { get; set; }
        /// <summary>
        /// method
        /// </summary>
        public MethodInfo Method { get; set; }
    }

    /// <summary>
    /// log of callbacks
    /// </summary>
    public class CallClientMethodLogInformation : BaseLogInformation
    {
        /// <summary>
        /// service name
        /// </summary>
        public string ServiceName { get; set; }
        /// <summary>
        /// parameters
        /// </summary>
        public SignalGo.Shared.Models.ParameterInfo[] Parameters { get; set; }
    }

    /// <summary>
    /// log of http calls
    /// </summary>
    public class HttpCallMethodLogInformation : BaseLogInformation
    {
        /// <summary>
        /// address of http call
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// parameters
        /// </summary>
        public SignalGo.Shared.Models.ParameterInfo[] Parameters { get; set; }
        /// <summary>
        /// method
        /// </summary>
        public MethodInfo Method { get; set; }
    }

    /// <summary>
    /// log of stream services
    /// </summary>
    public class StreamCallMethodLogInformation : BaseLogInformation
    {
        /// <summary>
        /// address of http call
        /// </summary>
        public string ServiceName { get; set; }
        /// <summary>
        /// parameters
        /// </summary>
        public SignalGo.Shared.Models.ParameterInfo[] Parameters { get; set; }
    }

    /// <summary>
    /// signalGo log system manager
    /// </summary>
    public class ServerMethodCallsLogger : IDisposable
    {
        public AutoLogger AutoLogger { get; set; } = new AutoLogger() { FileName = "ServerMethodCalls Logs.log" };
        /// <summary>
        /// when user called and response a service method
        /// </summary>
        public Action<CallMethodLogInformation> OnServiceMethodCalledAction { get; set; }
        /// <summary>
        /// when server called a client method
        /// </summary>
        public Action<CallClientMethodLogInformation> OnServiceCallbackMethodCalledAction { get; set; }
        /// <summary>
        /// when a http method called from client
        /// </summary>
        public Action<HttpCallMethodLogInformation> OnHttpServiceMethodCalledAction { get; set; }
        /// <summary>
        /// when a stream method called from client
        /// </summary>
        public Action<StreamCallMethodLogInformation> OnStreamServiceMethodCalledAction { get; set; }

        /// <summary>
        /// initialize events and start service
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
            //StartEngine();
        }

        private void BeginClientMethodCallAction(object clientInfo, string callGuid, string serviceName, string methodName, SignalGo.Shared.Models.ParameterInfo[] values)
        {
            //ClientInfo client = (ClientInfo)clientInfo;
            //CallClientMethodLogInformation result = AddCallClientMethodLog(client.ClientId, client.IPAddress, client.ConnectedDateTime, serviceName, methodName, values);
            //if (result != null)
            //    result.CallerGuid = callGuid;
        }

        private void BeginHttpMethodCallAction(object clientInfo, string callGuid, string address, MethodInfo method, SignalGo.Shared.Models.ParameterInfo[] values)
        {
            //ClientInfo client = (ClientInfo)clientInfo;
            //HttpCallMethodLogInformation result = AddHttpMethodLog(client.ClientId, client.IPAddress, client.ConnectedDateTime, address, method, values);
            //if (result != null)
            //    result.CallerGuid = callGuid;
        }

        private void BeginMethodCallAction(object clientInfo, string callGuid, string serviceName, MethodInfo method, SignalGo.Shared.Models.ParameterInfo[] values)
        {
            //ClientInfo client = (ClientInfo)clientInfo;
            //CallMethodLogInformation result = AddCallMethodLog(client.ClientId, client.IPAddress, client.ConnectedDateTime, serviceName, method, values);
            //if (result != null)
            //    result.CallerGuid = callGuid;
        }

        private void BeginStreamCallAction(object clientInfo, string callGuid, string serviceName, string methodName, SignalGo.Shared.Models.ParameterInfo[] values)
        {
            //ClientInfo client = (ClientInfo)clientInfo;
            //StreamCallMethodLogInformation result = AddStreamCallMethodLog(client.ClientId, client.IPAddress, client.ConnectedDateTime, serviceName, methodName, values);
            //if (result != null)
            //    result.CallerGuid = callGuid;
        }

        private void EndClientMethodCallAction(object clientInfo, string callGuid, string serviceName, string methodName, object[] values, string result, Exception exception)
        {
            //BaseLogInformation find = Logs.FirstOrDefault(x => x.CallerGuid == callGuid);
            //if (find != null)
            //{
            //    find.Exception = exception;
            //    FinishLog((CallClientMethodLogInformation)find, result);
            //}
        }

        private void EndHttpMethodCallAction(object clientInfo, string callGuid, string address, System.Reflection.MethodInfo method, SignalGo.Shared.Models.ParameterInfo[] values, object result, Exception exception)
        {
            //BaseLogInformation find = Logs.FirstOrDefault(x => x.CallerGuid == callGuid);
            //if (find != null)
            //{
            //    find.Exception = exception;
            //    FinishLog((HttpCallMethodLogInformation)find, result == null ? "" : JsonConvert.SerializeObject(result, new JsonSerializerSettings()
            //    {
            //        NullValueHandling = NullValueHandling.Ignore,
            //        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            //        Error = (o, e) =>
            //        {

            //        }
            //    }));
            //}
        }

        private void EndMethodCallAction(object clientInfo, string callGuid, string serviceName, System.Reflection.MethodInfo method, SignalGo.Shared.Models.ParameterInfo[] values, string result, Exception exception)
        {
            //BaseLogInformation find = Logs.FirstOrDefault(x => x.CallerGuid == callGuid);
            //if (find != null)
            //{
            //    find.Exception = exception;
            //    FinishLog((CallMethodLogInformation)find, result);
            //}
        }

        private void EndStreamCallAction(object clientInfo, string callGuid, string serviceName, string methodName, SignalGo.Shared.Models.ParameterInfo[] values, string result, Exception exception)
        {
            //BaseLogInformation find = Logs.FirstOrDefault(x => x.CallerGuid == callGuid);
            //if (find != null)
            //{
            //    find.Exception = exception;
            //    FinishLog((StreamCallMethodLogInformation)find, result);
            //}
        }


        /// <summary>
        /// when system logger is started
        /// </summary>
        public bool IsStart
        {
            get
            {
                return !isStop;
            }
        }
        /// <summary>
        /// if you want log persian datet time
        /// </summary>
        public bool IsPersianDateLog { get; set; } = false;

        private bool isStop = true;
        private Task _thread = null;

        private void StartEngine()
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

        private ConcurrentQueue<BaseLogInformation> Logs = new ConcurrentQueue<BaseLogInformation>();

        public CallMethodLogInformation AddCallMethodLog(string clientId, string ipAddress, DateTime connectedDateTime, string serviceName, MethodInfo method, SignalGo.Shared.Models.ParameterInfo[] parameters)
        {
            if (isStop)
                return null;
            CallMethodLogInformation log = new CallMethodLogInformation() { DateTimeStartMethod = DateTime.Now.ToLocalTime(), Method = method, Parameters = parameters, ServiceName = serviceName, ConnectedDateTime = connectedDateTime, IPAddress = ipAddress, ClientId = clientId, MethodName = method?.Name };
            Logs.Enqueue(log);
            return log;
        }

        public HttpCallMethodLogInformation AddHttpMethodLog(string clientId, string ipAddress, DateTime connectedDateTime, string address, MethodInfo method, SignalGo.Shared.Models.ParameterInfo[] parameters)
        {
            if (isStop)
                return null;
            HttpCallMethodLogInformation log = new HttpCallMethodLogInformation() { DateTimeStartMethod = DateTime.Now.ToLocalTime(), Method = method, Parameters = parameters, Address = address, ConnectedDateTime = connectedDateTime, IPAddress = ipAddress, ClientId = clientId, MethodName = method?.Name };
            Logs.Enqueue(log);
            return log;
        }

        public CallClientMethodLogInformation AddCallClientMethodLog(string clientId, string ipAddress, DateTime connectedDateTime, string serviceName, string methodName, SignalGo.Shared.Models.ParameterInfo[] parameters)
        {
            if (isStop)
                return null;
            CallClientMethodLogInformation log = new CallClientMethodLogInformation() { DateTimeStartMethod = DateTime.Now.ToLocalTime(), MethodName = methodName, Parameters = parameters, ServiceName = serviceName, ConnectedDateTime = connectedDateTime, IPAddress = ipAddress, ClientId = clientId };
            Logs.Enqueue(log);
            return log;
        }

        public StreamCallMethodLogInformation AddStreamCallMethodLog(string clientId, string ipAddress, DateTime connectedDateTime, string serviceName, string methodName, SignalGo.Shared.Models.ParameterInfo[] parameters)
        {
            if (isStop)
                return null;
            StreamCallMethodLogInformation log = new StreamCallMethodLogInformation() { DateTimeStartMethod = DateTime.Now.ToLocalTime(), MethodName = methodName, Parameters = parameters, ServiceName = serviceName, ConnectedDateTime = connectedDateTime, IPAddress = ipAddress, ClientId = clientId };
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

        private string CombinePath(params string[] pathes)
        {
            string result = pathes[0];
            foreach (string item in pathes.Skip(1))
            {
                result = System.IO.Path.Combine(result, item);
            }
            return result;
        }

        private void WriteToFile(CallMethodLogInformation log)
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
            foreach (ParameterInfo parameter in log.Method.GetParameters())
            {
                build.Append((isFirst ? "" : ",") + parameter.ParameterType.Name + " " + parameter.Name);
                isFirst = false;
            }
            build.AppendLine(")");

            build.AppendLine($"	With Values:");
            foreach (Shared.Models.ParameterInfo parameter in log.Parameters)
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
            using (FileStream stream = new System.IO.FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                stream.Seek(0, System.IO.SeekOrigin.End);
                byte[] bytes = Encoding.UTF8.GetBytes(build.ToString());
                stream.Write(bytes, 0, bytes.Length);
            }
#endif
        }

        private void WriteToFile(CallClientMethodLogInformation log)
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
            foreach (Shared.Models.ParameterInfo parameter in log.Parameters)
            {
                build.Append((isFirst ? "" : ",") + (parameter.Name == null ? "Null" : parameter.Name) + " obj" + index);
                isFirst = false;
                index++;
            }
            build.AppendLine(")");

            build.AppendLine($"	With Values:");
            foreach (Shared.Models.ParameterInfo parameter in log.Parameters)
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
            using (FileStream stream = new System.IO.FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                stream.Seek(0, System.IO.SeekOrigin.End);
                byte[] bytes = Encoding.UTF8.GetBytes(build.ToString());
                stream.Write(bytes, 0, bytes.Length);
            }
#endif
        }

        private void WriteToFile(HttpCallMethodLogInformation log)
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
            foreach (ParameterInfo parameter in log.Method.GetParameters())
            {
                build.Append((isFirst ? "" : ",") + parameter.ParameterType.Name + " " + parameter.Name);
                isFirst = false;
            }
            build.AppendLine(")");
            if (log.Parameters != null)
            {
                build.AppendLine($"	With Values:");
                foreach (Shared.Models.ParameterInfo value in log.Parameters)
                {
                    build.AppendLine("			" + (value == null || string.IsNullOrEmpty(value.Name) ? "Null" : value.Name));
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
            using (FileStream stream = new System.IO.FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                stream.Seek(0, System.IO.SeekOrigin.End);
                byte[] bytes = Encoding.UTF8.GetBytes(build.ToString());
                stream.Write(bytes, 0, bytes.Length);
            }
#endif
        }

        private void WriteToFile(StreamCallMethodLogInformation log)
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
                foreach (Shared.Models.ParameterInfo parameter in log.Parameters)
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
            using (FileStream stream = new System.IO.FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                stream.Seek(0, System.IO.SeekOrigin.End);
                byte[] bytes = Encoding.UTF8.GetBytes(build.ToString());
                stream.Write(bytes, 0, bytes.Length);
            }
#endif
        }

        private string GetDateTimeString(DateTime dt)
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
            foreach (BaseLogInformation item in Logs)
            {
                item.CanWriteToFile = true;
            }
            Logs.Enqueue(new CallMethodLogInformation() { CanWriteToFile = true });
        }
    }
}
