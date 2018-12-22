using System;

namespace SignalGo.Shared.DataTypes
{
    public class DisplayNameAttribute : Attribute
    {
        public DisplayNameAttribute(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
    }
}
