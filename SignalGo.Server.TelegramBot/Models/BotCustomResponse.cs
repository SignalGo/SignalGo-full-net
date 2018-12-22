using System;
using System.Collections.Generic;
using System.Text;

namespace SignalGo.Server.TelegramBot.Models
{
    public class BotCustomResponse
    {
        public Action OnAfterComeplete { get; set; }
    }
}
