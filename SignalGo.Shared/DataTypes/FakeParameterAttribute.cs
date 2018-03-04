using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
