using SignalGo.Server.Models;
using SignalGo.Shared.Models;
using System.Collections.Generic;
using Telegram.Bot.Types;

namespace SignalGo.Server.TelegramBot
{
    public class TelegramClientInfo : ClientInfo
    {
        public Message Message { get; set; }
        public string CurrentServiceName { get; set; }
        public string CurrentMethodName { get; set; }
        public string CurrentParameterName { get; set; }
        public List<ParameterInfo> ParameterInfoes { get; set; } = new List<ParameterInfo>();
    }
}
