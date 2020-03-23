using NUnit.Framework;
using SignalGo.Server.ServiceManager;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Server.Tests
{
    public class SimpleServer
    {
        [Test]
        public async Task RunServer()
        {
            try
            {
                ServerProvider serverProvider = new ServerProvider();
                serverProvider.Start("http://localhost:4545");
                await Task.Delay(10000 * 1000);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
            }
        }
    }
}
