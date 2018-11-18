namespace SignalGo.DataExchanger.Conditions
{
    public interface IAddConditionSides : IRunnable
    {
        bool IsComplete { get; set; }
        void Add(IRunnable runnable);
        void ChangeOperatorType(OperatorType operatorType);
    }
}
