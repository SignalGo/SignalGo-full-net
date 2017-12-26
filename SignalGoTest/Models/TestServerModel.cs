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
        public string HelloWorld(string yourName)
        {
            OperationContext<TestSetting>.CurrentSetting = new TestSetting() { Name = yourName };
            return "hello: " + yourName;
        }

        public bool Logout(string yourName)
        {
            throw new NotImplementedException();
        }

        public string WhoAmI()
        {
            return "you are : " + OperationContext<TestSetting>.CurrentSetting.Name;
        }

        public int MUL(int x, int y)
        {
            return x * y;
        }

        public double Tagh(double x, double y)
        {
            return x / y;
        }

        public TimeSpan TimeS(int x)
        {
            return new TimeSpan(x);
        }

        public long LongValue()
        {
            return long.MaxValue;
        }

        Tuple<bool> ITestServerModelBase.Logout(string yourName)
        {
            throw new NotImplementedException();
        }
    }
}
