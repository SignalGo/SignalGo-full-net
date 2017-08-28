using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.Log
{
    /// <summary>
    /// log exceptions and text to log file
    /// </summary>
    public static class AutoLogger
    {
        /// <summary>
        /// if false ignore write errors to .log file
        /// </summary>
        public static bool IsEnabled { get; set; } = true;
        
        static AutoLogger()
        {
            try
            {
                ApplicationDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            }
            catch
            {

            }
        }

        static void GetOneStackTraceText(StackTrace stackTrace, StringBuilder builder)
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
        

        static object lockOBJ = new object();
        public static void LogText(string text, bool stacktrace = false)
        {
            if (!IsEnabled)
                return;
            StringBuilder str = new StringBuilder();
            str.AppendLine("<Text Log Start>");
            str.AppendLine(text);
            if (stacktrace)
            {
                str.AppendLine("<StackTrace>");
                StringBuilder builder = new StringBuilder();
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                GetOneStackTraceText(new StackTrace(new Exception(text), true), builder);
#else
                GetOneStackTraceText(new StackTrace(true), builder);
#endif
                str.AppendLine(builder.ToString());

                str.AppendLine("</StackTrace>");
            }
            //str.AppendLine(GetFullStack());
            str.AppendLine("<Text Log End>");

            string fileName = Path.Combine(ApplicationDirectory, "SignalGo Logs.log");
            try
            {
                lock (lockOBJ)
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
        }

        public static string ApplicationDirectory { get; set; }

        public static void LogError(Exception e, string title)
        {
            if (!IsEnabled)
                return;
            try
            {
                //#if (!DEBUG)
                //                if (!canSave && !ForceLog)
                //                    return;
                //#endif
                StringBuilder str = new StringBuilder();
                str.AppendLine(title);
                str.AppendLine(e.ToString());
                //if (logFullStack)
                //    str.AppendLine(GetFullStack());
                str.AppendLine("Time : " + DateTime.Now.ToLocalTime().ToString());
                str.AppendLine("--------------------------------------------------------------------------------------------------");
                str.AppendLine("--------------------------------------------------------------------------------------------------");
                string fileName = Path.Combine(ApplicationDirectory, "SignalGo Logs.log");

                try
                {
                    lock (lockOBJ)
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
        }

        //static string GetAllInner(Exception e)
        //{
        //    string msg = "Start Exception:" + System.Environment.NewLine;
        //    while (e != null)
        //    {
        //        msg += "<---------Start One--------->" + System.Environment.NewLine + GetTextMessageFromException(e) + System.Environment.NewLine + "<---------End One--------->" + System.Environment.NewLine;
        //        e = e.InnerException;
        //    }
        //    return msg + "End Exception";
        //}

        //static string GetTextMessageFromException(Exception e)
        //{
        //    if (e == null)
        //        return "No Exception";
        //    else
        //    {
        //        if (e.Message == null)
        //        {
        //            if (e.StackTrace == null)
        //            {
        //                return "Null Error";
        //            }
        //            else
        //            {
        //                return "FaghatStack : " + e.StackTrace;
        //            }
        //        }
        //        else
        //        {
        //            if (e.StackTrace == null)
        //            {
        //                return "Stack Is Null But Message: " + e.Message;
        //            }
        //            else
        //            {
        //                return "*Message: " + e.Message + System.Environment.NewLine + "*Stack: " + e.StackTrace;
        //            }
        //        }
        //    }
        //}
    }
}
