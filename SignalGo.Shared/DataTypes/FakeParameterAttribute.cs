using System;

namespace SignalGo.Shared.DataTypes
{
    public enum FakeParameterType
    {
        None = 0,
        LastParameter = 1
    }

    public class FakeParameterAttribute : Attribute
    {
        public FakeParameterType Type { get; set; } = FakeParameterType.LastParameter;
    }
}
