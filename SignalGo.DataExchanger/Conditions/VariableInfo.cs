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
    public class VariableInfo : IAddConditionSides
    {
        public Dictionary<string, object> PublicVariables { get; set; } = new Dictionary<string, object>();
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
            var first = PublicVariables.First();
            PublicVariables[first.Key] = newPoint;
            var result = WhereInfo.Run(newPoint);
            foreach (var item in Children)
            {
                item.Run(result);
            }
            return result;
        }
    }
}
