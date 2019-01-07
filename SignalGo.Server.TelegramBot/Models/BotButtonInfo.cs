using System;

namespace SignalGo.Server.TelegramBot.Models
{
    /// <summary>
    /// button of a bot level generating
    /// </summary>
    public class BotButtonInfo
    {
        public BotButtonInfo()
        {

        }

        public BotButtonInfo(string key)
        {
            Key = key;
        }

        public BotButtonInfo(string key, string caption)
        {
            Key = key;
            Caption = caption;
        }

        /// <summary>
        /// key or caption of bot
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// caption of button
        /// </summary>
        public string Caption { get; set; }
        /// <summary>
        /// service name
        /// </summary>
        public string ServiceName { get; set; }
        /// <summary>
        /// method name
        /// </summary>
        public string MethodName { get; set; }
        /// <summary>
        /// وقتی روی دکمه کلیک کرد
        /// </summary>
        public Action<TelegramClientInfo> Click { get; set; }

        public static implicit operator BotButtonInfo(string key)
        {
            return new BotButtonInfo(key);
        }
    }
}
