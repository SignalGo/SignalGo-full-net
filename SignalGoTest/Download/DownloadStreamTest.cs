using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SignalGoTest.Download
{
    [TestClass]
    public class DownloadStreamTest
    {
        [TestMethod]
        public void TestDownload()
        {
            GlobalInitalization.Initialize();
            System.Threading.Thread.Sleep(1000 * 600);
            //var service = GlobalInitalization.GetStreamService();
            //var result = service.DownloadImage("hello world", new Models.TestStreamModel() { Name = "test name", Values = new System.Collections.Generic.List<string>() { "value test 1", "value test 2" } });
            //byte[] bytes = new byte[1024];
            //var readLen = result.Stream.Read(bytes, 0, bytes.Length);
            //System.Diagnostics.Trace.Assert(result.Data == "hello return" && readLen == 4 && bytes[0] == 2 && bytes[1] == 5 && bytes[2] == 8 && bytes[3] == 9);
        }
    }
}
