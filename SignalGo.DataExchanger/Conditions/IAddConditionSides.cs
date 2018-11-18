namespace SignalGo.DataExchanger.Conditions
{
    public interface IAddConditionSides : IRunnable
    {
        IAddConditionSides Parent { get; set; }
        bool IsComplete { get; set; }
        IAddConditionSides Add(IRunnable runnable);
        void ChangeOperatorType(OperatorType operatorType);
    }
}
