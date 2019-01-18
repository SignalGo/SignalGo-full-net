namespace SignalGo.Server.TelegramBot.Models
{
    public enum BotResponseType : byte
    {
        None = 0,
        Text = 1
    }

    public class BotResponseInfoBase
    {
        public BotResponseType Type { get; private set; } = BotResponseType.Text;
        public string Message { get; set; }
    }

    public class BotResponseInfo<T> : BotResponseInfoBase
    {
        public T Response { get; private set; }

        public static implicit operator BotResponseInfo<T>(T response)
        {
            return new BotResponseInfo<T>()
            {
                Response = response,
                Message = response.ToString(),
            };
        }
    }
}
