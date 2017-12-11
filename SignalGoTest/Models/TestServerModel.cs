using SignalGo.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGoTest.Models
{
    public class TestSetting
    {
        public string Name { get; set; }
    }

    public class TestServerModel : ITestServerModel
    {
        public Tuple<string> HelloWorld(string yourName)
        {
            OperationContext<TestSetting>.CurrentSetting = new TestSetting() { Name = yourName };

            return new Tuple<string>("hello: " + yourName);
        }

        public Tuple<bool> Logout(string yourName)
        {
            throw new NotImplementedException();
        }

        public Tuple<string> WhoAmI()
        {
            return new Tuple<string>("you are : " + OperationContext<TestSetting>.CurrentSetting.Name);
        }
    }
}
