using SignalGo.DataExchanger.Conditions;
using System;
using System.Collections;
using System.Linq;

namespace SignalGo.DataExchanger.Methods.Query
{
    /// <summary>
    /// calculate sum of parameters
    /// </summary>
    public class SumMethodInfo : MethodBaseInfo
    {
        public override object Run(object newPoint)
        {
            double value = 0;
            foreach (IRunnable item in Parameters)
            {
                value += (double)OperatorInfo.ConvertType(typeof(double), item.Run(newPoint));
            }
            return value;
        }
    }
}
