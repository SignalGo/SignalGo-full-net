using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.DataTypes
{
    /// <summary>
    /// service contract is communicate services between client and server
    /// </summary>
    public class ServiceContractAttribute : Attribute
    {
        public ServiceContractAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
