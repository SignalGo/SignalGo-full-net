namespace SignalGo.DataExchanger.Conditions
{
    public interface IAddConditionSides : IRunnable
    {
        IAddConditionSides Parent { get; set; }
        bool IsComplete { get; set; }
        IAddConditionSides Add(IRunnable runnable);
        IAddConditionSides Add();
        void ChangeOperatorType(OperatorType operatorType);
    }
}
