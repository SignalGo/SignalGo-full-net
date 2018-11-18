using System;
using System.Collections.Generic;
using System.Linq;

namespace SignalGo.DataExchanger.Conditions
{
    /// <summary>
    /// this is where 
    /// </summary>
    public class WhereInfo : IRunnable, IAddConditionSides
    {
        public IAddConditionSides Parent { get; set; }
        public Dictionary<string, object> PublicVariables { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsComplete { get; set; }
        /// <summary>
        /// all of the detected operators
        /// </summary>
        public List<OperatorKey> Operators { get; set; } = new List<OperatorKey>();
        /// <summary>
        /// name of variable of where for example var x from user.posts 'x' is variable name
        /// </summary>
        public string VariableName { get; set; }

        /// <summary>
        /// add a runnable to this where
        /// </summary>
        /// <param name="runnable"></param>
        public IAddConditionSides Add(IRunnable runnable)
        {
            OperatorInfo operatorInfo = new OperatorInfo()
            {
                Parent = this,
                PublicVariables = PublicVariables,
            };
            operatorInfo.Add(runnable);
            Operators.Add(new OperatorKey()
            {
                OperatorType = OperatorType.None,
                OperatorInfo = operatorInfo
            });
            return operatorInfo;
        }

        public void ChangeOperatorType(OperatorType operatorType)
        {
            OperatorKey findEmpty = Operators.FirstOrDefault(x => x.OperatorType == OperatorType.None);
            if (findEmpty == null)
                throw new Exception("before I found left side condition I found operator, this is not right code I think");
            findEmpty.OperatorType = operatorType;
        }

        public object Run(object newPoint)
        {
            bool value = true;
            IRunnable lastOperator = null;
            OperatorType lastOperatorType = OperatorType.None;
            foreach (OperatorKey item in Operators)
            {
                value = OperatorInfo.Compare(newPoint, lastOperator, item.OperatorInfo, lastOperatorType);
                lastOperator = item.OperatorInfo;
                lastOperatorType = item.OperatorType;
            }
            return value;
        }
    }
}
