using System;
using System.Collections.Generic;

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

        public static bool Compare(object newPoint, IRunnable leftSide, IRunnable rightSide, OperatorType operatorType)
        {
            //if that was first condition
            if (leftSide == null)
                return true;// (bool)rightSide.Run(newPoint);
            switch (operatorType)
            {
                //check 'and' condition
                case OperatorType.And:
                    {
                        return (bool)leftSide.Run(newPoint) && (bool)rightSide.Run(newPoint);
                    }
                //check 'or' condition
                case OperatorType.Or:
                    {
                        return (bool)leftSide.Run(newPoint) || (bool)rightSide.Run(newPoint);
                    }
                case OperatorType.Equal:
                    {
                        return Equals(leftSide.Run(newPoint), rightSide.Run(newPoint));
                    }
                case OperatorType.NotEqual:
                    {
                        return !Equals(leftSide.Run(newPoint), rightSide.Run(newPoint));
                    }
                case OperatorType.GreaterThan:
                    {
                        IComparable leftSideCompare = (IComparable)leftSide.Run(newPoint);
                        return leftSideCompare.CompareTo(rightSide.Run(newPoint)) == -1;
                    }
                case OperatorType.LessThan:
                    {
                        IComparable leftSideCompare = (IComparable)leftSide.Run(newPoint);
                        return leftSideCompare.CompareTo(rightSide.Run(newPoint)) == 1;
                    }
                case OperatorType.GreaterThanEqual:
                    {
                        IComparable leftSideCompare = (IComparable)leftSide.Run(newPoint);
                        int result = leftSideCompare.CompareTo(rightSide.Run(newPoint));
                        return result <= 0;
                    }
                case OperatorType.LessThanEqual:
                    {
                        IComparable leftSideCompare = (IComparable)leftSide.Run(newPoint);
                        int result = leftSideCompare.CompareTo(rightSide.Run(newPoint));
                        return result >= 0;
                    }
                default:
                    throw new Exception($"I cannot support {operatorType} operator");
            }
        }

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

        public void ChangeOperatorType(OperatorType operatorType)
        {
            Type = operatorType;
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
