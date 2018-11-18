using System.Collections.Generic;
using System.Linq;

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
            var split = PropertyPath.Split('.');

            for (int i = 1; i < split.Length; i++)
            {
                var property = newPoint.GetType().GetProperties().FirstOrDefault(x => x.Name.Equals(split[i],System.StringComparison.OrdinalIgnoreCase));
                if (property == null)
                    throw new System.Exception($"I cannot find property {split[i]} in {PropertyPath} in object {newPoint.GetType().FullName}");
                newPoint = property.GetValue(newPoint);
            }
            return newPoint;
        }
    }
}
