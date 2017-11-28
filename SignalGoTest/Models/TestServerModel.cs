using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGoTest.Models
{
    public class TestServerModel : ITestServerModel
    {
        public Tuple<string> HelloWorld(string yourName)
        {
            return new Tuple<string>("hello: " + yourName);
        }

        public Tuple<bool> Logout(string yourName)
        {
            if (yourName == "hello")
                return new Tuple<bool>(false);
            return new Tuple<bool>(false);
        }
    }
}
