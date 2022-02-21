using System;

namespace SignalGo.Shared.DataTypes
{
    /// <summary>
    /// contract of method name or property name
    /// </summary>
    public class OperationContract : Attribute
    {
        public OperationContract(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
