using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Shared.Models
{
    public class CallbackServiceLogInfo
    {
        public string ServiceName { get; set; }
#if (!NET35)
        public ObservableCollection<CallbackMethodLogInfo> Calls { get; set; } = new ObservableCollection<CallbackMethodLogInfo>();
#endif
    }

    public class CallbackMethodLogInfo
    {
        public string MethodName { get; set; }
        public DateTime DateTime { get; set; }
        public List<CallbackParameterLogInfo> Parameters { get; set; } = new List<CallbackParameterLogInfo>();
    }

    /// <summary>
    /// details of parameter
    /// </summary>
    public class CallbackParameterLogInfo
    {
        /// <summary>
        /// name of parameter
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// value of parameter
        /// </summary>
        public object Value { get; set; }
        /// <summary>
        /// parameter type
        /// </summary>
        public string ParameterType { get; set; }
    }
}
