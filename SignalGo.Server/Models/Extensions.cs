using System.Collections.Generic;

namespace SignalGo.Server.Models
{
    public static class Extensions
    {
        public static void Add(this IDictionary<string, string[]> headers, string key, string value)
        {
            headers.Add(key, value.Split(','));
        }
    }
}
