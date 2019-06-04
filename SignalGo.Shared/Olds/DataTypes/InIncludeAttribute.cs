namespace SignalGo.Shared.DataTypes
{
    /// <summary>
    /// include properties of incoming calls from client to server
    /// include = take only exchange type
    /// </summary>
    public class InIncludeAttribute : CustomDataExchangerAttribute
    {
        public override LimitExchangeType LimitationMode { get; set; } = LimitExchangeType.IncomingCall;
        public override CustomDataExchangerType ExchangeType { get; set; } = CustomDataExchangerType.TakeOnly;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties">properties to include</param>
        public InIncludeAttribute(params string[] properties)
        {
            Properties = properties;
        }
    }
}
