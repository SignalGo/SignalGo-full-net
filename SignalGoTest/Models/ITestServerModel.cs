using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGoTest.Models
{
    public interface ITestServerModelBase
    {
        Tuple<bool> Logout(string yourName);
    }

    [SignalGo.Shared.DataTypes.ServiceContract("TestServerModel", SignalGo.Shared.DataTypes.InstanceType.SingleInstance)]
    public interface ITestServerModel : ITestServerModelBase
    {
        string HelloWorld(string yourName);
        string WhoAmI();
        int MUL(int x, int y);
        double Tagh(double x, double y);
        System.TimeSpan TimeS (int x);
        long LongValue ();
    }
}
