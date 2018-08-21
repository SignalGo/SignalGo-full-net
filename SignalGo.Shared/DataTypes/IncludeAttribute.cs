namespace SignalGo.Shared.DataTypes
{
    public class IncludeAttribute : CustomDataExchangerAttribute
    {
        public override CustomDataExchangerType ExchangeType { get; set; } = CustomDataExchangerType.TakeOnly;
    }
}
