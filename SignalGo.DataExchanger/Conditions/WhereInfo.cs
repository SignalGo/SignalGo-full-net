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

        /// <summary>
        /// add empty sides
        /// </summary>
        /// <returns></returns>
        public IAddConditionSides Add()
        {
            WhereInfo whereInfo = new WhereInfo()
            {
                Parent = this,
                PublicVariables = PublicVariables
            };
            Operators.Add(new OperatorKey()
            {
                OperatorType = OperatorType.None,
                OperatorInfo = whereInfo
            });
            return whereInfo;
        }

        public virtual IAddConditionSides Add(IAddConditionSides runnable)
        {
            throw new NotImplementedException();
        }

        public Tuple<IAddConditionSides, IAddConditionSides> AddDouble(IAddConditionSides runnable)
        {
            throw new NotImplementedException();
        }

        public void ChangeOperatorType(OperatorType operatorType)
        {
            OperatorKey findEmpty = Operators.FirstOrDefault(x => x.OperatorType == OperatorType.None);
            if (findEmpty == null)
                throw new Exception("before I found left side condition I found operator, this is not right code I think");
            findEmpty.OperatorType = operatorType;
        }

        public virtual object Run(object newPoint)
        {
            //periority check of opertators,
            //near will be check first
            List<OperatorType> operatorPeriority = new List<OperatorType>
            {
                OperatorType.Equal,
                OperatorType.GreaterThan,
                OperatorType.GreaterThanEqual,
                OperatorType.LessThan,
                OperatorType.LessThanEqual,
                OperatorType.NotEqual,
                OperatorType.And,
                OperatorType.Or,
                OperatorType.None
            };
            List<OperatorKey> operators = Operators.ToList();
            object value = null;
            do
            {
                OperatorType operatorToCheck = operatorPeriority.FirstOrDefault();
                if (operators.Count == 1 && value == null)
                {
                    value = operators.FirstOrDefault().OperatorInfo.Run(newPoint);
                    break;
                }
                foreach (OperatorKey leftItem in operators.Where(x => x.OperatorType == operatorToCheck).ToArray())
                {
                    int index = operators.IndexOf(leftItem);
                    OperatorKey sideItem = null;
                    if (index + 1 >= operators.Count)
                    {
                        sideItem = operators[index - 1];
                    }
                    else
                    {
                        sideItem = operators[index + 1];
                    }
                    value = OperatorInfo.Compare(newPoint, leftItem.OperatorInfo, sideItem.OperatorInfo, operatorToCheck);
                    operators.Remove(leftItem);
                    operators.Remove(sideItem);
                    if (operators.Count == 0)
                        break;
                    operators.Add(new OperatorKey() { OperatorType = sideItem.OperatorType, OperatorInfo = new ValueInfo() { Value = value } });
                }
                if (operators.Count(x => x.OperatorType == operatorToCheck) == 0)
                    operatorPeriority.Remove(operatorToCheck);
            }
            while (operatorPeriority.Count > 0);
            //OperatorType lastOperatorType = OperatorType.None;
            //foreach (OperatorKey item in Operators)
            //{
            //    lastValue = OperatorInfo.Compare(newPoint, lastValue, item.OperatorInfo, lastOperatorType);
            //    lastOperatorType = item.OperatorType;
            //}
            return value;
        }
    }
}
