using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.DataTypes
{
    public class HttpSupportAttribute : Attribute
    {
        public List<string> Addresses { get; set; } = new List<string>();
        public HttpSupportAttribute(string address)
        {
            Addresses.Add(address);
        }

        public HttpSupportAttribute(string[] addresses)
        {
            Addresses.AddRange(addresses);
        }
    }
}
