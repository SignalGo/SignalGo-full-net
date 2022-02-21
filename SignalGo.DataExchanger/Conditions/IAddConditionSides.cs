using System;

namespace SignalGo.DataExchanger.Conditions
{
    public interface IAddConditionSides : IRunnable
    {
        IAddConditionSides Parent { get; set; }
        bool IsComplete { get; set; }
        IAddConditionSides Add(IAddConditionSides runnable);
        Tuple<IAddConditionSides, IAddConditionSides> AddDouble(IAddConditionSides runnable);
        IAddConditionSides Add(IRunnable runnable);
        IAddConditionSides Add();
        void ChangeOperatorType(OperatorType operatorType);
    }
}
