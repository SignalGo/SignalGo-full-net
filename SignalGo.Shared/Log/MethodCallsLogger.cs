using Newtonsoft.Json;
using SignalGo.Shared.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace SignalGo.Shared.Log
{
    public abstract class BaseLogInformation
    {
        public DateTime DateTime { get; set; }
        public DateTime ResultDateTime { get; set; }
        /// <summary>
        /// after set call method result is going to true and write to log file
        /// </summary>
        public bool CanWriteToFile { get; set; }
        public string SessionId { get; set; }
        public DateTime ConnectedDateTime { get; set; }
        public string IPAddress { get; set; }
        public object Result { get; set; }
        public string MethodName { get; set; }
    }

    public class CallMethodLogInformation : BaseLogInformation
    {
        public string ServiceName { get; set; }
        public List<Models.ParameterInfo> Parameters { get; set; }
        public MethodInfo Method { get; set; }
    }

    public class CallClientMethodLogInformation : BaseLogInformation
    {
        public string ServiceName { get; set; }
        public List<Models.ParameterInfo> Parameters { get; set; }
    }

    public class HttpCallMethodLogInformation : BaseLogInformation
    {
        public string Address { get; set; }
        public List<string> Parameters { get; set; }
        public MethodInfo Method { get; set; }
    }


    public static class MethodCallsLogger
    {
        /// <summary>
        /// if false ignore write errors to .log file
        /// </summary>
        public static bool IsEnabled
        {
            set
            {
                StopEngine();
                if (value)
                    StartEngine();
            }
        }

        public static bool IsStart
        {
            get
            {
                return !isStop;
            }
        }

        public static bool IsPersianDateLog { get; set; } = false;

        static bool isStop = true;
        static void StopEngine()
        {
            if (isStop)
                return;
            waitForDispose.Reset();
            waitForDispose.WaitOne();
        }

        static ManualResetEvent waitForDispose = new ManualResetEvent(false);

        static void StartEngine()
        {
            if (!isStop)
                return;
            isStop = false;
            Thread _thread = new Thread(() =>
            {
                try
                {
                    BaseLogInformation nextLog = null;
                    while (!isStop)
                    {
                        if (Logs.TryPeek(out nextLog) && nextLog.CanWriteToFile)
                        {
                            if (Logs.TryDequeue(out nextLog))
                            {
                                if (nextLog is CallMethodLogInformation)
                                    WriteToFile((CallMethodLogInformation)nextLog);
                                else if (nextLog is HttpCallMethodLogInformation)
                                    WriteToFile((HttpCallMethodLogInformation)nextLog);
                                else if (nextLog is CallClientMethodLogInformation)
                                    WriteToFile((CallClientMethodLogInformation)nextLog);
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
                waitForDispose.Set();
                isStop = true;
            });
            _thread.IsBackground = true;
            _thread.Start();
        }

        static ConcurrentQueue<BaseLogInformation> Logs = new ConcurrentQueue<BaseLogInformation>();

        public static CallMethodLogInformation AddCallMethodLog(string sessionId, string ipAddress, DateTime connectedDateTime, string serviceName, MethodInfo method, List<Models.ParameterInfo> parameters)
        {
            if (isStop)
                return null;
            var log = new CallMethodLogInformation() { DateTime = DateTime.Now.ToLocalTime(), Method = method, Parameters = parameters, ServiceName = serviceName, ConnectedDateTime = connectedDateTime, IPAddress = ipAddress, SessionId = sessionId };
            Logs.Enqueue(log);
            return log;
        }

        public static HttpCallMethodLogInformation AddHttpMethodLog(string sessionId, string ipAddress, DateTime connectedDateTime, string address, MethodInfo method, List<string> parameters)
        {
            if (isStop)
                return null;
            var log = new HttpCallMethodLogInformation() { DateTime = DateTime.Now.ToLocalTime(), Method = method, Parameters = parameters, Address = address, ConnectedDateTime = connectedDateTime, IPAddress = ipAddress, SessionId = sessionId };
            Logs.Enqueue(log);
            return log;
        }

        public static CallClientMethodLogInformation AddCallClientMethodLog(string sessionId, string ipAddress, DateTime connectedDateTime, string serviceName, string methodName, List<Models.ParameterInfo> parameters)
        {
            if (isStop)
                return null;
            var log = new CallClientMethodLogInformation() { DateTime = DateTime.Now.ToLocalTime(), MethodName = methodName, Parameters = parameters, ServiceName = serviceName, ConnectedDateTime = connectedDateTime, IPAddress = ipAddress, SessionId = sessionId };
            Logs.Enqueue(log);
            return log;
        }

        public static void FinishLog(CallMethodLogInformation log, object result)
        {
            if (isStop)
                return;
            if (log == null)
                return;
            log.ResultDateTime = DateTime.Now.ToLocalTime();
            log.Result = result;
            log.CanWriteToFile = true;
        }

        public static void FinishLog(HttpCallMethodLogInformation log, object result)
        {
            if (isStop)
                return;
            if (log == null)
                return;
            log.ResultDateTime = DateTime.Now.ToLocalTime();
            log.Result = result;
            log.CanWriteToFile = true;
        }

        public static void FinishLog(CallClientMethodLogInformation log, object result)
        {
            if (isStop)
                return;
            if (log == null)
                return;
            log.ResultDateTime = DateTime.Now.ToLocalTime();
            log.Result = result;
            log.CanWriteToFile = true;
        }

        static string CombinePath(params string[] pathes)
        {
            string result = pathes[0];
            foreach (var item in pathes.Skip(1))
            {
                result = System.IO.Path.Combine(result, item);
            }
            return result;
        }

        static void WriteToFile(CallMethodLogInformation log)
        {
#if(NET35)
            string path = CombinePath(AutoLogger.ApplicationDirectory, "Logs", log.DateTime.Year.ToString(), log.DateTime.Month.ToString(), log.DateTime.Day.ToString());
#else
            string path = System.IO.Path.Combine(AutoLogger.ApplicationDirectory, "Logs", log.DateTime.Year.ToString(), log.DateTime.Month.ToString(), log.DateTime.Day.ToString());
#endif
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
            path = System.IO.Path.Combine(path, $"{log.DateTime.Year}-{log.DateTime.Month}-{log.DateTime.Day} {log.DateTime.ToLocalTime().Hour}.log");

            StringBuilder build = new StringBuilder();
            build.AppendLine("########################################");
            build.AppendLine("Client Information:");
            build.AppendLine($"	Ip Address:	{log.IPAddress}");
            build.AppendLine($"	SessionId:	{log.SessionId}");
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
            build.AppendLine($"Result Information:");
            build.AppendLine("			" + (log.Result == null ? "Null" : JsonConvert.SerializeObject(log.Result, Formatting.None, new JsonSerializerSettings() { Formatting = Formatting.None })).Replace(@"\""", ""));
            build.AppendLine("");
            build.AppendLine($"Invoked Time:");
            build.AppendLine($"			{GetDateTimeString(log.DateTime)}");
            build.AppendLine($"Result Time:");
            build.AppendLine($"			{GetDateTimeString(log.ResultDateTime)}");
            build.AppendLine("----------------------------------------------------------------------------------------");
            build.AppendLine("");
            using (var stream = new System.IO.FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                stream.Seek(0, System.IO.SeekOrigin.End);
                byte[] bytes = Encoding.UTF8.GetBytes(build.ToString());
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        static void WriteToFile(CallClientMethodLogInformation log)
        {
#if (NET35)
            string path = CombinePath(AutoLogger.ApplicationDirectory, "Logs", log.DateTime.Year.ToString(), log.DateTime.Month.ToString(), log.DateTime.Day.ToString());
#else
            string path = System.IO.Path.Combine(AutoLogger.ApplicationDirectory, "Logs", log.DateTime.Year.ToString(), log.DateTime.Month.ToString(), log.DateTime.Day.ToString());
#endif
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
            path = System.IO.Path.Combine(path, $"Callback-{log.DateTime.Year}-{log.DateTime.Month}-{log.DateTime.Day} {log.DateTime.ToLocalTime().Hour}.log");

            StringBuilder build = new StringBuilder();
            build.AppendLine("########################################");
            build.AppendLine("Client Information:");
            build.AppendLine($"	Ip Address:	{log.IPAddress}");
            build.AppendLine($"	SessionId:	{log.SessionId}");
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
            build.AppendLine($"Result Information:");
            build.AppendLine("			" + (log.Result == null ? "Null" : JsonConvert.SerializeObject(log.Result, Formatting.None, new JsonSerializerSettings() { Formatting = Formatting.None })).Replace(@"\""", ""));
            build.AppendLine("");
            build.AppendLine($"Invoked Time:");
            build.AppendLine($"			{GetDateTimeString(log.DateTime)}");
            build.AppendLine($"Result Time:");
            build.AppendLine($"			{GetDateTimeString(log.ResultDateTime)}");
            build.AppendLine("----------------------------------------------------------------------------------------");
            build.AppendLine("");
            using (var stream = new System.IO.FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                stream.Seek(0, System.IO.SeekOrigin.End);
                byte[] bytes = Encoding.UTF8.GetBytes(build.ToString());
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        static void WriteToFile(HttpCallMethodLogInformation log)
        {
#if (NET35)
            string path = CombinePath(AutoLogger.ApplicationDirectory, "Logs", log.DateTime.Year.ToString(), log.DateTime.Month.ToString(), log.DateTime.Day.ToString());
#else
            string path = System.IO.Path.Combine(AutoLogger.ApplicationDirectory, "Logs", log.DateTime.Year.ToString(), log.DateTime.Month.ToString(), log.DateTime.Day.ToString());
#endif
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
            path = System.IO.Path.Combine(path, $"HTTP-{log.DateTime.Year}-{log.DateTime.Month}-{log.DateTime.Day} {log.DateTime.ToLocalTime().Hour}.log");

            StringBuilder build = new StringBuilder();
            build.AppendLine("########################################");
            build.AppendLine("Client Information:");
            build.AppendLine($"	Ip Address:	{log.IPAddress}");
            build.AppendLine($"	SessionId:	{log.SessionId}");
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

            build.AppendLine($"	With Values:");
            foreach (var value in log.Parameters)
            {
                build.AppendLine("			" + (value == null ? "Null" : value));
            }
            build.AppendLine("");
            build.AppendLine($"Result Information:");
            build.AppendLine("			" + (log.Result == null ? "Null" : JsonConvert.SerializeObject(log.Result, Formatting.None, new JsonSerializerSettings() { Formatting = Formatting.None })).Replace(@"\""", ""));
            build.AppendLine("");
            build.AppendLine("			" + (log.Result == null ? "Null" : log.Result.GetType().FullName));
            build.AppendLine("");
            build.AppendLine($"Invoked Time:");
            build.AppendLine($"			{GetDateTimeString(log.DateTime)}");
            build.AppendLine($"Result Time:");
            build.AppendLine($"			{GetDateTimeString(log.ResultDateTime)}");
            build.AppendLine("----------------------------------------------------------------------------------------");
            build.AppendLine("");
            using (var stream = new System.IO.FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                stream.Seek(0, System.IO.SeekOrigin.End);
                byte[] bytes = Encoding.UTF8.GetBytes(build.ToString());
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        static string GetDateTimeString(DateTime dt)
        {
            PersianCalendar persian = new PersianCalendar();
            string log = "";
            if (IsPersianDateLog)
                log = $"{persian.GetYear(dt)}/{persian.GetMonth(dt)}/{persian.GetDayOfMonth(dt)} {persian.GetHour(dt)}:{persian.GetMinute(dt)}:{persian.GetSecond(dt)}.{persian.GetMilliseconds(dt)} ## ";
            return log + $"{dt.ToString("MM/dd/yyyy HH:mm:ss")}.{dt.Millisecond}";
        }
    }
}
