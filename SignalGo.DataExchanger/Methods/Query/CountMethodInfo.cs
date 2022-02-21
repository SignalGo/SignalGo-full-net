using SignalGo.DataExchanger.Conditions;
using System;
using System.Collections;
using System.Linq;

namespace SignalGo.DataExchanger.Methods.Query
{
    /// <summary>
    /// calculate count of parameters
    /// </summary>
    public class CountMethodInfo : MethodBaseInfo
    {
        public override object Run(object newPoint)
        {
            IRunnable meparameter = Parameters.FirstOrDefault();
            object result = meparameter.Run(newPoint);
            if (result == null)
                return null;
            if (result is ICollection collection)
            {
                return collection.Count;
            }
            else if (result is IEnumerable enumerable)
            {
                System.Reflection.PropertyInfo countProperty = result.GetType().GetProperty("Count");
                if (countProperty != null)
                    return countProperty.GetValue(result);
                else
                {
                    countProperty = result.GetType().GetProperty("Length");
                    if (countProperty != null)
                        return countProperty.GetValue(result);
                    else
                    {
                        return enumerable.Cast<object>().Count();
                    }
                }
            }
            else
                throw new Exception("you are trying to get count of a property but that propery is not ienumerable");
        }
    }
}
