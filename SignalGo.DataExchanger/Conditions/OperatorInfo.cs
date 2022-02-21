using System;
using System.Collections.Generic;
using System.Linq;

namespace SignalGo.DataExchanger.Conditions
{
    /// <summary>
    /// type of Operator like 'and' and 'or' '>' '<' etc
    /// </summary>
    public enum OperatorType : byte
    {
        None = 0,
        /// <summary>
        /// and to before
        /// </summary>
        And = 1,
        /// <summary>
        /// or to before
        /// </summary>
        Or = 2,
        /// <summary>
        /// = operator
        /// </summary>
        Equal = 3,
        /// <summary>
        /// > operator
        /// </summary>
        GreaterThan = 4,
        /// <summary>
        /// < operator
        /// </summary>
        LessThan = 5,
        /// <summary>
        /// != operator
        /// </summary>
        NotEqual = 6,
        /// <summary>
        /// >= operator
        /// </summary>
        GreaterThanEqual = 7,
        /// <summary>
        /// <= operator
        /// </summary>
        LessThanEqual = 8,
        /// <summary>
        /// sum operator
        /// </summary>
        Sum = 9,
    }

    /// <summary>
    /// operators like > < etc
    /// </summary>
    public class OperatorInfo : IRunnable, IAddConditionSides
    {
        public IAddConditionSides Parent { get; set; }
        public Dictionary<string, object> PublicVariables { get; set; }
        /// <summary>
        /// this is complete runnable left side and right side is full
        /// </summary>
        public bool IsComplete { get; set; }
        public static char[] OperatorStartChars { get; set; } = new char[] { '=', '>', '<', '!', '&', '|' };
        public static Dictionary<string, OperatorType> SupportedOperators { get; set; } = new Dictionary<string, OperatorType>()
        {
            {"=", OperatorType.Equal },
            {"==", OperatorType.Equal },
            {"and", OperatorType.And },
            {"&&", OperatorType.And },
            {"or", OperatorType.Or },
            {"||", OperatorType.Or },
            {">", OperatorType.GreaterThan },
            {">=", OperatorType.GreaterThanEqual },
            {"<", OperatorType.LessThan },
            {"<=", OperatorType.LessThanEqual },
            {"!=", OperatorType.NotEqual },
        };
        /// <summary>
        /// type of operator
        /// </summary>
        public OperatorType Type { get; set; }
        /// <summary>
        /// left side of condition
        /// </summary>
        public IRunnable LeftSideCondition { get; set; }
        /// <summary>
        /// right side of condition
        /// </summary>
        public IRunnable RightSideCondition { get; set; }


        /// <summary>
        /// run operator
        /// </summary>
        /// <param name="newPoint"></param>
        /// <returns></returns>
        public object Run(object newPoint)
        {
            return Compare(newPoint, LeftSideCondition, RightSideCondition, Type);
        }

        public static object Compare(object newPoint, IRunnable leftSide, IRunnable rightSide, OperatorType operatorType)
        {
            //if that was first condition
            if (leftSide == null)
                return true;// (bool)rightSide.Run(newPoint);
            return Compare(newPoint, leftSide.Run(newPoint), rightSide, operatorType);
        }

        public static object Compare(object newPoint, object lastCheckValue, IRunnable rightSide, OperatorType operatorType)
        {
            try
            {
                if (rightSide == null)
                    throw new Exception("I cannot found right side of condition please check if you are using empty parentheses just remove them");
                //if that was first condition
                //if (lastCheckValue == null)
                //    return true;// (bool)rightSide.Run(newPoint);
                object rightValue = rightSide.Run(newPoint);
                Type leftType = null;
                if (lastCheckValue != null)
                    leftType = lastCheckValue.GetType();
                switch (operatorType)
                {
                    //check 'and' condition
                    case OperatorType.And:
                        {
                            return (bool)lastCheckValue && (bool)rightValue;
                        }
                    //check 'or' condition
                    case OperatorType.Or:
                        {
                            return (bool)lastCheckValue || (bool)rightValue;
                        }
                    case OperatorType.Equal:
                        {
                            return Equals(lastCheckValue, ConvertType(leftType, rightValue));
                        }
                    case OperatorType.NotEqual:
                        {
                            return !Equals(lastCheckValue, ConvertType(leftType, rightValue));
                        }
                    case OperatorType.GreaterThan:
                        {
                            IComparable leftSideCompare = (IComparable)lastCheckValue;
                            return leftSideCompare.CompareTo(ConvertType(leftType, rightValue)) == 1;
                        }
                    case OperatorType.LessThan:
                        {
                            IComparable leftSideCompare = (IComparable)lastCheckValue;
                            return leftSideCompare.CompareTo(ConvertType(leftType, rightValue)) == -1;
                        }
                    case OperatorType.GreaterThanEqual:
                        {
                            IComparable leftSideCompare = (IComparable)lastCheckValue;
                            int result = leftSideCompare.CompareTo(ConvertType(leftType, rightValue));
                            return result >= 0;
                        }
                    case OperatorType.LessThanEqual:
                        {
                            IComparable leftSideCompare = (IComparable)lastCheckValue;
                            int result = leftSideCompare.CompareTo(ConvertType(leftType, rightValue));
                            return result <= 0;
                        }
                    default:
                        throw new Exception($"I cannot support {operatorType} operator");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// convert a type to new type for compare
        /// </summary>
        /// <param name="type"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public static object ConvertType(Type type, object newValue)
        {
            if (type != null && newValue.GetType() != type)
                return Convert.ChangeType(newValue, type);
            return newValue;
        }

        /// <summary>
        /// add side to operator
        /// </summary>
        /// <param name="runnable"></param>
        /// <returns></returns>
        public IAddConditionSides Add(IRunnable runnable)
        {
            if (IsComplete)
                throw new Exception("I found a problem, a condition is completed from right and left side but you are adding another side to this condition, check your query please");
            if (LeftSideCondition == null)
                LeftSideCondition = runnable;
            else
            {
                RightSideCondition = runnable;
                IsComplete = true;
                return Parent;
            }
            return this;
        }

        public IAddConditionSides Add()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// change the operator type
        /// </summary>
        /// <param name="operatorType"></param>
        public void ChangeOperatorType(OperatorType operatorType)
        {
            Type = operatorType;
        }

        public IAddConditionSides Add(IAddConditionSides runnable)
        {
            throw new NotImplementedException();
        }

        public Tuple<IAddConditionSides, IAddConditionSides> AddDouble(IAddConditionSides runnable)
        {
            throw new NotImplementedException();
        }
    }

    public class OperatorKey
    {
        /// <summary>
        /// operator type to compare with after himself
        /// </summary>
        public OperatorType OperatorType { get; set; }
        /// <summary>
        /// runnableInformarion
        /// </summary>
        public IRunnable OperatorInfo { get; set; }
    }
}
