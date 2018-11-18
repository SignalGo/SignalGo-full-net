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
        public void Add(IRunnable runnable)
        {
            Operators.Add(new OperatorKey()
            {
                OperatorType = OperatorType.None,
                OperatorInfo = runnable
            });
        }

        public void ChangeOperatorType(OperatorType operatorType)
        {
            OperatorKey findEmpty = Operators.FirstOrDefault(x=>x.OperatorType == OperatorType.None);
            if (findEmpty == null)
                throw new Exception("before I found left side condition I found operator, this is not right code I think");
            findEmpty.OperatorType = operatorType;
        }

        public object Run(object newPoint)
        {
            bool value = true;
            //OperatorInfo lastOperator = null;
            //foreach (OperatorKey item in Operators)
            //{
            //    value = OperatorInfo.Compare(newPoint, lastOperator, item.OperatorInfo, item.OperatorType);
            //    lastOperator = item.OperatorInfo;
            //}
            return value;
        }
    }
}
