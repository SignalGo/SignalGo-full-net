using SignalGo.Server.Models;
using SignalGo.Shared.Models;
using System.Collections.Generic;

namespace SignalGo.Server.TelegramBot
{
    public class TelegramClientInfo : ClientInfo
    {
        public string CurrentServiceName { get; set; }
        public string CurrentMethodName { get; set; }
        public string CurrentParameterName { get; set; }
        public List<ParameterInfo> ParameterInfoes { get; set; } = new List<ParameterInfo>();
    }
}
