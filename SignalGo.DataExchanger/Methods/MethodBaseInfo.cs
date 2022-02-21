using SignalGo.DataExchanger.Conditions;
using System;
using System.Collections.Generic;

namespace SignalGo.DataExchanger.Methods
{
    /// <summary>
    /// base of all methods needs
    /// </summary>
    public abstract class MethodBaseInfo : IAddConditionSides
    {
        /// <summary>
        /// parameters of method
        /// </summary>
        public List<IRunnable> Parameters { get; set; } = new List<IRunnable>();

        public IAddConditionSides Parent { get; set; }

        public bool IsComplete { get; set; }

        public Dictionary<string, object> PublicVariables { get; set; }

        public IAddConditionSides Add(IRunnable runnable)
        {
            Parameters.Add(runnable);
            return this;
        }

        public IAddConditionSides Add(IAddConditionSides runnable)
        {
            throw new NotImplementedException();
        }

        public IAddConditionSides Add()
        {
            return null;
        }

        /// <summary>
        /// add runnable to method and return double conditions side, parent and self 
        /// </summary>
        /// <param name="runnable"></param>
        /// <returns></returns>
        public Tuple<IAddConditionSides, IAddConditionSides> AddDouble(IAddConditionSides runnable)
        {
            MethodBaseInfo instance = (MethodBaseInfo)Activator.CreateInstance(GetType());
            instance.Parent = runnable;
            instance.PublicVariables = runnable.PublicVariables;
            return new Tuple<IAddConditionSides, IAddConditionSides>(runnable.Add((IRunnable)instance), instance);
        }

        public void ChangeOperatorType(OperatorType operatorType)
        {
            throw new NotImplementedException();
        }

        public abstract object Run(object newPoint);
    }
}
