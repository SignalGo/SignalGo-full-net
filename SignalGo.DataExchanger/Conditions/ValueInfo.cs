using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.DataExchanger.Conditions
{
    /// <summary>
    /// direct value like string int etc
    /// </summary>
    public class ValueInfo : IRunnable
    {
        public Dictionary<string, object> PublicVariables { get; set; }
        public string Value { get; set; }
        public object Run(object newPoint)
        {
            return Value;
        }
    }
}
