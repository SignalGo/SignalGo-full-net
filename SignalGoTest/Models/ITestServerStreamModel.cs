using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGoTest.Models
{
    public class TestStreamModel
    {
        public string Name { get; set; }
        public List<string> Values { get; set; }

    }

    [ServiceContract("TestServerStreamModel")]
    public interface ITestServerStreamModel
    {
        StreamInfo<string> DownloadImage(string name, TestStreamModel testStreamModel);
    }
}
