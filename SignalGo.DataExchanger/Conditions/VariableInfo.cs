using SignalGo.DataExchanger.Compilers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SignalGo.DataExchanger.Conditions
{
    /// <summary>
    /// variable info like x,y etc
    /// </summary>
    public class VariableInfo : IAddConditionSides
    {
        public Dictionary<string, object> PublicVariables { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, string> PublicVariablePropertyNames { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// where inside of variable
        /// </summary>
        public WhereInfo WhereInfo { get; set; }
        /// <summary>
        /// childs variable
        /// </summary>
        public List<IRunnable> Children { get; set; } = new List<IRunnable>();

        public IAddConditionSides Parent { get; set; }

        public bool IsComplete { get; set; }

        public IAddConditionSides Add(IAddConditionSides runnable)
        {
            Children.Add(runnable);
            return this;
        }

        public IAddConditionSides Add(IRunnable runnable)
        {
            Children.Add(runnable);
            return this;
        }

        public IAddConditionSides Add()
        {
            throw new NotImplementedException();
        }

        public Tuple<IAddConditionSides, IAddConditionSides> AddDouble(IAddConditionSides runnable)
        {
            throw new NotImplementedException();
        }

        public void ChangeOperatorType(OperatorType operatorType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// run variable wheres
        /// </summary>
        /// <param name="newPoint"></param>
        /// <returns></returns>
        public object Run(object newPoint)
        {
            KeyValuePair<string, object> first = PublicVariables.First();
            PublicVariables[first.Key] = newPoint;
            object result = WhereInfo.Run(newPoint);
            foreach (VariableInfo variableInfo in Children)
            {
                object variableValue = null;
                System.Reflection.PropertyInfo property = null;
                foreach (KeyValuePair<string, object> variable in variableInfo.PublicVariables)
                {
                    if (variableInfo.PublicVariablePropertyNames.TryGetValue(variable.Key, out string name))
                    {
                        string[] nameValue = name.Split('.');
                        if (first.Key.Equals(nameValue[0], StringComparison.OrdinalIgnoreCase))
                        {
                            property = newPoint.GetType().GetProperties().FirstOrDefault(x => x.Name.Equals(nameValue.Last(), System.StringComparison.OrdinalIgnoreCase));
                            if (property != null)
                            {
                                object value = property.GetValue(newPoint);
                                variableValue = value;
                                break;
                            }
                        }
                    }
                }
                if (variableValue is IEnumerable enumerable)
                {
                    Type[] generics = property.PropertyType.GetGenericArguments();
                    if (generics.Length > 0)
                    {
                        var method = typeof(ConditionsCompiler).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).Where(x => x.Name == "GenerateArrayObject" && x.GetGenericArguments().Length > 0).FirstOrDefault();
                        method = method.MakeGenericMethod(generics[0]);
                        var value = method.Invoke(null, new object[] { variableValue, variableInfo });
                        value = typeof(Enumerable).GetMethod("ToList", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(generics[0]).Invoke(null, new object[] { value });
                        property.SetValue(newPoint, value);
                    }
                    else
                    {
                        var value = ConditionsCompiler.GenerateArrayObject(variableValue, variableInfo);
                        property.SetValue(newPoint, value);
                    }
                }
                else
                {
                    object value = variableInfo.Run(variableValue);
                }

            }
            return result;
        }
    }
}
