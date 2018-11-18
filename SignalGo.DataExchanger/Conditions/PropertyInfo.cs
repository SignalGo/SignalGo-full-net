using System.Collections.Generic;

namespace SignalGo.DataExchanger.Conditions
{
    /// <summary>
    /// a condition to check left and right side data
    /// </summary>
    public class PropertyInfo : IRunnable
    {
        public Dictionary<string, object> PublicVariables { get; set; }
        public string PropertyPath { get; set; }
        public object Run(object newPoint)
        {
            return newPoint;
        }
    }
}
