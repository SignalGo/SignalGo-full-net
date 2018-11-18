using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.DataExchanger.Conditions
{
    /// <summary>
    /// variable info like x,y etc
    /// </summary>
    public class VariableInfo : IRunnable
    {
        public Dictionary<string, object> PublicVariables { get; set; } = new Dictionary<string, object>();
        /// <summary>
        /// where inside of variable
        /// </summary>
        public WhereInfo WhereInfo { get; set; }

        /// <summary>
        /// run variable wheres
        /// </summary>
        /// <param name="newPoint"></param>
        /// <returns></returns>
        public object Run(object newPoint)
        {
            var first = PublicVariables.First();
            PublicVariables[first.Key] = newPoint;
            return WhereInfo.Run(newPoint);
        }
    }
}
