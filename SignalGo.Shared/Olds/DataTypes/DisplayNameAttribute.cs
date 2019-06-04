using System;

namespace SignalGo.Shared.DataTypes
{
    public class ParameterDisplayNameAttribute : Attribute
    {
        public ParameterDisplayNameAttribute(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
    }
}
