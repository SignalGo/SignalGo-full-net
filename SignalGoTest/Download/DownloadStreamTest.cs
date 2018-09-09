using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SignalGoTest.Download
{
    [TestClass]
    public class DownloadStreamTest
    {
        [TestMethod]
        public void TestDownload()
        {
            try
            {
                GlobalInitalization.Initialize();
                GlobalInitalization.InitializeAndConnecteClient();
                //System.Threading.Thread.Sleep(1000 * 600);
                SignalGoTestServices.StreamServices.ITestServerStreamModel service = GlobalInitalization.GetStreamService();
                SignalGo.Shared.Models.StreamInfo<string> result = service.DownloadImage("hello world", new ClientModels.TestStreamModel() { Name = "test name", Values = new System.Collections.Generic.List<string>() { "value test 1", "value test 2" } });
                byte[] bytes = result.Stream.Read(1024, out int readLen);
                System.Diagnostics.Trace.Assert(result.Data == "hello return" && readLen == 4 && bytes[0] == 2 && bytes[1] == 5 && bytes[2] == 8 && bytes[3] == 9);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.Assert(false);
            }
        }
    }
}
