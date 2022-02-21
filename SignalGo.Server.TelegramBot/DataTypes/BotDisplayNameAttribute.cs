using System;
using System.Collections.Generic;
using System.Linq;

namespace SignalGo.Server.TelegramBot.DataTypes
{
    public class BotKeyValuePair
    {
        public string Name { get; set; }
        public string Content { get; set; }
    }

    public class BotDisplayNameAttribute : Attribute
    {
        public BotDisplayNameAttribute(string content, params string[] available)
        {
            Content = content;
            Available = available;
        }

        public bool IsEnabled { get; set; } = true;
        public string Content { get; set; }
        public string[] Available { get; set; }

        public IEnumerable<BotKeyValuePair> GetBotKeyValuePairs()
        {
            for (int i = 0; i < Available.Length; i += 2)
            {
                yield return new BotKeyValuePair()
                {
                    Name = Available[i],
                    Content = Available[i + 1]
                };
            }
        }

        public string FindValue(string key)
        {
            var index = Array.FindIndex(Available, row => row.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
                return Available[index + 1];
            return null;
        }
    }
}
