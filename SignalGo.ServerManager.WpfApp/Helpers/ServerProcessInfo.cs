using SignalGo.ServiceManager.Core.Models;
using SignalGo.Shared.Log;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace SignalGo.ServerManager.WpfApp.Helpers
{
    public class ServerProcessInfo : ServerProcessBaseInfo
    {
        /// <summary>
        /// buffer
        /// </summary>
        public const int BUFFER_SIZE = 1024 * 5;

        private string m_PipeID;
        private NamedPipeServerStream m_PipeServerStream;
        private bool IsDisposing { get; set; }
        private Thread m_PipeMessagingThread;

        /// <summary>
        /// ServerProcessInfoBase Constructor
        /// </summary>
        public ServerProcessInfo() { }

        /// <summary>
        /// Starts the IPC server and run the child process
        /// </summary>
        /// <param name="paramUID">Unique ID of the named pipe</param>
        /// <returns></returns>
        public override void Start(string paramUID, string fileName,string shell ="cmd")
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException("we can't find service executable file. please verify the service path");
            m_PipeID = paramUID;

            m_PipeMessagingThread = new Thread(new ThreadStart(StartIPCServer));
            m_PipeMessagingThread.Name = this.GetType().Name + ".PipeMessagingThread";
            m_PipeMessagingThread.IsBackground = true;
            m_PipeMessagingThread.Start();

            ProcessStartInfo processInfo = new ProcessStartInfo(fileName, this.m_PipeID);
            //processInfo.CreateNoWindow = false;
            //processInfo.UseShellExecute = true;
            BaseProcess = Process.Start(processInfo);
            try
            {
                BaseProcess.WaitForInputIdle();
            }
            catch (Exception ex)
            {
                Thread.Sleep(500);
            }
        }

        /// <summary>
        ///  Send message to the child process
        /// </summary>
        /// <param name="paramData"></param>
        /// <returns></returns>
        public bool Send(string paramData)
        {

            return true;
        }

        /// <summary>
        /// Start the IPC server listener and wait for
        /// incomming messages from the appropriate child process
        /// </summary>
        void StartIPCServer()
        {
            if (m_PipeServerStream == null)
            {
                m_PipeServerStream = new NamedPipeServerStream(
                    m_PipeID,
                    PipeDirection.InOut,
                    1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous,
                    BUFFER_SIZE,
                    BUFFER_SIZE);

            }

            // Wait for a client to connect
            try
            {
                //Wait for connection from the child process
                m_PipeServerStream.WaitForConnection();
            }
            catch (ObjectDisposedException exDisposed)
            {
                AutoLogger.Default.LogError(exDisposed, string.Format("StartIPCServer for process {0}", this.m_PipeID));
            }
            catch (IOException exIO)
            {
                AutoLogger.Default.LogError(exIO, string.Format("StartIPCServer for process {0}", this.m_PipeID));
            }

            while (!IsDisposing && StartAsyncReceive())
            {
            }
            Dispose();
        }

        /// <summary>
        /// Read line of text from the connected client
        /// </summary>
        /// <returns>return false on pipe communication exception</returns>
        bool StartAsyncReceive()
        {
            try
            {
                byte[] bytes = new byte[BUFFER_SIZE];
                int readCount = m_PipeServerStream.Read(bytes, 0, bytes.Length);

                if (readCount == 0)
                {
                    // The client is down
                    return false;
                }

            }
            // Catch the IOException that is raised if the pipe is broken
            // or disconnected.
            catch (Exception e)
            {
                AutoLogger.Default.LogError(e, "AsyncReceive ERROR:");
                return false;
            }

            return true;

        }

        /// <summary>
        /// Dispose the client process
        /// </summary>
        void DisposeClientProcess()
        {
            try
            {
                IsDisposing = true;

                try
                {
                    //I will fails if the process doesn't exist
                    if (BaseProcess != null)
                        BaseProcess.Kill();
                }
                catch
                { }
                if (m_PipeServerStream != null)
                    m_PipeServerStream.Dispose();//This will stop any pipe activity

                AutoLogger.Default.LogText(string.Format("Process {0} is Closed", m_PipeID));
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, string.Format("Process {0} is Close", this.m_PipeID));
            }

        }

        #region IDisposable Members

        public override void Dispose()
        {
            DisposeClientProcess();
        }

        #endregion
    }
}
