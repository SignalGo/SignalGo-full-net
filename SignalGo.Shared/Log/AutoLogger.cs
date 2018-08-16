using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Shared.Log
{
    /// <summary>
    /// log exceptions and texts to a file
    /// </summary>
    public class AutoLogger
    {
        public static AutoLogger Default { get; set; } = new AutoLogger() { DirectoryName = "", FileName = "App Logs.log" };
        /// <summary>
        /// is enabled log system
        /// </summary>
        public static bool IsEnabled { get; set; } = true;
        /// <summary>
        /// full path of log
        /// </summary>
        public static string DirectoryLocation { get; set; }
        /// <summary>
        /// directory name of log
        /// </summary>
        public string DirectoryName { get; set; }
        /// <summary>
        /// file name to save
        /// </summary>
        public string FileName { get; set; }

        string SavePath
        {
            get
            {
                string dir = "";
                if (string.IsNullOrEmpty(DirectoryName))
                    dir = DirectoryLocation;
                else
                    dir = Path.Combine(DirectoryLocation, DirectoryName);
#if (!PORTABLE)
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
#endif
                return Path.Combine(dir, FileName);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public AutoLogger()
        {
#if (!PORTABLE)
            try
            {
#if (NETSTANDARD1_6)
                DirectoryLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
#else
                if (!string.IsNullOrEmpty(AppDomain.CurrentDomain.BaseDirectory))
                    DirectoryLocation = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar);
#endif
                if (!string.IsNullOrEmpty(DirectoryLocation) && !Directory.Exists(DirectoryLocation))
                    Directory.CreateDirectory(DirectoryLocation);
                //Console.WriteLine("log location:" + DirectoryLocation);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            DirectoryName = "SignalGoDiagnostic";
            FileName = "SignalGo Logs.log";
#endif
        }
#if (!PORTABLE)

        void GetOneStackTraceText(StackTrace stackTrace, StringBuilder builder)
        {
            builder.AppendLine("<------------------------------StackTrace One Begin------------------------------>");

            StackFrame[] stackFrames = stackTrace.GetFrames();  // get method calls (frames)

            // write call stack method names
            foreach (StackFrame stackFrame in stackFrames)
            {
                var method = stackFrame.GetMethod();

                builder.AppendLine("<---Method Begin--->");
                builder.AppendLine("File Name: " + stackFrame.GetFileName());
                builder.AppendLine("Line Number: " + stackFrame.GetFileLineNumber());
                builder.AppendLine("Column Number: " + stackFrame.GetFileColumnNumber());



                builder.AppendLine("Name: " + method.Name);
                builder.AppendLine("Class: " + method.DeclaringType.Name);
                var param = method.GetParameters();
                builder.AppendLine("Params Count: " + param.Length);
                int i = 1;
                foreach (var p in param)
                {
                    builder.AppendLine("Param " + i + ":" + p.ParameterType.Name);
                    i++;
                }
                builder.AppendLine("<---Method End--->");
            }
            builder.AppendLine("<------------------------------StackTrace One End------------------------------>");
        }
#endif
        readonly object _lockOBJ = new object();
        /// <summary>
        /// log text message
        /// </summary>
        /// <param name="text">text to log</param>
        /// <param name="stacktrace">log stacktrace</param>
        public void LogText(string text, bool stacktrace = false)
        {
            if (string.IsNullOrEmpty(DirectoryLocation) || !IsEnabled)
            {
                return;
            }
            AsyncActions.Run(() =>
            {
                StringBuilder str = new StringBuilder();
                str.AppendLine("<Text Log Start>");
                str.AppendLine(text);
                if (stacktrace)
                {
                    str.AppendLine("<StackTrace>");
                    StringBuilder builder = new StringBuilder();
#if (NETSTANDARD || NETCOREAPP)
                                                                GetOneStackTraceText(new StackTrace(new Exception(text), true), builder);
#else
                    GetOneStackTraceText(new StackTrace(true), builder);
#endif
                    str.AppendLine(builder.ToString());

                    str.AppendLine("</StackTrace>");
                }
                str.AppendLine("<Text Log End>");

                string fileName = SavePath;
                try
                {
                    lock (_lockOBJ)
                    {
                        using (var stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                        {
                            stream.Seek(0, SeekOrigin.End);
                            byte[] bytes = Encoding.UTF8.GetBytes(System.Environment.NewLine + str.ToString());
                            stream.Write(bytes, 0, bytes.Length);
                        }
                    }
                }
                catch
                {

                }
            });
        }


        /// <summary>
        /// log an exception to file
        /// </summary>
        /// <param name="e">exception of log</param>
        /// <param name="title">title of log</param>
        public void LogError(Exception e, string title)
        {
            if (string.IsNullOrEmpty(DirectoryLocation) || !IsEnabled)
                return;
#if (NET35 || NET40)
            AsyncActions.Run(() =>
#else
            AsyncActions.Run(() =>
#endif
            {
                try
                {
                    StringBuilder str = new StringBuilder();
                    str.AppendLine(title);
                    str.AppendLine(e.ToString());
                    str.AppendLine("Time : " + DateTime.Now.ToLocalTime().ToString());
                    str.AppendLine("--------------------------------------------------------------------------------------------------");
                    str.AppendLine("--------------------------------------------------------------------------------------------------");
                    string fileName = SavePath;

                    try
                    {
                        lock (_lockOBJ)
                        {
                            using (FileStream stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                            {
                                stream.Seek(0, SeekOrigin.End);
                                byte[] bytes = Encoding.UTF8.GetBytes(System.Environment.NewLine + str.ToString());
                                stream.Write(bytes, 0, bytes.Length);
                            }
                        }
                    }
                    catch
                    {

                    }
                }
                catch
                {

                }
            });
        }
    }
}
