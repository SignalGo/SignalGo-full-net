using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.DataExchanger.Conditions
{
    /// <summary>
    /// runnable interface can run on compiler
    /// </summary>
    public interface IRunnable
    {
        Dictionary<string, object> PublicVariables { get; set; }
        object Run(object newPoint);
    }
}
